using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Replica.Recruitment.Models;

namespace Replica.Recruitment.Services;

public sealed class SupabaseRecruitmentService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private Task<bool>? _refreshTask;

    public string SupabaseUrl { get; set; } = "https://donarysbdrbdaceackbe.supabase.co";
    public string AnonKey { get; set; } = "sb_publishable_1_tLxWerihARY1tUikn0sw_ZRI11NPd";
    public string? AuthToken { get; set; }
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Static API key (UUID) generated once on first login and stored locally.
    /// When present, used instead of OAuth refresh tokens for permanent silent reconnection.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// The Discord Snowflake ID associated with the API key (needed for the replica-auth Edge Function).
    /// </summary>
    public string? ApiKeyDiscordId { get; set; }

    public Action<string?, string?, string?, string?, string?, string?, DateTime?>? OnSessionUpdated { get; set; }
    public Func<bool>? ReloadTokensFromDisk { get; set; }

    public string? CurrentUserId { get; private set; }
    public string? CurrentDiscordId { get; private set; }
    public string? CurrentDiscordTag { get; private set; }
    public string? CurrentAvatarUrl { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }

    public bool IsAuthenticated => (!string.IsNullOrEmpty(AuthToken) || !string.IsNullOrEmpty(RefreshToken) || !string.IsNullOrEmpty(ApiKey)) && !string.IsNullOrEmpty(CurrentUserId);

    public bool IsLoading { get; private set; }
    public string? LastError { get; private set; }

    public SupabaseRecruitmentService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public void RestoreFromCache(string? authToken, string? refreshToken, string? userId, string? discordTag, string? discordId, string? avatarUrl, DateTime? expiresAt, string? apiKey = null)
    {
        AuthToken = authToken;
        RefreshToken = refreshToken;
        CurrentUserId = userId;
        CurrentDiscordTag = discordTag;
        CurrentDiscordId = discordId;
        CurrentAvatarUrl = avatarUrl;
        TokenExpiresAt = expiresAt;
        if (!string.IsNullOrEmpty(apiKey))
        {
            ApiKey = apiKey;
            ApiKeyDiscordId = discordId;
        }
    }

    private void NotifySessionUpdated()
    {
        OnSessionUpdated?.Invoke(AuthToken, RefreshToken, CurrentUserId, CurrentDiscordTag, CurrentDiscordId, CurrentAvatarUrl, TokenExpiresAt);
    }

    public async Task<bool> EnsureValidTokenAsync()
    {
        // If the token is still valid for 5+ minutes, nothing to do
        if (!string.IsNullOrEmpty(AuthToken) && TokenExpiresAt.HasValue && TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            return true;
        }

        // Prefer API key auth (permanent, never expires)
        if (!string.IsNullOrEmpty(ApiKey))
        {
            return await AuthWithApiKeyAsync();
        }

        if (ReloadTokensFromDisk != null && ReloadTokensFromDisk())
        {
            if (!string.IsNullOrEmpty(AuthToken) && TokenExpiresAt.HasValue && TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
            {
                return true;
            }
            if (!string.IsNullOrEmpty(ApiKey))
            {
                return await AuthWithApiKeyAsync();
            }
        }

        if (string.IsNullOrEmpty(RefreshToken))
        {
            return !string.IsNullOrEmpty(AuthToken);
        }

        // Fallback: OAuth refresh token
        return await RefreshTokenAsync();
    }

    private Task<bool>? _apiKeyAuthTask;

    /// <summary>
    /// Authenticates using the static API key via the replica-auth Supabase Edge Function.
    /// This never fails due to token rotation and provides permanent silent reconnection.
    /// </summary>
    public Task<bool> AuthWithApiKeyAsync()
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            Plugin.Log.Warning("[Replica] AuthWithApiKeyAsync called but ApiKey is missing.");
            return Task.FromResult(false);
        }

        lock (this)
        {
            if (_apiKeyAuthTask == null || _apiKeyAuthTask.IsCompleted)
            {
                _apiKeyAuthTask = AuthWithApiKeyAsyncInternal();
            }
            return _apiKeyAuthTask;
        }
    }

    private async Task<bool> AuthWithApiKeyAsyncInternal()
    {
        try
        {
            Plugin.Log.Information($"[Replica] Authenticating via static API key (key preview: {ApiKey[..Math.Min(8, ApiKey.Length)]}...)...");
            var edgeFunctionUrl = $"{SupabaseUrl.TrimEnd('/')}/functions/v1/replica-auth";
            var payload = new
            {
                api_key = ApiKey.Trim()
            };
            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            using var req = new HttpRequestMessage(HttpMethod.Post, edgeFunctionUrl);
            req.Headers.Add("apikey", AnonKey);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                Plugin.Log.Error($"[Replica] API key auth failed. Status: {res.StatusCode}, Body: {err}");
                return false;
            }

            var resJson = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("access_token", out var acc))
                AuthToken = acc.GetString();

            if (root.TryGetProperty("expires_in", out var expIn) && expIn.TryGetInt64(out var secs))
                TokenExpiresAt = DateTime.UtcNow.AddSeconds(secs);
            else
                TokenExpiresAt = DateTime.UtcNow.AddHours(1);

            if (root.TryGetProperty("user", out var userObj))
            {
                if (userObj.TryGetProperty("id", out var uid)) CurrentUserId = uid.GetString();
                if (userObj.TryGetProperty("discord_tag", out var tag)) CurrentDiscordTag = tag.GetString();
                if (userObj.TryGetProperty("discord_id", out var did)) CurrentDiscordId = did.GetString();
            }

            Plugin.Log.Information($"[Replica] API key auth succeeded. User: {CurrentDiscordTag}");
            NotifySessionUpdated();
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[Replica] AuthWithApiKeyAsync exception: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Calls the Supabase RPC get_or_create_replica_api_key using the current AuthToken.
    /// Returns the static API key and stores it in the service.
    /// </summary>
    public async Task<string?> FetchOrCreateApiKeyAsync()
    {
        if (string.IsNullOrEmpty(AuthToken))
        {
            Plugin.Log.Warning("[Replica] FetchOrCreateApiKeyAsync: missing AuthToken.");
            return null;
        }

        try
        {
            var rpcUrl = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/rpc/get_or_create_replica_api_key";
            var payload = new
            {
                p_discord_id = CurrentDiscordId,
                p_discord_tag = CurrentDiscordTag
            };
            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            using var req = new HttpRequestMessage(HttpMethod.Post, rpcUrl);
            req.Headers.Add("apikey", AnonKey);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                Plugin.Log.Warning($"[Replica] FetchOrCreateApiKeyAsync RPC failed. Status: {res.StatusCode}, Body: {err}");
                return null;
            }

            var resJson = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("api_key", out var keyProp))
            {
                var key = keyProp.GetString();
                if (!string.IsNullOrEmpty(key))
                {
                    ApiKey = key;
                    ApiKeyDiscordId = CurrentDiscordId ?? CurrentUserId;
                    Plugin.Log.Information($"[Replica] Static API key successfully fetched/created: {key[..Math.Min(8, key.Length)]}...");
                    return key;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[Replica] FetchOrCreateApiKeyAsync exception: {ex.Message}");
        }

        return null;
    }

    public Task<bool> RefreshTokenAsync()
    {
        // Prefer API key auth over fragile OAuth refresh tokens
        if (!string.IsNullOrEmpty(ApiKey))
        {
            return AuthWithApiKeyAsync();
        }

        lock (this)
        {
            if (_refreshTask == null || _refreshTask.IsCompleted)
            {
                _refreshTask = RefreshTokenAsyncInternal();
            }
            return _refreshTask;
        }
    }

    private async Task<bool> RefreshTokenAsyncInternal()
    {
        if (string.IsNullOrEmpty(RefreshToken)) return false;

        using var globalMutex = new System.Threading.Mutex(false, "Local\\ReplicaSupabaseAuthMutex");
        bool acquired = false;
        try
        {
            try
            {
                acquired = globalMutex.WaitOne(TimeSpan.FromSeconds(10));
            }
            catch (System.Threading.AbandonedMutexException)
            {
                acquired = true;
            }

            if (ReloadTokensFromDisk != null && ReloadTokensFromDisk())
            {
                if (!string.IsNullOrEmpty(AuthToken) && TokenExpiresAt.HasValue && TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
                {
                    return true;
                }
            }

            string? attemptedToken = RefreshToken?.Trim();
            if (string.IsNullOrEmpty(attemptedToken)) return false;

            Plugin.Log.Information($"[Replica] Sending token refresh request. Token Length: {attemptedToken?.Length ?? 0}");

            try
            {
                var payload = new { refresh_token = attemptedToken };
                var json = JsonSerializer.Serialize(payload, _jsonOptions);

                using var req = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseUrl.TrimEnd('/')}/auth/v1/token?grant_type=refresh_token");
                req.Headers.Add("apikey", AnonKey);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using var res = await _httpClient.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                {
                    string errContent = string.Empty;
                    try
                    {
                        errContent = await res.Content.ReadAsStringAsync();
                        Plugin.Log.Error($"[Replica] Token refresh failed. Status: {res.StatusCode}, Body: {errContent}");
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.Error($"[Replica] Token refresh failed. Status: {res.StatusCode}, could not read body: {ex.Message}");
                    }

                    if (res.StatusCode == System.Net.HttpStatusCode.BadRequest || res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        bool isInvalidGrant = false;
                        try
                        {
                            using var errDoc = JsonDocument.Parse(errContent);
                            var errRoot = errDoc.RootElement;
                            if (errRoot.TryGetProperty("error", out var errProp) && errProp.GetString() == "invalid_grant")
                            {
                                isInvalidGrant = true;
                            }
                            else if (errRoot.TryGetProperty("error_code", out var errCodeProp) && errCodeProp.GetString() == "validation_failed")
                            {
                                isInvalidGrant = true;
                            }
                            else if (errRoot.TryGetProperty("msg", out var msgProp) && msgProp.GetString() != null && msgProp.GetString().Contains("Refresh token is not valid"))
                            {
                                isInvalidGrant = true;
                            }
                        }
                        catch
                        {
                            // If response is not valid JSON, it might be a captive portal or proxy error
                        }

                        if (isInvalidGrant)
                        {
                            Plugin.Log.Error("[Replica] Token refresh failed (invalid token). Clearing session.");
                            if (ReloadTokensFromDisk != null && ReloadTokensFromDisk())
                            {
                                if (!string.IsNullOrEmpty(AuthToken) && TokenExpiresAt.HasValue && TokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5))
                                {
                                    return true;
                                }
                            }
                            ClearSession();
                        }
                    }
                    return false;
                }

                var resJson = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(resJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("access_token", out var acc))
                {
                    AuthToken = acc.GetString();
                }
                if (root.TryGetProperty("refresh_token", out var rToken))
                {
                    RefreshToken = rToken.GetString();
                }

                if (root.TryGetProperty("expires_in", out var expIn) && expIn.TryGetInt64(out var seconds))
                {
                    TokenExpiresAt = DateTime.UtcNow.AddSeconds(seconds);
                }
                else if (root.TryGetProperty("expires_at", out var expAt) && expAt.TryGetInt64(out var expUnix))
                {
                    TokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                }
                else
                {
                    TokenExpiresAt = DateTime.UtcNow.AddHours(1);
                }

                bool gotMetadata = false;
                if (root.TryGetProperty("user", out var userObj))
                {
                    if (userObj.TryGetProperty("id", out var uid))
                    {
                        CurrentUserId = uid.GetString();
                    }

                    if (userObj.TryGetProperty("user_metadata", out var meta))
                    {
                        if (meta.TryGetProperty("full_name", out var fn)) CurrentDiscordTag = fn.GetString();
                        else if (meta.TryGetProperty("name", out var n)) CurrentDiscordTag = n.GetString();
                        else if (meta.TryGetProperty("user_name", out var un)) CurrentDiscordTag = un.GetString();

                        if (meta.TryGetProperty("provider_id", out var pid)) CurrentDiscordId = pid.GetString();
                        else if (meta.TryGetProperty("sub", out var sub)) CurrentDiscordId = sub.GetString();

                        if (meta.TryGetProperty("avatar_url", out var av)) CurrentAvatarUrl = av.GetString();

                        gotMetadata = !string.IsNullOrEmpty(CurrentDiscordTag);
                    }
                }

                // Fallback: If user_metadata was omitted in the refresh token payload, query /auth/v1/user
                if (!gotMetadata && !string.IsNullOrEmpty(AuthToken))
                {
                    try
                    {
                        using var userReq = new HttpRequestMessage(HttpMethod.Get, $"{SupabaseUrl.TrimEnd('/')}/auth/v1/user");
                        userReq.Headers.Add("apikey", AnonKey);
                        userReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
                        using var userRes = await _httpClient.SendAsync(userReq);
                        if (userRes.IsSuccessStatusCode)
                        {
                            var uJson = await userRes.Content.ReadAsStringAsync();
                            using var uDoc = JsonDocument.Parse(uJson);
                            var uRoot = uDoc.RootElement;
                            if (uRoot.TryGetProperty("id", out var uid2)) CurrentUserId = uid2.GetString();
                            if (uRoot.TryGetProperty("user_metadata", out var uMeta))
                            {
                                if (uMeta.TryGetProperty("full_name", out var fn)) CurrentDiscordTag = fn.GetString();
                                else if (uMeta.TryGetProperty("name", out var n)) CurrentDiscordTag = n.GetString();
                                else if (uMeta.TryGetProperty("user_name", out var un)) CurrentDiscordTag = un.GetString();

                                if (uMeta.TryGetProperty("provider_id", out var pid)) CurrentDiscordId = pid.GetString();
                                else if (uMeta.TryGetProperty("sub", out var sub)) CurrentDiscordId = sub.GetString();

                                if (uMeta.TryGetProperty("avatar_url", out var av)) CurrentAvatarUrl = av.GetString();
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                CurrentDiscordTag ??= "DiscordUser";
                CurrentDiscordId ??= CurrentUserId;

                NotifySessionUpdated();
                return true;
            }
            catch
            {
                return false;
            }
        }
        finally
        {
            if (acquired)
            {
                globalMutex.ReleaseMutex();
            }
        }
    }

    public void ClearSession()
    {
        AuthToken = null;
        RefreshToken = null;
        CurrentUserId = null;
        CurrentDiscordTag = null;
        CurrentDiscordId = null;
        CurrentAvatarUrl = null;
        TokenExpiresAt = null;
        NotifySessionUpdated();
    }

    public async Task<bool> LoginWithTokenAsync(string? rawInput, string? explicitRefreshToken = null)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            string? token = rawInput?.Trim();
            string? refToken = explicitRefreshToken;

            if (!string.IsNullOrEmpty(token) && token.Contains("access_token="))
            {
                int start = token.IndexOf("access_token=", StringComparison.Ordinal) + "access_token=".Length;
                int end = token.IndexOf('&', start);
                string extractedToken = end > start ? token[start..end] : token[start..];

                if (token.Contains("refresh_token="))
                {
                    int rStart = token.IndexOf("refresh_token=", StringComparison.Ordinal) + "refresh_token=".Length;
                    int rEnd = token.IndexOf('&', rStart);
                    refToken = rEnd > rStart ? token[rStart..rEnd] : token[rStart..];
                }

                if (token.Contains("expires_in="))
                {
                    int eStart = token.IndexOf("expires_in=", StringComparison.Ordinal) + "expires_in=".Length;
                    int eEnd = token.IndexOf('&', eStart);
                    string expStr = eEnd > eStart ? token[eStart..eEnd] : token[eStart..];
                    if (long.TryParse(expStr, out var expSec))
                    {
                        TokenExpiresAt = DateTime.UtcNow.AddSeconds(expSec);
                    }
                }

                token = extractedToken;
            }

            if (!string.IsNullOrEmpty(refToken))
            {
                RefreshToken = refToken.Trim();
            }

            Plugin.Log.Information($"[Replica] Parsed tokens on login - AccessToken Length: {token?.Length ?? 0}, RefreshToken Length: {RefreshToken?.Length ?? 0}");

            // If token is empty but we have a RefreshToken, refresh directly
            if (string.IsNullOrWhiteSpace(token))
            {
                if (!string.IsNullOrEmpty(RefreshToken))
                {
                    return await RefreshTokenAsync();
                }

                LastError = "Empty token or URL.";
                return false;
            }

            // Test and fetch user profile via /auth/v1/user
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{SupabaseUrl.TrimEnd('/')}/auth/v1/user");
            req.Headers.Add("apikey", AnonKey);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                Plugin.Log.Error($"[Replica] Login /auth/v1/user call failed. Status: {res.StatusCode}, Body: {err}");

                if (!string.IsNullOrEmpty(RefreshToken))
                {
                    Plugin.Log.Information("[Replica] Trying to recover from login failure by refreshing token...");
                    bool refreshed = await RefreshTokenAsync();
                    if (refreshed)
                    {
                        Plugin.Log.Information("[Replica] Token refresh recovery succeeded after login failure.");
                        return true;
                    }
                }

                LastError = $"Authentication failed ({res.StatusCode}): {err}";
                return false;
            }

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            CurrentUserId = root.GetProperty("id").GetString();
            AuthToken = token;
            TokenExpiresAt ??= DateTime.UtcNow.AddHours(1);

            if (root.TryGetProperty("user_metadata", out var meta))
            {
                if (meta.TryGetProperty("full_name", out var fn)) CurrentDiscordTag = fn.GetString();
                else if (meta.TryGetProperty("name", out var n)) CurrentDiscordTag = n.GetString();
                else if (meta.TryGetProperty("user_name", out var un)) CurrentDiscordTag = un.GetString();

                if (meta.TryGetProperty("provider_id", out var pid)) CurrentDiscordId = pid.GetString();
                else if (meta.TryGetProperty("sub", out var sub)) CurrentDiscordId = sub.GetString();

                if (meta.TryGetProperty("avatar_url", out var av)) CurrentAvatarUrl = av.GetString();
            }

            CurrentDiscordTag ??= "DiscordUser";
            CurrentDiscordId ??= CurrentUserId;

            NotifySessionUpdated();
            return true;
        }
        catch (Exception ex)
        {
            LastError = $"Login error: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, string? jsonBody = null)
    {
        var url = $"{SupabaseUrl.TrimEnd('/')}/rest/v1/{endpoint.TrimStart('/')}";
        var req = new HttpRequestMessage(method, url);
        req.Headers.Add("apikey", AnonKey);

        if (!string.IsNullOrEmpty(AuthToken))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
        }
        else
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AnonKey);
        }

        if (jsonBody != null)
        {
            req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        return req;
    }

    private async Task<HttpResponseMessage> SendWithAutoRefreshAsync(Func<HttpRequestMessage> requestFactory)
    {
        await EnsureValidTokenAsync();

        var req = requestFactory();
        var res = await _httpClient.SendAsync(req);

        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized || res.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            Plugin.Log.Warning($"[Replica] Request to {req.RequestUri} failed with {res.StatusCode}. Attempting token refresh...");
            if (!string.IsNullOrEmpty(RefreshToken))
            {
                bool refreshed = await RefreshTokenAsync();
                if (refreshed)
                {
                    Plugin.Log.Information("[Replica] Token refresh succeeded. Retrying request...");
                    var retryReq = requestFactory();
                    res = await _httpClient.SendAsync(retryReq);
                }
                else
                {
                    Plugin.Log.Error("[Replica] Token refresh failed during retry attempt.");
                }
            }
        }

        if (!res.IsSuccessStatusCode)
        {
            try
            {
                string body = await res.Content.ReadAsStringAsync();
                Plugin.Log.Error($"[Replica] Request to {req.RequestUri} failed. Status: {res.StatusCode}, Body: {body}");
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[Replica] Request to {req.RequestUri} failed. Status: {res.StatusCode}, could not read body: {ex.Message}");
            }
        }

        return res;
    }

    public async Task<List<RecruitmentListing>> GetListingsAsync(string? contentType = null, string? region = null, string? datacenter = null, string? role = null)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            var queryParams = new List<string>
            {
                "select=*,applications(status)",
                "status=eq.OPEN",
                $"expires_at=gt.{Uri.EscapeDataString(DateTime.UtcNow.ToString("o"))}",
                "order=bumped_at.desc"
            };

            if (!string.IsNullOrEmpty(contentType) && contentType != "All Content")
            {
                queryParams.Add($"content_type=eq.{Uri.EscapeDataString(contentType)}");
            }

            if (!string.IsNullOrEmpty(region) && region != "All Regions")
            {
                queryParams.Add($"region=eq.{Uri.EscapeDataString(region)}");
            }

            if (!string.IsNullOrEmpty(datacenter) && datacenter != "All DCs")
            {
                queryParams.Add($"datacenter=eq.{Uri.EscapeDataString(datacenter)}");
            }

            if (!string.IsNullOrEmpty(role) && role != "All Roles")
            {
                queryParams.Add($"roles_needed=cs.{{{Uri.EscapeDataString(role)}}}");
            }

            var endpoint = $"listings?{string.Join("&", queryParams)}";
            using var res = await SendWithAutoRefreshAsync(() => CreateRequest(HttpMethod.Get, endpoint));

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                LastError = $"HTTP {res.StatusCode}: {err}";
                return [];
            }

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<RecruitmentListing>>(json, _jsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> CreateListingAsync(RecruitmentListing listing)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            var json = JsonSerializer.Serialize(listing, _jsonOptions);
            using var res = await SendWithAutoRefreshAsync(() =>
            {
                var req = CreateRequest(HttpMethod.Post, "listings", json);
                req.Headers.Add("Prefer", "return=representation");
                return req;
            });

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();

                // If remote database is missing author_aetherphone or sharing columns (PGRST204), fallback to standard columns
                if (err.Contains("PGRST204") || err.Contains("author_aetherphone") || err.Contains("share_discord_on_accept"))
                {
                    var fallbackPayload = new
                    {
                        id = listing.Id,
                        user_id = listing.UserId,
                        author_discord_id = listing.AuthorDiscordId,
                        author_discord_tag = listing.AuthorDiscordTag,
                        author_display_name = listing.AuthorDisplayName,
                        author_avatar_url = listing.AuthorAvatarUrl,
                        content_type = listing.ContentType,
                        target_duty = listing.TargetDuty,
                        region = listing.Region,
                        datacenter = listing.Datacenter,
                        languages = listing.Languages,
                        progression = listing.Progression,
                        schedule_days = listing.ScheduleDays,
                        schedule_time_start = listing.ScheduleTimeStart,
                        schedule_time_end = listing.ScheduleTimeEnd,
                        schedule_timezone = listing.ScheduleTimezone,
                        roles_needed = listing.RolesNeeded,
                        current_roster = listing.CurrentRoster,
                        tags = listing.Tags,
                        description = listing.Description,
                        status = listing.Status,
                        created_at = listing.CreatedAt,
                        updated_at = listing.UpdatedAt,
                        bumped_at = listing.BumpedAt,
                        expires_at = listing.ExpiresAt
                    };

                    var fallbackJson = JsonSerializer.Serialize(fallbackPayload, _jsonOptions);
                    using var fallbackRes = await SendWithAutoRefreshAsync(() =>
                    {
                        var fallbackReq = CreateRequest(HttpMethod.Post, "listings", fallbackJson);
                        fallbackReq.Headers.Add("Prefer", "return=representation");
                        return fallbackReq;
                    });

                    if (fallbackRes.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    var fallbackErr = await fallbackRes.Content.ReadAsStringAsync();
                    LastError = $"Failed to create listing ({fallbackRes.StatusCode}): {fallbackErr}";
                    return false;
                }

                LastError = $"Failed to create listing ({res.StatusCode}): {err}";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> CloseListingAsync(string listingId)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            var payload = new
            {
                status = "CLOSED",
                updated_at = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            using var res = await SendWithAutoRefreshAsync(() =>
            {
                var req = CreateRequest(HttpMethod.Patch, $"listings?id=eq.{listingId}", json);
                req.Headers.Add("Prefer", "return=minimal");
                return req;
            });

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                LastError = $"Close listing error ({res.StatusCode}): {err}";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> UpdateListingAsync(RecruitmentListing listing)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            var payload = new
            {
                author_display_name = listing.AuthorDisplayName,
                author_aetherphone = listing.AuthorAetherphone,
                share_discord_on_accept = listing.ShareDiscordOnAccept,
                share_aetherphone_on_accept = listing.ShareAetherphoneOnAccept,
                content_type = listing.ContentType,
                target_duty = listing.TargetDuty,
                region = listing.Region,
                datacenter = listing.Datacenter,
                languages = listing.Languages,
                progression = listing.Progression,
                schedule_days = listing.ScheduleDays,
                schedule_time_start = listing.ScheduleTimeStart,
                schedule_time_end = listing.ScheduleTimeEnd,
                schedule_timezone = listing.ScheduleTimezone,
                roles_needed = listing.RolesNeeded,
                current_roster = listing.CurrentRoster,
                tags = listing.Tags,
                description = listing.Description,
                status = listing.Status,
                updated_at = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            using var res = await SendWithAutoRefreshAsync(() =>
            {
                var req = CreateRequest(HttpMethod.Patch, $"listings?id=eq.{listing.Id}", json);
                req.Headers.Add("Prefer", "return=minimal");
                return req;
            });

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();

                if (err.Contains("PGRST204") || err.Contains("author_aetherphone") || err.Contains("share_discord_on_accept"))
                {
                    var fallbackPayload = new
                    {
                        author_display_name = listing.AuthorDisplayName,
                        content_type = listing.ContentType,
                        target_duty = listing.TargetDuty,
                        region = listing.Region,
                        datacenter = listing.Datacenter,
                        languages = listing.Languages,
                        progression = listing.Progression,
                        schedule_days = listing.ScheduleDays,
                        schedule_time_start = listing.ScheduleTimeStart,
                        schedule_time_end = listing.ScheduleTimeEnd,
                        schedule_timezone = listing.ScheduleTimezone,
                        roles_needed = listing.RolesNeeded,
                        current_roster = listing.CurrentRoster,
                        tags = listing.Tags,
                        description = listing.Description,
                        status = listing.Status,
                        updated_at = DateTime.UtcNow
                    };

                    var fallbackJson = JsonSerializer.Serialize(fallbackPayload, _jsonOptions);
                    using var fallbackRes = await SendWithAutoRefreshAsync(() =>
                    {
                        var fallbackReq = CreateRequest(HttpMethod.Patch, $"listings?id=eq.{listing.Id}", fallbackJson);
                        fallbackReq.Headers.Add("Prefer", "return=minimal");
                        return fallbackReq;
                    });

                    if (fallbackRes.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    var fallbackErr = await fallbackRes.Content.ReadAsStringAsync();
                    LastError = $"Update error ({fallbackRes.StatusCode}): {fallbackErr}";
                    return false;
                }

                LastError = $"Update error ({res.StatusCode}): {err}";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> DeleteListingAsync(string listingId)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            using var res = await SendWithAutoRefreshAsync(() => CreateRequest(HttpMethod.Delete, $"listings?id=eq.{listingId}"));
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                LastError = $"Delete listing error ({res.StatusCode}): {err}";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> DeleteApplicationAsync(string appId)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            using var res = await SendWithAutoRefreshAsync(() => CreateRequest(HttpMethod.Delete, $"applications?id=eq.{appId}"));
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<CandidateProfile?> GetProfileAsync(string? userId = null, string? discordId = null)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            string endpoint;
            if (!string.IsNullOrEmpty(userId))
            {
                endpoint = $"candidate_profiles?user_id=eq.{userId}&select=*";
            }
            else if (!string.IsNullOrEmpty(discordId))
            {
                endpoint = $"candidate_profiles?discord_id=eq.{discordId}&select=*";
            }
            else
            {
                return null;
            }

            using var res = await SendWithAutoRefreshAsync(() => CreateRequest(HttpMethod.Get, endpoint));

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                LastError = $"Fetch profile error: {err}";
                return null;
            }

            var json = await res.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<CandidateProfile>>(json, _jsonOptions);
            return (list != null && list.Count > 0) ? list[0] : null;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> SaveProfileAsync(CandidateProfile profile)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            if (string.IsNullOrEmpty(CurrentUserId) && string.IsNullOrEmpty(profile.UserId))
            {
                LastError = "You must link your Discord in the 'Account / Discord' tab before saving your profile.";
                return false;
            }

            profile.UserId ??= CurrentUserId;
            profile.DiscordId = CurrentDiscordId ?? profile.DiscordId;
            profile.DiscordTag = CurrentDiscordTag ?? profile.DiscordTag;
            profile.AvatarUrl = CurrentAvatarUrl ?? profile.AvatarUrl;
            profile.UpdatedAt = DateTime.UtcNow;

            var payload = new
            {
                user_id = profile.UserId,
                discord_id = profile.DiscordId,
                discord_tag = profile.DiscordTag,
                display_name = profile.DisplayName,
                aetherphone = profile.Aetherphone,
                share_discord_on_accept = profile.ShareDiscordOnAccept,
                share_aetherphone_on_accept = profile.ShareAetherphoneOnAccept,
                avatar_url = profile.AvatarUrl,
                character_name = profile.CharacterName,
                character_world = profile.CharacterWorld,
                character_datacenter = profile.CharacterDatacenter,
                character_region = profile.CharacterRegion,
                ilvl = profile.Ilvl,
                languages = profile.Languages,
                regions_accepted = profile.RegionsAccepted,
                main_jobs = profile.MainJobs,
                secondary_jobs = profile.SecondaryJobs,
                plugins_used = profile.PluginsUsed,
                available_days = profile.AvailableDays,
                preferred_time_start = profile.PreferredTimeStart,
                preferred_time_end = profile.PreferredTimeEnd,
                nights_per_week = profile.NightsPerWeek,
                experience = profile.Experience,
                about_me = profile.AboutMe,
                link_fflogs = profile.LinkFflogs,
                link_tomestone = profile.LinkTomestone,
                link_lodestone = profile.LinkLodestone,
                updated_at = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            // PostgREST Upsert with on_conflict=user_id
            using var res = await SendWithAutoRefreshAsync(() =>
            {
                var req = CreateRequest(HttpMethod.Post, "candidate_profiles?on_conflict=user_id", json);
                req.Headers.Add("Prefer", "resolution=merge-duplicates,return=representation");
                return req;
            });

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();

                if (err.Contains("PGRST204") || err.Contains("aetherphone") || err.Contains("share_discord_on_accept"))
                {
                    var fallbackPayload = new
                    {
                        user_id = profile.UserId,
                        discord_id = profile.DiscordId,
                        discord_tag = profile.DiscordTag,
                        display_name = profile.DisplayName,
                        avatar_url = profile.AvatarUrl,
                        character_name = profile.CharacterName,
                        character_world = profile.CharacterWorld,
                        character_datacenter = profile.CharacterDatacenter,
                        character_region = profile.CharacterRegion,
                        ilvl = profile.Ilvl,
                        languages = profile.Languages,
                        regions_accepted = profile.RegionsAccepted,
                        main_jobs = profile.MainJobs,
                        secondary_jobs = profile.SecondaryJobs,
                        plugins_used = profile.PluginsUsed,
                        available_days = profile.AvailableDays,
                        preferred_time_start = profile.PreferredTimeStart,
                        preferred_time_end = profile.PreferredTimeEnd,
                        nights_per_week = profile.NightsPerWeek,
                        experience = profile.Experience,
                        about_me = profile.AboutMe,
                        link_fflogs = profile.LinkFflogs,
                        link_tomestone = profile.LinkTomestone,
                        link_lodestone = profile.LinkLodestone,
                        updated_at = DateTime.UtcNow
                    };

                    var fallbackJson = JsonSerializer.Serialize(fallbackPayload, _jsonOptions);
                    using var fallbackRes = await SendWithAutoRefreshAsync(() =>
                    {
                        var fallbackReq = CreateRequest(HttpMethod.Post, "candidate_profiles?on_conflict=user_id", fallbackJson);
                        fallbackReq.Headers.Add("Prefer", "resolution=merge-duplicates,return=representation");
                        return fallbackReq;
                    });

                    if (fallbackRes.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    var fallbackErr = await fallbackRes.Content.ReadAsStringAsync();
                    LastError = $"Save profile error ({fallbackRes.StatusCode}): {fallbackErr}";
                    return false;
                }

                LastError = $"Save profile error ({res.StatusCode}): {err}";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> ApplyToListingAsync(string listingId, string appliedJob, string appliedRole, string customMessage, CandidateProfile profileSnapshot)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            var applicantId = CurrentUserId ?? (!string.IsNullOrEmpty(profileSnapshot.UserId) ? profileSnapshot.UserId : (!string.IsNullOrEmpty(profileSnapshot.DiscordTag) ? profileSnapshot.DiscordTag : profileSnapshot.DisplayName));
            if (string.IsNullOrEmpty(applicantId))
            {
                applicantId = "anonymous_applicant";
            }

            var payload = new
            {
                listing_id = listingId,
                applicant_user_id = applicantId,
                applicant_profile_snapshot = profileSnapshot,
                applied_as_job = appliedJob,
                applied_as_role = appliedRole,
                custom_message = customMessage,
                status = "PENDING",
                updated_at = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            using var res = await SendWithAutoRefreshAsync(() =>
            {
                var req = CreateRequest(HttpMethod.Post, "applications?on_conflict=listing_id,applicant_user_id", json);
                req.Headers.Add("Prefer", "resolution=merge-duplicates,return=representation");
                return req;
            });

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                LastError = $"Application failed ({res.StatusCode}): {err}";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<List<ApplicationItem>> GetApplicationsForListingAsync(string listingId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(listingId)) return [];

            IsLoading = true;
            using var res = await SendWithAutoRefreshAsync(() => CreateRequest(HttpMethod.Get, $"applications?listing_id=eq.{listingId}&select=*&order=created_at.desc"));

            if (!res.IsSuccessStatusCode)
            {
                return [];
            }

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ApplicationItem>>(json, _jsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<List<ApplicationItem>> GetMyApplicationsAsync(string? fallbackId = null)
    {
        try
        {
            var targetId = CurrentUserId ?? fallbackId;
            if (string.IsNullOrEmpty(targetId))
            {
                return [];
            }

            IsLoading = true;
            using var res = await SendWithAutoRefreshAsync(() => CreateRequest(HttpMethod.Get, $"applications?applicant_user_id=eq.{targetId}&select=*&order=created_at.desc"));

            if (!res.IsSuccessStatusCode)
            {
                return [];
            }

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ApplicationItem>>(json, _jsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> UpdateApplicationStatusAsync(string applicationId, string newStatus)
    {
        try
        {
            IsLoading = true;
            LastError = null;

            var payload = new { status = newStatus, updated_at = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            using var res = await SendWithAutoRefreshAsync(() => CreateRequest(HttpMethod.Patch, $"applications?id=eq.{applicationId}", json));
            return res.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
