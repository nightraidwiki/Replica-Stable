using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Excel.Sheets;
using Replica.Recruitment.Models;
using Replica.Recruitment.Services;

namespace Replica.Windows;

public sealed class RecruitmentView : IDisposable
{
    private readonly Plugin _plugin;
    public readonly SupabaseRecruitmentService Service;

    // Job icons cache
    private readonly Dictionary<string, ISharedImmediateTexture> _jobIconTextures = new();

    // View state: 0 = Browse Listings, 1 = My Profile, 2 = My Listings & Applications, 3 = Account / Discord
    private int _selectedSubTab = 0;
    private string _searchText = string.Empty;
    private int _filterContentTypeIdx = 0;
    private int _filterRegionIdx = 0;
    private int _filterRoleIdx = 0;
    private int _filterLanguageIdx = 0;

    // Listings cache
    private List<RecruitmentListing> _cachedListings = [];
    private RecruitmentListing? _selectedListingDetails;
    private bool _showApplyModal = false;
    private string _applyMessage = string.Empty;
    private string _applySelectedJob = string.Empty;
    private string _applySelectedRole = string.Empty;
    private string _applyStatusMessage = string.Empty;

    // Local profile editing
    private CandidateProfile _editingProfile = new();
    private string _profileStatusMessage = string.Empty;

    // Applications state
    private List<ApplicationItem> _mySentApplications = [];
    private Dictionary<string, List<ApplicationItem>> _receivedApplicationsByListing = new();
    private bool _isLoadingApplications = false;
    private readonly HashSet<string> _expandedApplicationIds = [];
    private readonly HashSet<string> _listingConfirmingCloseIds = [];

    // Create / Edit listing modal / form state
    private const int MaxListingsPerUser = 2;
    private bool _showCreatePfModal = false;
    private bool _isEditingPf = false;
    private string _inputTagsString = "Discord Voice, Standard Pastebin Strats, Serious Environment";
    private RecruitmentListing _newListing = new();
    private string _createStatusMessage = string.Empty;
    private string _inputRedirectUrlOrToken = string.Empty;
    private string _authStatusMessage = string.Empty;

    // Notification & polling state
    private readonly Dictionary<string, string> _knownApplicationStatuses = [];
    private readonly HashSet<string> _knownReceivedApplicationIds = [];
    private bool _isFirstNotificationPoll = true;
    private bool _isPolling = false;

    private bool SessionFileExists()
    {
        return System.IO.File.Exists(GetSessionFilePath());
    }

    private string GetSessionFilePath()
    {
        return System.IO.Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "replica_session.json");
    }

    private ReplicaSession? LoadSessionFromDisk()
    {
        try
        {
            string path = GetSessionFilePath();
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                return System.Text.Json.JsonSerializer.Deserialize<ReplicaSession>(json);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[Replica] Error loading session file: {ex.Message}");
        }
        return null;
    }

    private void SaveSessionToDisk(string? authToken, string? refreshToken, string? userId, string? discordTag, string? discordId, string? avatarUrl, DateTime? expiresAt, string? apiKey = null)
    {
        try
        {
            // Preserve existing ApiKey from service if not explicitly provided
            string? keyToSave = apiKey ?? Service.ApiKey;

            var session = new ReplicaSession
            {
                AuthToken = authToken,
                RefreshToken = refreshToken,
                UserId = userId,
                DiscordTag = discordTag,
                DiscordId = discordId,
                DiscordAvatarUrl = avatarUrl,
                TokenExpiresAt = expiresAt.HasValue ? new DateTimeOffset(expiresAt.Value).ToUnixTimeSeconds() : 0,
                ApiKey = keyToSave
            };
            string path = GetSessionFilePath();
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            string json = System.Text.Json.JsonSerializer.Serialize(session);
            System.IO.File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[Replica] Error saving session file: {ex.Message}");
        }
    }

    private void DeleteSessionFromDisk()
    {
        try
        {
            string path = GetSessionFilePath();
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[Replica] Error deleting session file: {ex.Message}");
        }
    }

    public RecruitmentView(Plugin plugin)
    {
        _plugin = plugin;
        Service = new SupabaseRecruitmentService
        {
            SupabaseUrl = _plugin.Configuration.SupabaseUrl,
            AnonKey = _plugin.Configuration.SupabaseAnonKey
        };

        Service.ReloadTokensFromDisk = () =>
        {
            try
            {
                var diskSession = LoadSessionFromDisk();
                if (diskSession != null && !string.IsNullOrEmpty(diskSession.RefreshToken) && diskSession.RefreshToken != Service.RefreshToken)
                {
                    DateTime? diskExpiresAt = diskSession.TokenExpiresAt > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(diskSession.TokenExpiresAt).UtcDateTime
                        : null;

                    Service.RestoreFromCache(
                        diskSession.AuthToken,
                        diskSession.RefreshToken,
                        diskSession.UserId,
                        diskSession.DiscordTag,
                        diskSession.DiscordId ?? diskSession.UserId,
                        diskSession.DiscordAvatarUrl,
                        diskExpiresAt
                    );
                    return true;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[Replica] Error reloading session from disk: {ex.Message}");
            }
            return false;
        };

        // Try to load from dedicated session file first, fallback to legacy configuration only if the session file does not exist
        bool sessionFileExists = SessionFileExists();
        var cachedSession = LoadSessionFromDisk();
        
        if (sessionFileExists)
        {
            if (cachedSession != null && (!string.IsNullOrEmpty(cachedSession.ApiKey) || !string.IsNullOrEmpty(cachedSession.RefreshToken)))
            {
                DateTime? expiresAt = cachedSession.TokenExpiresAt > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(cachedSession.TokenExpiresAt).UtcDateTime
                    : null;

                Service.RestoreFromCache(
                    cachedSession.AuthToken,
                    cachedSession.RefreshToken,
                    cachedSession.UserId,
                    cachedSession.DiscordTag,
                    cachedSession.DiscordId ?? cachedSession.UserId,
                    cachedSession.DiscordAvatarUrl,
                    expiresAt,
                    cachedSession.ApiKey
                );
            }
            // If session file exists but has no valid credentials (e.g. disconnected), we stay unauthenticated
        }
        else
        {
            DateTime? expiresAt = _plugin.Configuration.DiscordTokenExpiresAt > 0
                ? DateTimeOffset.FromUnixTimeSeconds(_plugin.Configuration.DiscordTokenExpiresAt).UtcDateTime
                : null;

            Service.RestoreFromCache(
                _plugin.Configuration.DiscordAuthToken,
                _plugin.Configuration.DiscordRefreshToken,
                _plugin.Configuration.DiscordUserId,
                _plugin.Configuration.DiscordTag,
                _plugin.Configuration.DiscordId ?? _plugin.Configuration.DiscordUserId,
                _plugin.Configuration.DiscordAvatarUrl,
                expiresAt,
                null // No ApiKey in legacy config, user will get one on next login
            );
        }

        Service.OnSessionUpdated = (authToken, refreshToken, userId, discordTag, discordId, avatarUrl, tokenExp) =>
        {
            _plugin.Configuration.DiscordAuthToken = authToken;
            _plugin.Configuration.DiscordRefreshToken = refreshToken;
            _plugin.Configuration.DiscordUserId = userId;
            _plugin.Configuration.DiscordId = discordId;
            _plugin.Configuration.DiscordTag = discordTag;
            _plugin.Configuration.DiscordAvatarUrl = avatarUrl;
            _plugin.Configuration.DiscordTokenExpiresAt = tokenExp.HasValue
                ? new DateTimeOffset(tokenExp.Value).ToUnixTimeSeconds()
                : 0;

            if (string.IsNullOrEmpty(refreshToken))
            {
                if (_authStatusMessage != "Disconnected.")
                {
                    _authStatusMessage = "Session expired. Please reconnect.";
                }
            }

            if (!string.IsNullOrEmpty(discordTag))
            {
                _editingProfile.DiscordTag = discordTag;
                _editingProfile.DiscordId = discordId ?? string.Empty;
                _editingProfile.UserId = userId;
                if (string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo))
                {
                    _plugin.Configuration.AccountPseudo = discordTag;
                }
                _editingProfile.DisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo)
                    ? _plugin.Configuration.AccountPseudo
                    : discordTag;
                
                _ = LoadProfileAsync();
            }

            _plugin.Configuration.Save();

            SaveSessionToDisk(authToken, refreshToken, userId, discordTag, discordId, avatarUrl, tokenExp);
        };

        InitDefaultProfile();
        InitDefaultNewListing();

        if (Service.IsAuthenticated)
        {
            _ = LoadProfileAsync();
        }

        _ = RefreshListingsAsync();
    }

    public bool ValidateContactSettings(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo))
        {
            errorMessage = "[!] Please set your Pseudo in the 'Account / Discord' tab (it is mandatory to create a PF or apply).";
            return false;
        }

        if (!_plugin.Configuration.ShareDiscordOnAccept && !_plugin.Configuration.ShareAetherphoneOnAccept)
        {
            errorMessage = "[!] You must enable at least one contact sharing option (Discord or Aetherphone) in the 'Account / Discord' tab.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private void InitDefaultProfile()
    {
        _editingProfile = new CandidateProfile
        {
            DisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) ? _plugin.Configuration.AccountPseudo : (!string.IsNullOrWhiteSpace(_plugin.Configuration.DiscordTag) ? _plugin.Configuration.DiscordTag : "My Raid Profile"),
            Aetherphone = _plugin.Configuration.AccountAetherphone,
            ShareDiscordOnAccept = _plugin.Configuration.ShareDiscordOnAccept,
            ShareAetherphoneOnAccept = _plugin.Configuration.ShareAetherphoneOnAccept,
            DiscordTag = !string.IsNullOrWhiteSpace(_plugin.Configuration.DiscordTag) ? _plugin.Configuration.DiscordTag : "Player#0001",
            DiscordId = !string.IsNullOrWhiteSpace(_plugin.Configuration.DiscordUserId) ? _plugin.Configuration.DiscordUserId : string.Empty,
            UserId = _plugin.Configuration.DiscordUserId,
            CharacterRegion = "EU",
            CharacterDatacenter = "Chaos",
            CharacterWorld = "Cerberus",
            Ilvl = 730,
            Languages = ["EN", "FR"],
            RegionsAccepted = ["EU"],
            MainJobs = ["PCT", "BLM", "RDM"],
            SecondaryJobs = [],
            PluginsUsed = new PluginsUsed
            {
                BossMod = true,
                Splatoon = true,
                Wrath = true,
                Rsr = false,
                Replica = true,
                Artisan = true,
                Cactbot = true,
                ModBeast = false
            },
            AvailableDays = ["Monday", "Tuesday", "Thursday", "Sunday"],
            PreferredTimeStart = "20:30",
            PreferredTimeEnd = "23:30",
            NightsPerWeek = "3-4",
            Experience = "M1S-M4S Week 1, DSR Clear patch 6.3, Prog FRU P4.",
            AboutMe = "Serious and punctual player, HQ food & pots always ready. Positive attitude and focused in raid."
        };
    }

    private void InitDefaultNewListing()
    {
        _newListing = new RecruitmentListing
        {
            ContentType = "Ultimate",
            TargetDuty = "Futures Rewritten (FRU)",
            Region = "EU",
            Datacenter = string.Empty,
            Languages = ["EN"],
            Progression = "P4 Phase 4 Transition",
            ScheduleDays = ["Tuesday", "Thursday", "Sunday"],
            ScheduleTimeStart = "20:45",
            ScheduleTimeEnd = "23:00",
            ScheduleTimezone = "Europe/Paris",
            RolesNeeded = ["ShieldHealer", "PhysRanged"],
            Tags = ["Discord Voice", "Standard Pastebin Strats", "Serious Environment"],
            Description = "FRU static looking for motivated Shield Healer and Phys Ranged to clear before next patch!",
            AuthorDisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) ? _plugin.Configuration.AccountPseudo : "Static Leader",
            AuthorAetherphone = _plugin.Configuration.AccountAetherphone,
            ShareDiscordOnAccept = _plugin.Configuration.ShareDiscordOnAccept,
            ShareAetherphoneOnAccept = _plugin.Configuration.ShareAetherphoneOnAccept,
            AuthorDiscordTag = "StaticLeader#0001"
        };
    }

    public void OpenSubTab(int tabIndex)
    {
        if (!Service.IsAuthenticated)
        {
            _selectedSubTab = 3;
            return;
        }

        _selectedSubTab = tabIndex;
        _listingConfirmingCloseIds.Clear();
        if (tabIndex == 2)
        {
            _ = RefreshMyApplicationsAndListingsAsync();
        }
    }

    public async System.Threading.Tasks.Task RefreshListingsAsync()
    {
        if (!Service.IsAuthenticated) return;

        var contentType = RecruitmentConstants.ContentTypes[_filterContentTypeIdx];
        var region = _filterRegionIdx > 0 ? RecruitmentConstants.Regions[_filterRegionIdx - 1] : null;
        var role = _filterRoleIdx > 0 ? RecruitmentConstants.Roles[_filterRoleIdx - 1] : null;

        _cachedListings = await Service.GetListingsAsync(contentType, region, null, role);
    }

    public async System.Threading.Tasks.Task PollNotificationsAsync()
    {
        if (_isPolling) return;
        _isPolling = true;
        try
        {
            if (!Service.IsAuthenticated) return;

            if (_cachedListings == null || _cachedListings.Count == 0)
            {
                await RefreshListingsAsync();
            }

            string myUserId = Service.CurrentUserId ?? string.Empty;
            string myPseudo = _plugin.Configuration.AccountPseudo;
            string myDiscordTag = _editingProfile.DiscordTag;

            // 1. Check Sent Applications (for "ACCEPTED" status)
            var sent = await Service.GetMyApplicationsAsync(myUserId);
            if (sent != null && sent.Count > 0)
            {
                foreach (var app in sent)
                {
                    if (_knownApplicationStatuses.TryGetValue(app.Id, out var oldStatus))
                    {
                        if (oldStatus != "ACCEPTED" && app.Status == "ACCEPTED")
                        {
                            var listing = _cachedListings?.FirstOrDefault(l => l.Id == app.ListingId);
                            string duty = listing?.TargetDuty ?? "Party Finder";
                            string leader = listing?.AuthorDisplayName ?? "Static Leader";

                            RecruitmentToastOverlay.AddToast(new RecruitmentToast
                            {
                                Kind = RecruitmentToastKind.ApplicationAccepted,
                                Title = "Application Accepted!",
                                Message = $"Your application for {duty} was ACCEPTED by {leader}! Contact coordinates are now unlocked.",
                                Icon = FontAwesomeIcon.CheckCircle,
                                OnOpen = () => _plugin.ShowRecruitment(2)
                            });
                        }
                    }
                    else if (app.Status == "ACCEPTED" && !_isFirstNotificationPoll)
                    {
                        var listing = _cachedListings?.FirstOrDefault(l => l.Id == app.ListingId);
                        string duty = listing?.TargetDuty ?? "Party Finder";
                        string leader = listing?.AuthorDisplayName ?? "Static Leader";

                        RecruitmentToastOverlay.AddToast(new RecruitmentToast
                        {
                            Kind = RecruitmentToastKind.ApplicationAccepted,
                            Title = "Application Accepted!",
                            Message = $"Your application for {duty} was ACCEPTED by {leader}! Contact coordinates are now unlocked.",
                            Icon = FontAwesomeIcon.CheckCircle,
                            OnOpen = () => _plugin.ShowRecruitment(2)
                        });
                    }
                    _knownApplicationStatuses[app.Id] = app.Status;
                }
            }

            // 2. Check Received Applications on My Published Listings
            var myListings = _cachedListings?.Where(l =>
                (!string.IsNullOrEmpty(myUserId) && l.UserId == myUserId) ||
                (!string.IsNullOrWhiteSpace(myDiscordTag) && l.AuthorDiscordTag == myDiscordTag) ||
                (!string.IsNullOrWhiteSpace(myPseudo) && l.AuthorDisplayName == myPseudo)).ToList() ?? [];

            foreach (var listing in myListings)
            {
                var received = await Service.GetApplicationsForListingAsync(listing.Id);
                if (received != null)
                {
                    _receivedApplicationsByListing[listing.Id] = received;
                    foreach (var app in received)
                    {
                        if (!_isFirstNotificationPoll && !_knownReceivedApplicationIds.Contains(app.Id))
                        {
                            string applicantPseudo = !string.IsNullOrWhiteSpace(app.ApplicantProfileSnapshot?.DisplayName)
                                ? app.ApplicantProfileSnapshot.DisplayName
                                : "A candidate";
                            string appliedRoleOrJob = !string.IsNullOrEmpty(app.AppliedAsJob) ? app.AppliedAsJob : app.AppliedAsRole;

                            RecruitmentToastOverlay.AddToast(new RecruitmentToast
                            {
                                Kind = RecruitmentToastKind.NewApplicationReceived,
                                Title = "New PF Application!",
                                Message = $"{applicantPseudo} applied for [{appliedRoleOrJob}] on your listing {listing.TargetDuty}!",
                                Icon = FontAwesomeIcon.UserPlus,
                                OnOpen = () => _plugin.ShowRecruitment(2)
                            });
                        }
                        _knownReceivedApplicationIds.Add(app.Id);
                    }
                }
            }

            _isFirstNotificationPoll = false;
        }
        catch
        {
            // Ignore transient background network issues
        }
        finally
        {
            _isPolling = false;
        }
    }

    public void Draw()
    {
        float scale = ImGuiHelpers.GlobalScale;
        float availWidth = ImGui.GetContentRegionAvail().X;

        // If not authenticated with Discord, force Account / Discord tab
        if (!Service.IsAuthenticated && _selectedSubTab != 3)
        {
            _selectedSubTab = 3;
        }



        // Header Nav Bar
        DrawTopNavBar(scale, availWidth);
        ImGui.Separator();
        ImGui.Spacing();

        // Main Tab Content
        switch (_selectedSubTab)
        {
            case 0:
                DrawBrowseTab(scale, availWidth);
                break;
            case 1:
                DrawProfileTab(scale, availWidth);
                break;
            case 2:
                DrawMyListingsTab(scale, availWidth);
                break;
            case 3:
                DrawAuthSettingsTab(scale, availWidth);
                break;
        }

        // Apply Modal Dialog
        if (_showApplyModal && _selectedListingDetails != null)
        {
            DrawApplyModal(scale);
        }

        // Create PF Modal Dialog
        if (_showCreatePfModal)
        {
            DrawCreatePfModal(scale);
        }
    }

    private void DrawTopNavBar(float scale, float availWidth)
    {
        ImGui.BeginGroup();

        void NavButton(string label, FontAwesomeIcon icon, int tabIndex)
        {
            bool isLocked = !Service.IsAuthenticated && tabIndex != 3;
            bool isSelected = _selectedSubTab == tabIndex;

            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f));
            }
            else if (isLocked)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.12f, 0.12f, 0.12f, 0.6f));
                ImGui.PushStyleColor(ImGuiCol.Text, Ui.Dimmed);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.16f, 0.16f, 0.16f, 1f));
                ImGui.PushStyleColor(ImGuiCol.Text, Ui.White);
            }

            FontAwesomeIcon displayIcon = isLocked ? FontAwesomeIcon.Lock : icon;
            string displayLabel = isLocked ? $"{label} (Locked)" : label;

            if (Ui.IconButton(displayIcon, displayLabel, $"nav_pf_{tabIndex}", new Vector2(0f, 32f * scale), scale))
            {
                if (isLocked)
                {
                    _selectedSubTab = 3;
                    _authStatusMessage = "Please link your Discord account in this tab to unlock Party Finder.";
                }
                else
                {
                    _selectedSubTab = tabIndex;
                    if (tabIndex == 2)
                    {
                        _ = RefreshMyApplicationsAndListingsAsync();
                    }
                }
            }

            if (isLocked && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("[Locked] Discord connection required to access this section.");
            }

            ImGui.PopStyleColor(2);
            ImGui.SameLine(0f, 6f * scale);
        }

        NavButton("Browse Listings", FontAwesomeIcon.Search, 0);
        NavButton("My Profile / Raid Resume", FontAwesomeIcon.User, 1);
        NavButton("My Listings & Applications", FontAwesomeIcon.FolderOpen, 2);
        NavButton("Account / Discord", FontAwesomeIcon.Comments, 3);

        // Status indicator on right
        float rightOffset = 180f * scale;
        if (availWidth > 600f)
        {
            ImGui.SameLine(availWidth - rightOffset);
            if (!Service.IsAuthenticated)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Ui.Gold);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Text(FontAwesomeIcon.Lock.ToIconString());
                ImGui.PopFont();
                ImGui.SameLine(0f, 4f * scale);
                ImGui.Text("Auth Required");
                ImGui.PopStyleColor();
            }
            else if (Service.IsLoading)
            {
                ImGui.TextColored(Ui.Gold, "Synchronizing...");
            }
            else if (!string.IsNullOrEmpty(Service.LastError) && (_cachedListings == null || _cachedListings.Count == 0))
            {
                ImGui.TextColored(Ui.Red, "API Error");
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Service.LastError);
                }
            }
            else
            {
                ImGui.TextColored(Ui.Green, "● Online");
            }
        }

        ImGui.EndGroup();
    }

    #region Tab 0: Browse Listings
    private void DrawBrowseTab(float scale, float availWidth)
    {
        // Filters header with "Create PF" button
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.12f, 0.12f, 0.12f, 0.6f));
        ImGui.BeginChild("##pf_filters", new Vector2(availWidth, 42f * scale), true, ImGuiWindowFlags.NoScrollbar);

        ImGui.SetNextItemWidth(130f * scale);
        if (ImGui.Combo("##content_type", ref _filterContentTypeIdx, RecruitmentConstants.ContentTypes, RecruitmentConstants.ContentTypes.Length))
        {
            _ = RefreshListingsAsync();
        }

        ImGui.SameLine(0f, 6f * scale);
        string[] regionsDisplay = ["All Regions", "EU", "NA", "JP", "OCE"];
        ImGui.SetNextItemWidth(100f * scale);
        if (ImGui.Combo("##region", ref _filterRegionIdx, regionsDisplay, regionsDisplay.Length))
        {
            _ = RefreshListingsAsync();
        }

        ImGui.SameLine(0f, 6f * scale);
        string[] rolesDisplay = ["All Roles Needed", .. RecruitmentConstants.RolesDisplay];
        ImGui.SetNextItemWidth(150f * scale);
        if (ImGui.Combo("##roles_filter", ref _filterRoleIdx, rolesDisplay, rolesDisplay.Length))
        {
            _ = RefreshListingsAsync();
        }

        ImGui.SameLine(0f, 6f * scale);
        string[] languagesDisplay = ["All Langs", .. RecruitmentConstants.SupportedLanguages];
        ImGui.SetNextItemWidth(90f * scale);
        ImGui.Combo("##languages_filter", ref _filterLanguageIdx, languagesDisplay, languagesDisplay.Length);

        ImGui.SameLine(0f, 6f * scale);
        ImGui.SetNextItemWidth(150f * scale);
        ImGui.InputTextWithHint("##search_txt", "Search duty, tag, author...", ref _searchText, 64);

        ImGui.SameLine(0f, 6f * scale);
        if (ImGui.Button("Refresh"))
        {
            _ = RefreshListingsAsync();
        }

        // Prominent Create PF Button
        ImGui.SameLine(0f, 10f * scale);
        int myListingCount = GetMyActiveListingCount();
        bool atLimit = myListingCount >= MaxListingsPerUser;
        if (atLimit)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.3f, 0.6f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.35f, 0.35f, 0.35f, 0.6f));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Ui.Accent);
        }
        if (ImGui.Button("Create PF###btn_create_pf", new Vector2(100f * scale, 0f)))
        {
            if (atLimit)
            {
                _createStatusMessage = $"You cannot create more than {MaxListingsPerUser} active listings. Please delete an existing one first.";
            }
            else
            {
                _isEditingPf = false;
                InitDefaultNewListing();
                _showCreatePfModal = true;
                _createStatusMessage = string.Empty;
            }
        }
        ImGui.PopStyleColor(2);
        if (atLimit)
        {
            ImGui.SameLine(0f, 8f * scale);
            ImGui.TextColored(Ui.Red, $"Limit: {myListingCount}/{MaxListingsPerUser} PF");
        }

        ImGui.EndChild();
        ImGui.PopStyleColor();

        ImGui.Spacing();

        // Listing Cards Grid or Details Split View
        if (_selectedListingDetails != null)
        {
            DrawListingFullDetails(_selectedListingDetails, scale, availWidth);
        }
        else
        {
            DrawListingCardsList(scale, availWidth);
        }
    }

    private void DrawListingCardsList(float scale, float availWidth)
    {
        var filtered = _cachedListings.Where(l =>
        {
            if (_filterLanguageIdx > 0 && _filterLanguageIdx - 1 < RecruitmentConstants.SupportedLanguages.Length)
            {
                var targetLang = RecruitmentConstants.SupportedLanguages[_filterLanguageIdx - 1];
                if (l.Languages == null || !l.Languages.Any(lang => string.Equals(lang, targetLang, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }

            if (string.IsNullOrWhiteSpace(_searchText)) return true;
            var term = _searchText.ToLowerInvariant();
            return l.TargetDuty.ToLowerInvariant().Contains(term)
                || l.Description.ToLowerInvariant().Contains(term)
                || l.AuthorDisplayName.ToLowerInvariant().Contains(term)
                || l.Tags.Any(t => t.ToLowerInvariant().Contains(term));
        }).ToList();

        if (filtered.Count == 0 && !Service.IsLoading)
        {
            ImGui.Dummy(new Vector2(0f, 30f * scale));
            ImGui.SetCursorPosX((availWidth - 320f * scale) * 0.5f);
            ImGui.BeginGroup();
            ImGui.TextColored(Ui.Dimmed, "No recruitment listings match your current filters.");
            if (ImGui.Button("Reset filters and show all", new Vector2(320f * scale, 30f * scale)))
            {
                _filterContentTypeIdx = 0;
                _filterRegionIdx = 0;
                _filterRoleIdx = 0;
                _filterLanguageIdx = 0;
                _searchText = string.Empty;
                _ = RefreshListingsAsync();
            }
            ImGui.EndGroup();
            return;
        }

        ImGui.BeginChild("##listings_scroll", new Vector2(availWidth, 0f), false);

        foreach (var listing in filtered)
        {
            DrawListingCard(listing, scale, availWidth - 16f * scale);
            ImGui.Spacing();
        }

        ImGui.EndChild();
    }

    private void DrawListingCard(RecruitmentListing listing, float scale, float width)
    {
        Vector2 cursor = ImGui.GetCursorScreenPos();
        float cardHeight = 110f * scale;
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        // Background
        drawList.AddRectFilled(cursor, cursor + new Vector2(width, cardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(0.14f, 0.14f, 0.14f, 0.95f)), 8f * scale);
        drawList.AddRect(cursor, cursor + new Vector2(width, cardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(Ui.Accent.X, Ui.Accent.Y, Ui.Accent.Z, 0.35f)), 8f * scale, ImDrawFlags.None, 1f);

        ImGui.BeginGroup();
        ImGui.SetCursorScreenPos(cursor + new Vector2(12f * scale, 10f * scale));

        // Duty Title, Region & Language Badges, and Author Pseudo
        ImGui.TextColored(Ui.White, listing.TargetDuty);
        ImGui.SameLine(0f, 10f * scale);
        ImGui.TextColored(Ui.Accent, $"[{listing.ContentType}]");
        ImGui.SameLine(0f, 6f * scale);
        ImGui.TextColored(Ui.Gold, $"[{listing.Region}]");
        if (listing.Languages != null && listing.Languages.Count > 0)
        {
            ImGui.SameLine(0f, 6f * scale);
            ImGui.TextColored(Ui.Blue, $"[{string.Join("/", listing.Languages)}]");
        }

        ImGui.SameLine(0f, 10f * scale);
        string authorPseudo = !string.IsNullOrWhiteSpace(listing.AuthorDisplayName) ? listing.AuthorDisplayName : listing.AuthorDiscordTag;
        ImGui.TextColored(Ui.Dimmed, $"by {authorPseudo}");

        int appCount = listing.Applications?.Count(a => a.Status == "PENDING" || a.Status == "ACCEPTED") ?? 0;
        ImGui.SameLine(0f, 10f * scale);
        ImGui.TextColored(Ui.Green, $"• {appCount} {(appCount == 1 ? "application" : "applications")}");

        // Progression & Schedule
        ImGui.SetCursorScreenPos(cursor + new Vector2(12f * scale, 32f * scale));
        ImGui.TextColored(Ui.Green, $"Progression: {listing.Progression}");
        ImGui.SameLine(0f, 16f * scale);
        ImGui.TextColored(Ui.White, $"Schedule: {FormatSchedule(listing)}");

        // Roles Needed Pills
        ImGui.SetCursorScreenPos(cursor + new Vector2(12f * scale, 54f * scale));
        ImGui.TextColored(Ui.White, "Roles needed: ");
        foreach (var role in listing.RolesNeeded)
        {
            ImGui.SameLine(0f, 4f * scale);
            ImGui.TextColored(Ui.Gold, $"[{role}]");
        }

        // Contact tags (Discord / Aetherphone) & General Tags
        ImGui.SetCursorScreenPos(cursor + new Vector2(12f * scale, 78f * scale));
        bool hasDiscord = listing.ShareDiscordOnAccept;
        bool hasPhone = listing.ShareAetherphoneOnAccept;

        if (hasDiscord)
        {
            ImGui.TextColored(Ui.DiscordColor, "[Discord]");
            ImGui.SameLine(0f, 6f * scale);
        }

        if (hasPhone)
        {
            ImGui.TextColored(Ui.AetherphoneColor, "[Aetherphone]");
            ImGui.SameLine(0f, 6f * scale);
        }

        if (listing.Tags != null)
        {
            foreach (var tag in listing.Tags.Where(t => !t.Equals("Discord", StringComparison.OrdinalIgnoreCase) && !t.Equals("Aetherphone", StringComparison.OrdinalIgnoreCase) && !t.Equals("DiscordTag", StringComparison.OrdinalIgnoreCase)))
            {
                ImGui.TextColored(Ui.White, $"[{tag}]");
                ImGui.SameLine(0f, 4f * scale);
            }
        }

        // Action Buttons on Right
        float btnWidth = 140f * scale;
        ImGui.SetCursorScreenPos(cursor + new Vector2(width - btnWidth - 12f * scale, 16f * scale));
        if (ImGui.Button($"View Details###view_{listing.Id}", new Vector2(btnWidth, 32f * scale)))
        {
            _selectedListingDetails = listing;
        }

        ImGui.SetCursorScreenPos(cursor + new Vector2(width - btnWidth - 12f * scale, 56f * scale));
        bool alreadyApplied = _mySentApplications.Any(a => a.ListingId == listing.Id);
        if (alreadyApplied)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGui.PushStyleColor(ImGuiCol.Button, Ui.Green);
            ImGui.Button($"Already Applied###apply_{listing.Id}", new Vector2(btnWidth, 32f * scale));
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
            if (ImGui.Button($"Quick Apply###apply_{listing.Id}", new Vector2(btnWidth, 32f * scale)))
            {
                _selectedListingDetails = listing;
                _applySelectedJob = string.Join(", ", _editingProfile.MainJobs.Concat(_editingProfile.SecondaryJobs).Distinct());
                _applySelectedRole = listing.RolesNeeded.FirstOrDefault() ?? "TankMT";
                _applyMessage = "Hello! Very interested in your listing. Available for a trial whenever convenient.";
                _applyStatusMessage = string.Empty;
                _showApplyModal = true;
            }
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();
        ImGui.SetCursorScreenPos(cursor + new Vector2(0f, cardHeight + 8f * scale));
    }

    private void DrawListingFullDetails(RecruitmentListing listing, float scale, float availWidth)
    {
        if (ImGui.Button("< Back to listings list", new Vector2(180f * scale, 28f * scale)))
        {
            _selectedListingDetails = null;
            return;
        }

        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.12f, 0.12f, 0.14f, 0.95f));
        ImGui.BeginChild("##listing_details_view", new Vector2(availWidth, 0f), true);

        ImGui.TextColored(Ui.White, listing.TargetDuty);
        ImGui.SameLine(0f, 10f * scale);
        ImGui.TextColored(Ui.Accent, $"[{listing.ContentType}]");
        ImGui.SameLine(0f, 6f * scale);
        ImGui.TextColored(Ui.Gold, $"[{listing.Region}]");
        if (listing.Languages != null && listing.Languages.Count > 0)
        {
            ImGui.SameLine(0f, 6f * scale);
            ImGui.TextColored(Ui.Blue, $"[{string.Join("/", listing.Languages)}]");
        }

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextColored(Ui.Green, "Progression:");
        ImGui.SameLine();
        ImGui.TextColored(Ui.White, listing.Progression);

        ImGui.TextColored(Ui.Gold, "Schedule & Times:");
        ImGui.SameLine();
        ImGui.TextColored(Ui.White, FormatSchedule(listing));

        ImGui.TextColored(Ui.Accent, "Recruiter / Author Pseudo:");
        ImGui.SameLine();
        string leaderPseudo = !string.IsNullOrWhiteSpace(listing.AuthorDisplayName) ? listing.AuthorDisplayName : listing.AuthorDiscordTag;
        ImGui.TextColored(Ui.White, leaderPseudo);

        ImGui.TextColored(Ui.Gold, "Active Applications:");
        ImGui.SameLine();
        int detailAppCount = listing.Applications?.Count(a => a.Status == "PENDING" || a.Status == "ACCEPTED") ?? 0;
        ImGui.TextColored(Ui.White, detailAppCount.ToString());

        ImGui.TextColored(Ui.Gold, "Contact Sharing (Upon Accepted Application):");
        ImGui.SameLine();
        if (!listing.ShareDiscordOnAccept && !listing.ShareAetherphoneOnAccept)
        {
            ImGui.TextColored(Ui.Dimmed, "[None]");
        }
        else
        {
            if (listing.ShareDiscordOnAccept)
            {
                ImGui.TextColored(Ui.DiscordColor, "[Discord]");
                if (listing.ShareAetherphoneOnAccept)
                {
                    ImGui.SameLine(0f, 4f * scale);
                    ImGui.TextColored(Ui.White, "&");
                    ImGui.SameLine(0f, 4f * scale);
                }
            }
            if (listing.ShareAetherphoneOnAccept)
            {
                ImGui.TextColored(Ui.AetherphoneColor, "[Aetherphone]");
            }
        }
        ImGui.SameLine(0f, 6f * scale);
        ImGui.TextColored(Ui.Green, "(Coordinates will be revealed once the application is accepted)");

        ImGui.Spacing();
        ImGui.TextColored(Ui.White, "Tags:");
        foreach (var tag in listing.Tags)
        {
            ImGui.SameLine(0f, 4f * scale);
            ImGui.TextColored(Ui.White, $"[{tag}]");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.TextColored(Ui.Gold, "Static Description:");
        ImGui.TextWrapped(listing.Description);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.TextColored(Ui.Gold, "Static Composition & Roles Needed:");
        foreach (var role in listing.RolesNeeded)
        {
            ImGui.BulletText($"Actively recruiting: {role}");
        }

        ImGui.Dummy(new Vector2(0f, 20f * scale));
        bool alreadyAppliedFull = _mySentApplications.Any(a => a.ListingId == listing.Id);
        if (alreadyAppliedFull)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.6f);
            ImGui.PushStyleColor(ImGuiCol.Button, Ui.Green);
            ImGui.Button("Already Applied with Candidate Profile", new Vector2(340f * scale, 38f * scale));
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
            if (ImGui.Button("Apply with Candidate Profile (1-Click)", new Vector2(340f * scale, 38f * scale)))
            {
                _applySelectedJob = string.Join(", ", _editingProfile.MainJobs.Concat(_editingProfile.SecondaryJobs).Distinct());
                _applySelectedRole = listing.RolesNeeded.FirstOrDefault() ?? "TankMT";
                _applyMessage = "Hello! Very interested in your listing. Available for a trial whenever convenient.";
                _applyStatusMessage = string.Empty;
                _showApplyModal = true;
            }
            ImGui.PopStyleColor();
        }

        ImGui.EndChild();
        ImGui.PopStyleColor();
    }
    #endregion

    #region Tab 1: My Profile / Raid Resume
    private void DrawProfileTab(float scale, float availWidth)
    {
        ImGui.TextColored(Ui.Gold, "Candidate Profile & Reusable Raid Resume");
        ImGui.TextColored(Ui.Dimmed, "Set up your profile once to instantly apply to all recruitment listings.");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.BeginChild("##profile_scroll", new Vector2(availWidth, 0f), false);
        ImGui.Spacing();

        if (!Service.IsAuthenticated)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.35f, 0.22f, 0.05f, 0.75f));
            if (ImGui.BeginChild("##auth_warning_banner", new Vector2(availWidth, 38f * scale), true, ImGuiWindowFlags.NoScrollbar))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Ui.Gold);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Text(FontAwesomeIcon.ExclamationTriangle.ToIconString());
                ImGui.PopFont();
                ImGui.SameLine(0f, 4f * scale);
                ImGui.Text("Discord not linked:");
                ImGui.PopStyleColor();
                ImGui.SameLine(0f, 6f * scale);
                ImGui.TextColored(Ui.White, "Link your Discord in 'Account / Discord' so your profile and candidacies persist permanently.");
                
                ImGui.SameLine(availWidth - 160f * scale);
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
                if (ImGui.Button("Link Discord###link_disc_btn", new Vector2(140f * scale, 24f * scale)))
                {
                    OpenSubTab(3);
                }
                ImGui.PopStyleColor();
            }
            ImGui.EndChild();
            ImGui.PopStyleColor();
            ImGui.Spacing();
        }

        // Section 1: Raid Info & Item Level
        Ui.SectionHeader(FontAwesomeIcon.ShieldAlt, "Raid Info & Item Level");
        ImGui.Spacing();

        ImGui.SetNextItemWidth(140f * scale);
        int ilvl = _editingProfile.Ilvl;
        if (ImGui.InputInt("Average Item Level (ilvl)", ref ilvl)) _editingProfile.Ilvl = Math.Max(0, ilvl);

        ImGui.SameLine(0f, 20f * scale);
        int profRegIdx = Array.IndexOf(RecruitmentConstants.Regions, _editingProfile.CharacterRegion);
        if (profRegIdx < 0) profRegIdx = 0;
        ImGui.SetNextItemWidth(140f * scale);
        if (ImGui.Combo("Region##prof_reg", ref profRegIdx, RecruitmentConstants.Regions, RecruitmentConstants.Regions.Length))
        {
            _editingProfile.CharacterRegion = RecruitmentConstants.Regions[profRegIdx];
        }

        ImGui.SameLine(0f, 20f * scale);
        ImGui.BeginGroup();
        ImGui.TextColored(Ui.White, "Languages:");
        ImGui.SameLine(0f, 8f * scale);
        string[] profLangs = ["FR", "EN", "DE", "JP"];
        foreach (var lang in profLangs)
        {
            bool hasLang = _editingProfile.Languages.Contains(lang);
            if (hasLang) ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
            if (ImGui.Button($"{lang}##prof_lang_{lang}", new Vector2(42f * scale, 24f * scale)))
            {
                if (hasLang)
                {
                    if (_editingProfile.Languages.Count > 1) _editingProfile.Languages.Remove(lang);
                }
                else
                {
                    _editingProfile.Languages.Add(lang);
                }
            }
            if (hasLang) ImGui.PopStyleColor();
            ImGui.SameLine(0f, 4f * scale);
        }
        ImGui.EndGroup();

        ImGui.Spacing();
        ImGui.Separator();

        // Section 2: Jobs & Roles
        Ui.SectionHeader(FontAwesomeIcon.HatWizard, "Mastered Jobs");
        ImGui.Spacing();

        ImGui.TextColored(Ui.Gold, "Select the jobs you can play:");
        DrawJobSelection(scale, _editingProfile.MainJobs, (job, state) =>
        {
            if (state) _editingProfile.MainJobs.Add(job);
            else _editingProfile.MainJobs.Remove(job);
        }, "main");

        ImGui.Spacing();
        ImGui.Separator();

        // Section 3: Plugins Used
        Ui.SectionHeader(FontAwesomeIcon.Cogs, "Plugins & Tools Used");
        ImGui.TextColored(Ui.Dimmed, "Indicate the plugins you are comfortable using to ensure optimal synergy with the static:");
        ImGui.Spacing();

        bool bm = _editingProfile.PluginsUsed.BossMod;
        if (ImGui.Checkbox("BossMod", ref bm)) _editingProfile.PluginsUsed.BossMod = bm;

        ImGui.SameLine(0f, 30f * scale);
        bool sp = _editingProfile.PluginsUsed.Splatoon;
        if (ImGui.Checkbox("Splatoon", ref sp)) _editingProfile.PluginsUsed.Splatoon = sp;

        ImGui.SameLine(0f, 30f * scale);
        bool wr = _editingProfile.PluginsUsed.Wrath;
        if (ImGui.Checkbox("Wrath Combo", ref wr)) _editingProfile.PluginsUsed.Wrath = wr;

        bool rsr = _editingProfile.PluginsUsed.Rsr;
        if (ImGui.Checkbox("Rotation Solver Reborn", ref rsr)) _editingProfile.PluginsUsed.Rsr = rsr;

        ImGui.SameLine(0f, 30f * scale);
        bool rep = _editingProfile.PluginsUsed.Replica;
        if (ImGui.Checkbox("Replica", ref rep)) _editingProfile.PluginsUsed.Replica = rep;

        ImGui.SameLine(0f, 30f * scale);
        bool art = _editingProfile.PluginsUsed.Artisan;
        if (ImGui.Checkbox("Artisan", ref art)) _editingProfile.PluginsUsed.Artisan = art;

        ImGui.SameLine(0f, 30f * scale);
        bool cact = _editingProfile.PluginsUsed.Cactbot;
        if (ImGui.Checkbox("Cactbot / ACT", ref cact)) _editingProfile.PluginsUsed.Cactbot = cact;

        ImGui.SameLine(0f, 30f * scale);
        bool mb = _editingProfile.PluginsUsed.ModBeast;
        if (ImGui.Checkbox("ModBeast", ref mb)) _editingProfile.PluginsUsed.ModBeast = mb;

        ImGui.Spacing();
        ImGui.Separator();

        // Section 4: Availabilities
        Ui.SectionHeader(FontAwesomeIcon.CalendarAlt, "Availability & Schedule");
        ImGui.Spacing();

        ImGui.TextColored(Ui.White, "Available days:");
        foreach (var day in RecruitmentConstants.DaysOfWeek)
        {
            bool hasDay = _editingProfile.AvailableDays.Contains(day);
            if (ImGui.Checkbox($"{day}##day_{day}", ref hasDay))
            {
                if (hasDay) _editingProfile.AvailableDays.Add(day);
                else _editingProfile.AvailableDays.Remove(day);
            }
            ImGui.SameLine(0f, 12f * scale);
        }
        ImGui.NewLine();

        string start = _editingProfile.PreferredTimeStart;
        DrawTimePicker("Start Time", ref start, scale);
        _editingProfile.PreferredTimeStart = start;

        ImGui.SameLine(0f, 16f * scale);

        string end = _editingProfile.PreferredTimeEnd;
        DrawTimePicker("End Time", ref end, scale);
        _editingProfile.PreferredTimeEnd = end;

        ImGui.SameLine(0f, 16f * scale);
        ImGui.BeginGroup();
        ImGui.TextColored(Ui.White, "Nights / week");
        ImGui.SetNextItemWidth(100f * scale);
        string npw = _editingProfile.NightsPerWeek;
        if (ImGui.InputText("##nights_per_week", ref npw, 16)) _editingProfile.NightsPerWeek = npw;
        ImGui.EndGroup();

        ImGui.Spacing();
        ImGui.Separator();

        // Section 5: Experience & Description
        Ui.SectionHeader(FontAwesomeIcon.FileAlt, "Raid Experience & Profile Bio");
        ImGui.Spacing();

        ImGui.TextColored(Ui.White, "Past experience & current progression:");
        string exp = _editingProfile.Experience;
        if (ImGui.InputTextMultiline("##exp_input", ref exp, 1024, new Vector2(availWidth - 20f * scale, 70f * scale)))
        {
            _editingProfile.Experience = exp;
        }

        ImGui.Spacing();
        ImGui.TextColored(Ui.White, "Personal introduction & goals:");
        string about = _editingProfile.AboutMe;
        if (ImGui.InputTextMultiline("##about_input", ref about, 1024, new Vector2(availWidth - 20f * scale, 70f * scale)))
        {
            _editingProfile.AboutMe = about;
        }

        ImGui.Spacing();
        ImGui.Separator();

        // Save Button
        ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
        if (ImGui.Button("Save Candidate Profile to Cloud", new Vector2(340f * scale, 36f * scale)))
        {
            _ = SaveProfileAsync();
        }
        ImGui.PopStyleColor();

        if (!string.IsNullOrEmpty(_profileStatusMessage))
        {
            ImGui.SameLine(0f, 12f * scale);
            ImGui.TextColored(Ui.Green, _profileStatusMessage);
        }

        ImGui.Dummy(new Vector2(0f, 20f * scale));
        ImGui.EndChild();
    }

    private void AutoDetectCharacterInfo()
    {
        try
        {
            var localPlayer = Plugin.ObjectTable.LocalPlayer;
            if (localPlayer != null)
            {
                _editingProfile.CharacterName = localPlayer.Name.TextValue;
                _editingProfile.CharacterWorld = localPlayer.HomeWorld.ValueNullable?.Name.ExtractText() ?? "Cerberus";
                _editingProfile.CharacterDatacenter = "Chaos";
                if (string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo))
                {
                    _plugin.Configuration.AccountPseudo = localPlayer.Name.TextValue;
                    _plugin.Configuration.Save();
                }
                _editingProfile.DisplayName = _plugin.Configuration.AccountPseudo;

                // Add current job if not present
                var jobAbbr = localPlayer.ClassJob.ValueNullable?.Abbreviation.ExtractText();
                if (!string.IsNullOrEmpty(jobAbbr) && !_editingProfile.MainJobs.Contains(jobAbbr))
                {
                    _editingProfile.MainJobs.Insert(0, jobAbbr);
                }

                _profileStatusMessage = $"Successfully imported: {_editingProfile.CharacterName} ({_editingProfile.CharacterWorld})!";
            }
            else
            {
                _profileStatusMessage = "Player not logged in-game.";
            }
        }
        catch (Exception ex)
        {
            _profileStatusMessage = $"Detection error: {ex.Message}";
        }
    }

    private async System.Threading.Tasks.Task SaveProfileAsync()
    {
        _editingProfile.DisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) 
            ? _plugin.Configuration.AccountPseudo 
            : _editingProfile.DisplayName;
        _editingProfile.Aetherphone = _plugin.Configuration.AccountAetherphone;
        _editingProfile.ShareDiscordOnAccept = _plugin.Configuration.ShareDiscordOnAccept;
        _editingProfile.ShareAetherphoneOnAccept = _plugin.Configuration.ShareAetherphoneOnAccept;

        if (Service.IsAuthenticated)
        {
            _editingProfile.UserId = Service.CurrentUserId;
            _editingProfile.DiscordId = Service.CurrentDiscordId ?? string.Empty;
            _editingProfile.DiscordTag = Service.CurrentDiscordTag ?? _editingProfile.DiscordTag;
            _editingProfile.AvatarUrl = Service.CurrentAvatarUrl;
        }

        _profileStatusMessage = "Saving profile...";
        bool success = await Service.SaveProfileAsync(_editingProfile);
        if (success)
        {
            _profileStatusMessage = "Candidate profile saved successfully!";
        }
        else
        {
            _profileStatusMessage = $"Error: {Service.LastError}";
        }
    }

    private async System.Threading.Tasks.Task LoadProfileAsync()
    {
        if (!Service.IsAuthenticated) return;
        
        _profileStatusMessage = "Loading profile from cloud...";
        var profile = await Service.GetProfileAsync(Service.CurrentUserId, Service.CurrentDiscordId);
        if (profile != null)
        {
            _editingProfile = profile;
            
            // Backwards compatibility migration: if they had secondary jobs, merge them into main jobs
            if (_editingProfile.SecondaryJobs != null && _editingProfile.SecondaryJobs.Count > 0)
            {
                foreach (var job in _editingProfile.SecondaryJobs)
                {
                    if (!_editingProfile.MainJobs.Contains(job))
                    {
                        _editingProfile.MainJobs.Add(job);
                    }
                }
                _editingProfile.SecondaryJobs.Clear();
            }
            
            if (string.IsNullOrWhiteSpace(_editingProfile.DisplayName))
                _editingProfile.DisplayName = _plugin.Configuration.AccountPseudo;
            if (string.IsNullOrWhiteSpace(_editingProfile.Aetherphone))
                _editingProfile.Aetherphone = _plugin.Configuration.AccountAetherphone;
            
            _profileStatusMessage = "Profile loaded successfully!";
        }
        else
        {
            _profileStatusMessage = "No profile found on cloud, using local default.";
        }
    }
    #endregion

    #region Tab 2: My Listings & Applications
    private void DrawMyListingsTab(float scale, float availWidth)
    {
        Ui.SectionHeader(FontAwesomeIcon.ListAlt, "Manage My Listings & Applications");
        ImGui.TextColored(Ui.Dimmed, "Switch between your published listings (to recruit) and your sent applications (to find a static).");
        ImGui.SameLine(availWidth - 260f * scale);
        if (_isLoadingApplications)
        {
            ImGui.TextColored(Ui.Gold, "Loading...");
            ImGui.SameLine(0f, 10f * scale);
        }
        if (ImGui.Button("Refresh###refresh_my_apps", new Vector2(130f * scale, 26f * scale)))
        {
            _ = RefreshMyApplicationsAndListingsAsync();
        }

        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.BeginTabBar("##my_manage_subtabs"))
        {
            if (ImGui.BeginTabItem("My Listings & Received Applications###subtab_my_listings"))
            {
                ImGui.Spacing();
                DrawMyPublishedListingsSection(scale, availWidth);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("My Sent Applications###subtab_my_sent_apps"))
            {
                ImGui.Spacing();
                DrawMySentApplicationsSection(scale, availWidth);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawMyPublishedListingsSection(float scale, float availWidth)
    {
        ImGui.BeginChild("##my_listings_scroll", new Vector2(availWidth, 0f), false);

        var myListings = _cachedListings.Where(l => 
            (!string.IsNullOrEmpty(Service.CurrentUserId) && l.UserId == Service.CurrentUserId) ||
            (!string.IsNullOrEmpty(_editingProfile.DiscordTag) && l.AuthorDiscordTag == _editingProfile.DiscordTag) || 
            (!string.IsNullOrEmpty(_editingProfile.DisplayName) && l.AuthorDisplayName == _editingProfile.DisplayName)).ToList();

        if (myListings.Count == 0)
        {
            ImGui.Dummy(new Vector2(0f, 20f * scale));
            ImGui.TextColored(Ui.Dimmed, "You have no active listings published on Party Finder.");
            ImGui.Spacing();
            if (ImGui.Button("Create New PF", new Vector2(240f * scale, 32f * scale)))
            {
                _isEditingPf = false;
                InitDefaultNewListing();
                _showCreatePfModal = true;
                _createStatusMessage = string.Empty;
            }
        }
        else if (myListings.Count >= MaxListingsPerUser)
        {
            ImGui.TextColored(Ui.Gold, $"Your active listings ({myListings.Count}/{MaxListingsPerUser} max):");
            ImGui.SameLine(0f, 10f * scale);
            ImGui.TextColored(Ui.Red, $"Limit reached — delete a listing to create a new one.");
            ImGui.Spacing();

            foreach (var listing in myListings)
            {
                DrawMyPublishedListingCard(listing, scale, availWidth - 20f * scale);
            }
        }
        else
        {
            ImGui.TextColored(Ui.Gold, $"Your active listings ({myListings.Count}):");
            ImGui.Spacing();

            foreach (var listing in myListings)
            {
                DrawMyPublishedListingCard(listing, scale, availWidth - 20f * scale);
            }
        }

        ImGui.Dummy(new Vector2(0f, 20f * scale));
        ImGui.EndChild();
    }

    private void DrawMyPublishedListingCard(RecruitmentListing listing, float scale, float width)
    {
        bool hasDescription = !string.IsNullOrWhiteSpace(listing.Description);
        bool hasRoles = listing.RolesNeeded != null && listing.RolesNeeded.Count > 0;
        
        _receivedApplicationsByListing.TryGetValue(listing.Id, out var rawApps);
        var apps = (rawApps ?? []).Where(a => a.Status == "PENDING" || a.Status == "ACCEPTED").ToList();
        int appCount = apps.Count;

        float baseHeaderHeight = 65f * scale;
        if (hasRoles) baseHeaderHeight += 20f * scale;
        if (hasDescription) baseHeaderHeight += 20f * scale;

        float appsHeight = 32f * scale;
        if (appCount > 0 && apps != null)
        {
            appsHeight = 28f * scale;
            foreach (var app in apps)
            {
                appsHeight += GetReceivedApplicationCardHeight(app, scale) + 8f * scale;
            }
        }
        float totalCardHeight = baseHeaderHeight + appsHeight + 14f * scale;

        Vector2 cursor = ImGui.GetCursorScreenPos();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        // Card background & subtle border
        drawList.AddRectFilled(cursor, cursor + new Vector2(width, totalCardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(0.13f, 0.13f, 0.13f, 0.95f)), 8f * scale);
        drawList.AddRect(cursor, cursor + new Vector2(width, totalCardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(Ui.Accent.X, Ui.Accent.Y, Ui.Accent.Z, 0.4f)), 8f * scale, ImDrawFlags.None, 1.2f);

        ImGui.BeginGroup();

        // Top line: Duty title, Content type, Region/Language badge, Edit and Close buttons
        ImGui.SetCursorScreenPos(cursor + new Vector2(14f * scale, 10f * scale));
        ImGui.TextColored(Ui.White, listing.TargetDuty);
        ImGui.SameLine(0f, 8f * scale);
        ImGui.TextColored(Ui.Accent, $"[{listing.ContentType}]");
        ImGui.SameLine(0f, 6f * scale);
        ImGui.TextColored(Ui.Gold, $"[{listing.Region}]");
        if (listing.Languages != null && listing.Languages.Count > 0)
        {
            ImGui.SameLine(0f, 6f * scale);
            ImGui.TextColored(Ui.Blue, $"[{string.Join("/", listing.Languages)}]");
        }

        float editBtnWidth = 85f * scale;
        float closeBtnWidth = 135f * scale;
        float totalRightWidth = editBtnWidth + closeBtnWidth + 8f * scale;
        ImGui.SetCursorScreenPos(cursor + new Vector2(width - totalRightWidth - 14f * scale, 8f * scale));

        ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
        if (Ui.IconButton(FontAwesomeIcon.Edit, "Edit", $"edit_{listing.Id}", new Vector2(editBtnWidth, 24f * scale), scale))
        {
            _isEditingPf = true;
            _newListing = new RecruitmentListing
            {
                Id = listing.Id,
                UserId = listing.UserId,
                AuthorDiscordId = listing.AuthorDiscordId,
                AuthorDiscordTag = listing.AuthorDiscordTag,
                AuthorDisplayName = listing.AuthorDisplayName,
                AuthorAetherphone = listing.AuthorAetherphone,
                AuthorAvatarUrl = listing.AuthorAvatarUrl,
                ShareDiscordOnAccept = listing.ShareDiscordOnAccept,
                ShareAetherphoneOnAccept = listing.ShareAetherphoneOnAccept,
                ContentType = listing.ContentType,
                TargetDuty = listing.TargetDuty,
                Region = listing.Region,
                Datacenter = listing.Datacenter,
                Languages = [.. (listing.Languages ?? [])],
                Progression = listing.Progression,
                ScheduleDays = [.. (listing.ScheduleDays ?? [])],
                ScheduleTimeStart = listing.ScheduleTimeStart,
                ScheduleTimeEnd = listing.ScheduleTimeEnd,
                ScheduleTimezone = listing.ScheduleTimezone,
                RolesNeeded = [.. (listing.RolesNeeded ?? [])],
                CurrentRoster = [.. (listing.CurrentRoster ?? [])],
                Tags = [.. (listing.Tags ?? [])],
                Description = listing.Description,
                Status = listing.Status,
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.UpdatedAt,
                BumpedAt = listing.BumpedAt,
                ExpiresAt = listing.ExpiresAt
            };
            _inputTagsString = string.Join(", ", _newListing.Tags);
            _createStatusMessage = string.Empty;
            _showCreatePfModal = true;
        }
        ImGui.PopStyleColor();

        ImGui.SameLine(0f, 8f * scale);
        bool isConfirming = _listingConfirmingCloseIds.Contains(listing.Id);
        string btnText = isConfirming ? "Are you sure?###close_" + listing.Id : "End Recruitment###close_" + listing.Id;
        ImGui.PushStyleColor(ImGuiCol.Button, isConfirming ? new Vector4(0.75f, 0.15f, 0.15f, 1.0f) : new Vector4(0.55f, 0.15f, 0.15f, 0.85f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, isConfirming ? new Vector4(0.95f, 0.2f, 0.2f, 1.0f) : new Vector4(0.75f, 0.2f, 0.2f, 1.0f));
        if (ImGui.Button(btnText, new Vector2(closeBtnWidth, 24f * scale)))
        {
            if (isConfirming)
            {
                _ = CloseAndRefreshListingAsync(listing);
            }
            else
            {
                _listingConfirmingCloseIds.Add(listing.Id);
            }
        }
        ImGui.PopStyleColor(2);

        // Schedule & Progression
        ImGui.SetCursorScreenPos(cursor + new Vector2(14f * scale, 34f * scale));
        ImGui.PushStyleColor(ImGuiCol.Text, Ui.White);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(FontAwesomeIcon.CalendarAlt.ToIconString());
        ImGui.PopFont();
        ImGui.SameLine(0f, 4f * scale);
        ImGui.Text($"Schedule: {FormatSchedule(listing)}");
        ImGui.PopStyleColor();
        ImGui.SameLine(0f, 16f * scale);
        ImGui.TextColored(Ui.Green, $"Progression: {listing.Progression}");

        float currY = 56f * scale;

        // Roles needed
        if (hasRoles)
        {
            ImGui.SetCursorScreenPos(cursor + new Vector2(14f * scale, currY));
            ImGui.TextColored(Ui.White, "Roles needed: ");
            foreach (var role in listing.RolesNeeded!)
            {
                ImGui.SameLine(0f, 4f * scale);
                ImGui.TextColored(Ui.Gold, $"[{role}]");
            }
            currY += 20f * scale;
        }

        // Description
        if (hasDescription)
        {
            ImGui.SetCursorScreenPos(cursor + new Vector2(14f * scale, currY));
            ImGui.TextColored(Ui.Dimmed, $"Note: {listing.Description}");
            currY += 20f * scale;
        }

        // Separator line
        drawList.AddLine(cursor + new Vector2(12f * scale, currY + 2f * scale), cursor + new Vector2(width - 12f * scale, currY + 2f * scale), ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.3f, 0.5f)));
        currY += 10f * scale;

        // Received applications section
        if (appCount > 0 && apps != null)
        {
            ImGui.SetCursorScreenPos(cursor + new Vector2(14f * scale, currY));
            ImGui.PushStyleColor(ImGuiCol.Text, Ui.Green);
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text(FontAwesomeIcon.Inbox.ToIconString());
            ImGui.PopFont();
            ImGui.SameLine(0f, 4f * scale);
            ImGui.Text($"Received applications ({appCount}):");
            ImGui.PopStyleColor();
            currY += 24f * scale;

            foreach (var app in apps)
            {
                ImGui.SetCursorScreenPos(cursor + new Vector2(14f * scale, currY));
                DrawReceivedApplicationCard(app, scale, width - 28f * scale);
                currY += GetReceivedApplicationCardHeight(app, scale) + 8f * scale;
            }
        }
        else
        {
            ImGui.SetCursorScreenPos(cursor + new Vector2(14f * scale, currY));
            ImGui.TextColored(Ui.Dimmed, "No applications received yet for this listing.");
            ImGui.SameLine(0f, 10f * scale);
            if (Ui.IconButton(FontAwesomeIcon.Sync, "Check applications", $"chk_{listing.Id}", new Vector2(0f, 20f * scale), scale))
            {
                _ = LoadApplicationsForListingAsync(listing.Id);
            }
        }

        ImGui.EndGroup();
        ImGui.SetCursorScreenPos(cursor + new Vector2(0f, totalCardHeight + 14f * scale));
    }

    private void DrawMySentApplicationsSection(float scale, float availWidth)
    {
        ImGui.BeginChild("##my_sent_apps_scroll", new Vector2(availWidth, 0f), false);

        if (_mySentApplications.Count == 0)
        {
            ImGui.Dummy(new Vector2(0f, 20f * scale));
            ImGui.TextColored(Ui.Dimmed, "You have not sent any applications yet.");
            ImGui.Spacing();
            if (Ui.IconButton(FontAwesomeIcon.Search, "Browse Static Listings", "browse_static_pf", new Vector2(280f * scale, 32f * scale), scale))
            {
                _selectedSubTab = 0; // Go to browse tab
            }
        }
        else
        {
            ImGui.TextColored(Ui.Gold, $"Application history ({_mySentApplications.Count}):");
            ImGui.Spacing();

            foreach (var myApp in _mySentApplications)
            {
                DrawSentApplicationCard(myApp, scale, availWidth - 20f * scale);
                ImGui.Spacing();
            }
        }

        ImGui.Dummy(new Vector2(0f, 20f * scale));
        ImGui.EndChild();
    }

    private float GetReceivedApplicationCardHeight(ApplicationItem app, float scale)
    {
        bool isExpanded = _expandedApplicationIds.Contains(app.Id);
        if (!isExpanded)
        {
            if (app.Status == "ACCEPTED")
            {
                return 64f * scale;
            }
            return 40f * scale;
        }

        float height = 88f * scale;
        var snap = app.ApplicantProfileSnapshot;
        if (snap != null)
        {
            if (!string.IsNullOrWhiteSpace(app.CustomMessage))
            {
                height += 20f * scale;
            }
            if (!string.IsNullOrWhiteSpace(snap.Experience))
            {
                height += 34f * scale;
            }
            if (!string.IsNullOrWhiteSpace(snap.AboutMe))
            {
                height += 34f * scale;
            }
        }
        return height;
    }

    private void DrawReceivedApplicationCard(ApplicationItem app, float scale, float width)
    {
        var snap = app.ApplicantProfileSnapshot ?? new CandidateProfile { CharacterName = "Player", DiscordTag = "N/A" };
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        Vector2 cursor = ImGui.GetCursorScreenPos();
        float cardHeight = GetReceivedApplicationCardHeight(app, scale);

        drawList.AddRectFilled(cursor, cursor + new Vector2(width, cardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(0.17f, 0.17f, 0.17f, 0.95f)), 6f * scale);
        drawList.AddRect(cursor, cursor + new Vector2(width, cardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(0.4f, 0.4f, 0.4f, 0.5f)), 6f * scale);

        ImGui.BeginGroup();
        float currY = 8f * scale;

        bool isExpanded = _expandedApplicationIds.Contains(app.Id);
        float headerClickWidth = width - 125f * scale;
        if (app.Status != "PENDING")
        {
            headerClickWidth = width - 110f * scale;
        }

        ImGui.SetCursorScreenPos(cursor);
        ImGui.PushID($"header_click_{app.Id}");
        if (ImGui.InvisibleButton("##header_btn", new Vector2(headerClickWidth, 36f * scale)))
        {
            if (isExpanded) _expandedApplicationIds.Remove(app.Id);
            else _expandedApplicationIds.Add(app.Id);
        }
        if (ImGui.IsItemHovered())
        {
            drawList.AddRectFilled(cursor, cursor + new Vector2(width, 36f * scale), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.04f)), 6f * scale);
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        ImGui.PopID();

        // Top line: Collapse/Expand chevron, Candidate pseudo, ilvl, and jobs
        ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, currY));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0f, 0f, 0f, 0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 0.2f));
        ImGui.PushFont(UiBuilder.IconFont);
        string toggleIcon = isExpanded ? FontAwesomeIcon.ChevronDown.ToIconString() : FontAwesomeIcon.ChevronRight.ToIconString();
        bool toggleClicked = ImGui.Button($"{toggleIcon}##toggle_{app.Id}", new Vector2(20f * scale, 20f * scale));
        ImGui.PopFont();
        if (toggleClicked)
        {
            if (isExpanded) _expandedApplicationIds.Remove(app.Id);
            else _expandedApplicationIds.Add(app.Id);
        }
        ImGui.PopStyleColor(2);
        
        ImGui.SameLine(0f, 4f * scale);
        string candidatePseudo = !string.IsNullOrWhiteSpace(snap.DisplayName) ? snap.DisplayName : snap.DiscordTag;
        ImGui.TextColored(Ui.White, $"Candidate: {candidatePseudo}");
        
        ImGui.SameLine(0f, 6f * scale);
        ImGui.TextColored(new Vector4(0.72f, 0.45f, 0.95f, 1f), $"(ilvl {snap.Ilvl})"); // Purple/violet color

        ImGui.SameLine(0f, 10f * scale);
        var jobs = (app.AppliedAsJob ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (jobs.Length > 0)
        {
            foreach (var job in jobs)
            {
                var jobTex = GetJobIconTexture(job);
                var jobWrap = jobTex?.GetWrapOrDefault();
                if (jobWrap != null)
                {
                    ImGui.Image(jobWrap.Handle, new Vector2(16f * scale, 16f * scale));
                    ImGui.SameLine(0f, 4f * scale);
                }
            }
        }
        ImGui.TextColored(Ui.Accent, $"[Jobs: {string.Join(", ", jobs)}]");

        // Contact line (if accepted, we show it whether expanded or not)
        if (app.Status == "ACCEPTED")
        {
            float contactY = currY + 24f * scale;
            ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, contactY));
            ImGui.BeginGroup();
            bool hasContact = false;
            if (snap.ShareDiscordOnAccept && !string.IsNullOrEmpty(snap.DiscordTag))
            {
                ImGui.TextColored(Ui.DiscordColor, $"Discord: {snap.DiscordTag}");
                ImGui.SameLine(0f, 4f * scale);
                if (ImGui.SmallButton($"Copy Discord###cp_disc_{app.Id}"))
                {
                    ImGui.SetClipboardText(snap.DiscordTag);
                }
                ImGui.SameLine(0f, 12f * scale);
                hasContact = true;
            }

            if (snap.ShareAetherphoneOnAccept && !string.IsNullOrEmpty(snap.Aetherphone))
            {
                ImGui.TextColored(Ui.AetherphoneColor, $"Aetherphone: {snap.Aetherphone}");
                ImGui.SameLine(0f, 4f * scale);
                if (ImGui.SmallButton($"Copy Phone###cp_phone_{app.Id}"))
                {
                    ImGui.SetClipboardText(snap.Aetherphone);
                }
                hasContact = true;
            }
            
            if (!hasContact)
            {
                ImGui.TextColored(Ui.Dimmed, "No contacts shared.");
            }
            ImGui.EndGroup();
        }

        if (isExpanded)
        {
            currY += 24f * scale;

            if (app.Status == "ACCEPTED")
            {
                // Already rendered above, just advance the vertical pointer
                currY += 20f * scale;
            }
            else
            {
                ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, currY));
                ImGui.PushStyleColor(ImGuiCol.Text, Ui.Dimmed);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Text(FontAwesomeIcon.Lock.ToIconString());
                ImGui.PopFont();
                ImGui.PopStyleColor();
                ImGui.SameLine(0f, 4f * scale);
                ImGui.TextColored(Ui.Dimmed, "Contact hidden until accepted (Shared: ");
                if (snap.ShareDiscordOnAccept)
                {
                    ImGui.SameLine(0f, 0f);
                    ImGui.TextColored(Ui.DiscordColor, "Discord");
                }
                if (snap.ShareAetherphoneOnAccept)
                {
                    if (snap.ShareDiscordOnAccept)
                    {
                        ImGui.SameLine(0f, 0f);
                        ImGui.TextColored(Ui.Dimmed, ", ");
                    }
                    ImGui.SameLine(0f, 0f);
                    ImGui.TextColored(Ui.AetherphoneColor, "Aetherphone");
                }
                ImGui.SameLine(0f, 0f);
                ImGui.TextColored(Ui.Dimmed, ")");
                
                currY += 20f * scale;
            }

            ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, currY));
            ImGui.TextColored(Ui.Gold, "Plugins:");
            ImGui.SameLine(0f, 4f * scale);
            string pluginsStr;
            if (snap.PluginsUsed != null)
            {
                var list = new List<string>();
                if (snap.PluginsUsed.BossMod) list.Add("BossMod");
                if (snap.PluginsUsed.Splatoon) list.Add("Splatoon");
                if (snap.PluginsUsed.Wrath) list.Add("Wrath Combo");
                if (snap.PluginsUsed.Rsr) list.Add("Rotation Solver Reborn");
                if (snap.PluginsUsed.Replica) list.Add("Replica");
                if (snap.PluginsUsed.Artisan) list.Add("Artisan");
                if (snap.PluginsUsed.Cactbot) list.Add("Cactbot / ACT");
                if (snap.PluginsUsed.ModBeast) list.Add("ModBeast");
                pluginsStr = list.Count > 0 ? string.Join(", ", list) : "None";
            }
            else
            {
                pluginsStr = "N/A";
            }
            ImGui.TextColored(Ui.White, pluginsStr);
            currY += 20f * scale;

            ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, currY));
            string daysStr = snap.AvailableDays != null ? string.Join(", ", snap.AvailableDays) : "N/A";
            string npwStr = !string.IsNullOrWhiteSpace(snap.NightsPerWeek) ? $" | {snap.NightsPerWeek} nights/week" : "";
            ImGui.TextColored(Ui.Green, $"Schedule: {daysStr} ({snap.PreferredTimeStart}-{snap.PreferredTimeEnd}){npwStr}");
            currY += 20f * scale;

            if (!string.IsNullOrWhiteSpace(app.CustomMessage))
            {
                ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, currY));
                ImGui.TextColored(Ui.White, $"Message: \"{app.CustomMessage}\"");
                currY += 20f * scale;
            }

            if (!string.IsNullOrWhiteSpace(snap.Experience))
            {
                ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, currY));
                ImGui.TextColored(Ui.Gold, "Experience:");
                
                ImGui.SetCursorScreenPos(cursor + new Vector2(90f * scale, currY - 2f * scale));
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.12f, 0.12f, 0.12f, 0.5f));
                if (ImGui.BeginChild($"##exp_scroll_{app.Id}", new Vector2(width - 105f * scale, 30f * scale), true, ImGuiWindowFlags.NoScrollbar))
                {
                    ImGui.TextWrapped(snap.Experience);
                    ImGui.EndChild();
                }
                ImGui.PopStyleColor();
                currY += 34f * scale;
            }

            if (!string.IsNullOrWhiteSpace(snap.AboutMe))
            {
                ImGui.SetCursorScreenPos(cursor + new Vector2(10f * scale, currY));
                ImGui.TextColored(Ui.Gold, "Bio:");
                
                ImGui.SetCursorScreenPos(cursor + new Vector2(90f * scale, currY - 2f * scale));
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.12f, 0.12f, 0.12f, 0.5f));
                if (ImGui.BeginChild($"##bio_scroll_{app.Id}", new Vector2(width - 105f * scale, 30f * scale), true, ImGuiWindowFlags.NoScrollbar))
                {
                    ImGui.TextWrapped(snap.AboutMe);
                    ImGui.EndChild();
                }
                ImGui.PopStyleColor();
                currY += 34f * scale;
            }
        }

        float rightBtnWidth = 110f * scale;
        float rightBtnY = isExpanded ? (cardHeight / 2f - 26f * scale) : 7f * scale;

        if (app.Status == "PENDING")
        {
            if (isExpanded)
            {
                ImGui.SetCursorScreenPos(cursor + new Vector2(width - rightBtnWidth - 10f * scale, rightBtnY));
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Green);
                if (ImGui.Button($"Accept###acc_{app.Id}", new Vector2(rightBtnWidth, 24f * scale)))
                {
                    app.Status = "ACCEPTED";
                    _ = Service.UpdateApplicationStatusAsync(app.Id, "ACCEPTED");

                    RecruitmentToastOverlay.AddToast(new RecruitmentToast
                    {
                        Kind = RecruitmentToastKind.ApplicationAccepted,
                        Title = "Application Accepted!",
                        Message = $"You accepted the application from {candidatePseudo} for [{app.AppliedAsJob}]! Contact shared.",
                        Icon = FontAwesomeIcon.CheckCircle,
                        OnOpen = () => _plugin.ShowRecruitment(2)
                    });
                }
                ImGui.PopStyleColor();

                ImGui.SetCursorScreenPos(cursor + new Vector2(width - rightBtnWidth - 10f * scale, rightBtnY + 28f * scale));
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Red);
                if (ImGui.Button($"Decline###dec_{app.Id}", new Vector2(rightBtnWidth, 24f * scale)))
                {
                    app.Status = "DECLINED";
                    _ = Service.UpdateApplicationStatusAsync(app.Id, "DECLINED");
                }
                ImGui.PopStyleColor();
            }
            else
            {
                float smallBtnWidth = 52f * scale;
                ImGui.SetCursorScreenPos(cursor + new Vector2(width - (smallBtnWidth * 2f) - 14f * scale, 8f * scale));
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Green);
                ImGui.PushFont(UiBuilder.IconFont);
                bool acceptPressed = ImGui.Button($"{FontAwesomeIcon.Check.ToIconString()}###acc_{app.Id}", new Vector2(smallBtnWidth, 24f * scale));
                ImGui.PopFont();
                if (acceptPressed)
                {
                    app.Status = "ACCEPTED";
                    _ = Service.UpdateApplicationStatusAsync(app.Id, "ACCEPTED");

                    RecruitmentToastOverlay.AddToast(new RecruitmentToast
                    {
                        Kind = RecruitmentToastKind.ApplicationAccepted,
                        Title = "Application Accepted!",
                        Message = $"You accepted the application from {candidatePseudo} for [{app.AppliedAsJob}]! Contact shared.",
                        Icon = FontAwesomeIcon.CheckCircle,
                        OnOpen = () => _plugin.ShowRecruitment(2)
                    });
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Accept Application");
                ImGui.PopStyleColor();

                ImGui.SameLine(0f, 4f * scale);
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Red);
                ImGui.PushFont(UiBuilder.IconFont);
                bool declinePressed = ImGui.Button($"{FontAwesomeIcon.Times.ToIconString()}###dec_{app.Id}", new Vector2(smallBtnWidth, 24f * scale));
                ImGui.PopFont();
                if (declinePressed)
                {
                    app.Status = "DECLINED";
                    _ = Service.UpdateApplicationStatusAsync(app.Id, "DECLINED");
                }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Decline Application");
                ImGui.PopStyleColor();
            }
        }
        else
        {
            ImGui.SetCursorScreenPos(cursor + new Vector2(width - rightBtnWidth - 10f * scale, (isExpanded || app.Status == "ACCEPTED") ? (cardHeight / 2f - 10f * scale) : 10f * scale));
            Vector4 statusCol = app.Status == "ACCEPTED" ? Ui.Green : Ui.Red;
            ImGui.TextColored(statusCol, $"Status: {app.Status}");
        }

        ImGui.EndGroup();
        ImGui.SetCursorScreenPos(cursor + new Vector2(0f, cardHeight + 6f * scale));
    }

    private void DrawSentApplicationCard(ApplicationItem app, float scale, float width)
    {
        var targetListing = _cachedListings.FirstOrDefault(l => l.Id == app.ListingId);
        string dutyName = targetListing?.TargetDuty ?? "Raid Listing";
        string leaderPseudo = targetListing?.AuthorDisplayName ?? targetListing?.AuthorDiscordTag ?? "Static Leader";

        bool showDiscord = targetListing != null &&
            targetListing.ShareDiscordOnAccept &&
            !string.IsNullOrEmpty(targetListing.AuthorDiscordTag) &&
            (app.ApplicantProfileSnapshot?.ShareDiscordOnAccept ?? false);

        bool showAetherphone = targetListing != null &&
            targetListing.ShareAetherphoneOnAccept &&
            !string.IsNullOrEmpty(targetListing.AuthorAetherphone) &&
            (app.ApplicantProfileSnapshot?.ShareAetherphoneOnAccept ?? false);

        bool showContacts = app.Status == "ACCEPTED" && (showDiscord || showAetherphone);

        Vector2 cursor = ImGui.GetCursorScreenPos();
        float cardHeight = (showContacts ? 92f : 70f) * scale;
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        drawList.AddRectFilled(cursor, cursor + new Vector2(width, cardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(0.15f, 0.15f, 0.15f, 0.95f)), 6f * scale);
        drawList.AddRect(cursor, cursor + new Vector2(width, cardHeight), ImGui.ColorConvertFloat4ToU32(new Vector4(Ui.Accent.X, Ui.Accent.Y, Ui.Accent.Z, 0.3f)), 6f * scale);

        ImGui.BeginGroup();
        ImGui.SetCursorScreenPos(cursor + new Vector2(12f * scale, 10f * scale));

        ImGui.TextColored(Ui.White, $"Application for: {dutyName}");
        ImGui.SameLine(0f, 10f * scale);
        var jobs = (app.AppliedAsJob ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (jobs.Length > 0)
        {
            foreach (var job in jobs)
            {
                var jobTex = GetJobIconTexture(job);
                var jobWrap = jobTex?.GetWrapOrDefault();
                if (jobWrap != null)
                {
                    ImGui.Image(jobWrap.Handle, new Vector2(16f * scale, 16f * scale));
                    ImGui.SameLine(0f, 4f * scale);
                }
            }
        }
        ImGui.TextColored(Ui.Accent, $"[Applied Jobs: {string.Join(", ", jobs)}]");
        ImGui.SameLine(0f, 10f * scale);
        ImGui.TextColored(Ui.Dimmed, $"Leader: {leaderPseudo}");

        if (showContacts)
        {
            ImGui.SetCursorScreenPos(cursor + new Vector2(12f * scale, 32f * scale));
            ImGui.BeginGroup();
            if (showDiscord)
            {
                ImGui.TextColored(Ui.DiscordColor, $"Discord: {targetListing.AuthorDiscordTag}");
                ImGui.SameLine(0f, 4f * scale);
                if (ImGui.SmallButton($"Copy Discord###cp_ldr_disc_{app.Id}"))
                {
                    ImGui.SetClipboardText(targetListing.AuthorDiscordTag);
                }
                ImGui.SameLine(0f, 12f * scale);
            }

            if (showAetherphone)
            {
                ImGui.TextColored(Ui.AetherphoneColor, $"Aetherphone: {targetListing.AuthorAetherphone}");
                ImGui.SameLine(0f, 4f * scale);
                if (ImGui.SmallButton($"Copy Phone###cp_ldr_ph_{app.Id}"))
                {
                    ImGui.SetClipboardText(targetListing.AuthorAetherphone);
                }
            }
            ImGui.EndGroup();
        }

        float messageY = (showContacts ? 56f : 34f) * scale;
        ImGui.SetCursorScreenPos(cursor + new Vector2(12f * scale, messageY));
        ImGui.TextColored(Ui.Dimmed, $"Sent message: \"{app.CustomMessage}\" (on {app.CreatedAt:MM/dd/yyyy HH:mm})");

        // Status badge on right
        Vector4 statusCol = app.Status switch
        {
            "ACCEPTED" => Ui.Green,
            "DECLINED" => Ui.Red,
            _ => Ui.Gold
        };

        float badgeWidth = 120f * scale;
        float badgeY = (showContacts ? 20f : 12f) * scale;
        float btnY = (showContacts ? 44f : 36f) * scale;

        ImGui.SetCursorScreenPos(cursor + new Vector2(width - badgeWidth - 12f * scale, badgeY));
        ImGui.TextColored(statusCol, $"[{app.Status}]");

        ImGui.SetCursorScreenPos(cursor + new Vector2(width - badgeWidth - 12f * scale, btnY));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.55f, 0.15f, 0.15f, 0.8f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.75f, 0.2f, 0.2f, 1f));
        if (ImGui.Button($"Cancel Apply###cancel_app_{app.Id}", new Vector2(badgeWidth, 22f * scale)))
        {
            _ = CancelApplicationAsync(app.Id);
        }
        ImGui.PopStyleColor(2);

        ImGui.EndGroup();
        ImGui.SetCursorScreenPos(cursor + new Vector2(0f, cardHeight + 6f * scale));
    }

    private int GetMyActiveListingCount()
    {
        return _cachedListings.Count(l =>
            (!string.IsNullOrEmpty(Service.CurrentUserId) && l.UserId == Service.CurrentUserId) ||
            (!string.IsNullOrEmpty(_editingProfile.DiscordTag) && l.AuthorDiscordTag == _editingProfile.DiscordTag) ||
            (!string.IsNullOrEmpty(_editingProfile.DisplayName) && l.AuthorDisplayName == _editingProfile.DisplayName));
    }

    private async System.Threading.Tasks.Task RefreshMyApplicationsAndListingsAsync()
    {
        _isLoadingApplications = true;
        await RefreshListingsAsync();

        if (Service.IsAuthenticated)
        {
            _mySentApplications = await Service.GetMyApplicationsAsync();
        }

        var myListings = _cachedListings.Where(l => 
            (!string.IsNullOrEmpty(Service.CurrentUserId) && l.UserId == Service.CurrentUserId) || 
            (!string.IsNullOrEmpty(_editingProfile.DiscordTag) && l.AuthorDiscordTag == _editingProfile.DiscordTag) ||
            (!string.IsNullOrEmpty(_editingProfile.DisplayName) && l.AuthorDisplayName == _editingProfile.DisplayName)).ToList();
        foreach (var l in myListings)
        {
            await LoadApplicationsForListingAsync(l.Id);
        }
        _isLoadingApplications = false;
    }

    private async System.Threading.Tasks.Task LoadApplicationsForListingAsync(string listingId)
    {
        var apps = await Service.GetApplicationsForListingAsync(listingId);
        _receivedApplicationsByListing[listingId] = apps;
    }

    private async System.Threading.Tasks.Task CloseAndRefreshListingAsync(RecruitmentListing listing)
    {
        _isLoadingApplications = true;
        // Delete listing entirely (applications are removed via ON DELETE CASCADE)
        bool success = await Service.DeleteListingAsync(listing.Id);
        if (success)
        {
            _cachedListings.Remove(listing);
            _receivedApplicationsByListing.Remove(listing.Id);
            _listingConfirmingCloseIds.Remove(listing.Id);
            await RefreshMyApplicationsAndListingsAsync();
        }
        else
        {
            // If delete failed, show error but do NOT invalidate session
            _listingConfirmingCloseIds.Remove(listing.Id);
        }
        _isLoadingApplications = false;
    }
    #endregion

    #region Create / Edit PF Modal
    private void DrawCreatePfModal(float scale)
    {
        string modalTitle = _isEditingPf ? "Edit PF###CreatePfModal" : "Create PF###CreatePfModal";
        ImGui.OpenPopup(modalTitle);

        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(720f * scale, 620f * scale), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(500f * scale, 400f * scale), new Vector2(1800f * scale, 1400f * scale));

        if (ImGui.BeginPopupModal(modalTitle, ref _showCreatePfModal, ImGuiWindowFlags.None))
        {
            Ui.SectionHeader(FontAwesomeIcon.PlusCircle, _isEditingPf ? "Edit Recruitment Listing" : "Create Recruitment Listing (Party Finder)");
            ImGui.TextColored(Ui.Dimmed, _isEditingPf ? "Update your published party finder listing." : "Publish a listing visible to the entire Replica community. Duration: 14 days.");
            ImGui.Separator();
            ImGui.Spacing();

            if (!ValidateContactSettings(out var contactWarning))
            {
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.35f, 0.1f, 0.1f, 0.5f));
                ImGui.BeginChild("##create_pf_contact_warning", new Vector2(0f, 34f * scale), true);
                ImGui.TextColored(Ui.Red, contactWarning);
                ImGui.EndChild();
                ImGui.PopStyleColor();
                ImGui.Spacing();
            }

            float bottomReserved = 48f * scale;
            ImGui.BeginChild("##create_pf_modal_scroll", new Vector2(0f, -bottomReserved), false);
            float availChild = ImGui.GetContentRegionAvail().X;
            float fieldWidth = MathF.Max(180f * scale, (availChild - 24f * scale) * 0.5f);

            ImGui.TextColored(Ui.Gold, "1. Category & Target Duty");
            int catIdx = Array.IndexOf(RecruitmentConstants.Categories, _newListing.ContentType);
            if (catIdx < 0) catIdx = 0;
            ImGui.SetNextItemWidth(170f * scale);
            if (ImGui.Combo("Category", ref catIdx, RecruitmentConstants.Categories, RecruitmentConstants.Categories.Length))
            {
                _newListing.ContentType = RecruitmentConstants.Categories[catIdx];
            }

            ImGui.SameLine(0f, 12f * scale);
            ImGui.SetNextItemWidth(fieldWidth);
            string duty = _newListing.TargetDuty;
            if (ImGui.InputText("Duty / Activity", ref duty, 128)) _newListing.TargetDuty = duty;

            ImGui.Spacing();
            ImGui.SetNextItemWidth(availChild - 140f * scale);
            string prog = _newListing.Progression;
            if (ImGui.InputText("Progression / Goal", ref prog, 128)) _newListing.Progression = prog;

            ImGui.Spacing();
            ImGui.TextColored(Ui.Gold, "2. Region & Language");
            int regIdx = Array.IndexOf(RecruitmentConstants.Regions, _newListing.Region);
            if (regIdx < 0) regIdx = 0;
            ImGui.SetNextItemWidth(140f * scale);
            if (ImGui.Combo("Region", ref regIdx, RecruitmentConstants.Regions, RecruitmentConstants.Regions.Length))
            {
                _newListing.Region = RecruitmentConstants.Regions[regIdx];
                _newListing.Datacenter = string.Empty;
            }

            ImGui.SameLine(0f, 20f * scale);
            ImGui.BeginGroup();
            ImGui.TextColored(Ui.White, "Languages:");
            ImGui.SameLine(0f, 8f * scale);
            string[] supportedLangs = ["FR", "EN", "DE", "JP"];
            foreach (var lang in supportedLangs)
            {
                bool hasLang = _newListing.Languages != null && _newListing.Languages.Contains(lang);
                if (hasLang) ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
                if (ImGui.Button($"{lang}##new_lang_{lang}", new Vector2(42f * scale, 24f * scale)))
                {
                    _newListing.Languages ??= [];
                    if (hasLang)
                    {
                        if (_newListing.Languages.Count > 1) _newListing.Languages.Remove(lang);
                    }
                    else
                    {
                        _newListing.Languages.Add(lang);
                    }
                }
                if (hasLang) ImGui.PopStyleColor();
                ImGui.SameLine(0f, 4f * scale);
            }
            ImGui.EndGroup();

            ImGui.Spacing();
            ImGui.TextColored(Ui.Gold, "3. Schedule & Hours");
            string tStart = _newListing.ScheduleTimeStart;
            DrawTimePicker("Start Time", ref tStart, scale);
            _newListing.ScheduleTimeStart = tStart;

            ImGui.SameLine(0f, 16f * scale);
            string tEnd = _newListing.ScheduleTimeEnd;
            DrawTimePicker("End Time", ref tEnd, scale);
            _newListing.ScheduleTimeEnd = tEnd;

            ImGui.Spacing();
            ImGui.TextColored(Ui.White, "Schedule Days:");
            for (int i = 0; i < RecruitmentConstants.DaysOfWeek.Length; i++)
            {
                var day = RecruitmentConstants.DaysOfWeek[i];
                bool hasDay = _newListing.ScheduleDays != null && _newListing.ScheduleDays.Contains(day);
                if (ImGui.Checkbox($"{day}##new_day_{day}", ref hasDay))
                {
                    _newListing.ScheduleDays ??= [];
                    if (hasDay) _newListing.ScheduleDays.Add(day);
                    else _newListing.ScheduleDays.Remove(day);
                }

                if (i < RecruitmentConstants.DaysOfWeek.Length - 1)
                {
                    if (ImGui.GetItemRectMax().X + 110f * scale < ImGui.GetWindowPos().X + availChild)
                    {
                        ImGui.SameLine(0f, 10f * scale);
                    }
                }
            }
            ImGui.NewLine();

            ImGui.Spacing();
            ImGui.TextColored(Ui.Gold, "4. Roles Needed");
            for (int i = 0; i < RecruitmentConstants.Roles.Length; i++)
            {
                var role = RecruitmentConstants.Roles[i];
                bool needed = _newListing.RolesNeeded != null && _newListing.RolesNeeded.Contains(role);
                if (ImGui.Checkbox($"{role}##role_req_{role}", ref needed))
                {
                    _newListing.RolesNeeded ??= [];
                    if (needed) _newListing.RolesNeeded.Add(role);
                    else _newListing.RolesNeeded.Remove(role);
                }

                if (i < RecruitmentConstants.Roles.Length - 1)
                {
                    if (ImGui.GetItemRectMax().X + 120f * scale < ImGui.GetWindowPos().X + availChild)
                    {
                        ImGui.SameLine(0f, 10f * scale);
                    }
                }
            }
            ImGui.NewLine();

            ImGui.Spacing();
            ImGui.TextColored(Ui.Gold, "5. Description & Atmosphere");
            string desc = _newListing.Description;
            if (ImGui.InputTextMultiline("##new_desc", ref desc, 1024, new Vector2(-1f, 75f * scale)))
            {
                _newListing.Description = desc;
            }

            ImGui.Spacing();
            ImGui.TextColored(Ui.Gold, "6. Custom Tags (Separated by commas)");
            ImGui.TextColored(Ui.Dimmed, "Configure tags like 'Serious Environment', 'Discord Voice', 'Casual', 'Blind Prog', etc.");
            ImGui.SetNextItemWidth(availChild - 20f * scale);
            if (ImGui.InputTextWithHint("##custom_tags_csv", "Ex: Serious Environment, Discord Voice, Standard Pastebin Strats...", ref _inputTagsString, 256))
            {
                _newListing.Tags = _inputTagsString
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            ImGui.Spacing();
            ImGui.TextColored(Ui.Gold, "7. Contact Sharing Options (Upon Accepted Application)");
            ImGui.TextColored(Ui.Dimmed, "Define which contact coordinates will be revealed to the applicant once you ACCEPT their application:");
            
            bool shareDisc = _newListing.ShareDiscordOnAccept;
            if (ImGui.Checkbox("Share my Discord Tag when an application is accepted##pf_share_disc", ref shareDisc))
            {
                _newListing.ShareDiscordOnAccept = shareDisc;
            }

            bool sharePhone = _newListing.ShareAetherphoneOnAccept;
            if (ImGui.Checkbox("Share my Aetherphone number when an application is accepted##pf_share_phone", ref sharePhone))
            {
                _newListing.ShareAetherphoneOnAccept = sharePhone;
            }

            if (!_newListing.ShareDiscordOnAccept && !_newListing.ShareAetherphoneOnAccept)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Ui.Red);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Text(FontAwesomeIcon.ExclamationTriangle.ToIconString());
                ImGui.PopFont();
                ImGui.SameLine(0f, 4f * scale);
                ImGui.Text("ERROR: You must enable at least one contact sharing option (Discord or Aetherphone) to save.");
                ImGui.PopStyleColor();
            }
            else if (_newListing.ShareAetherphoneOnAccept && string.IsNullOrWhiteSpace(_plugin.Configuration.AccountAetherphone) && string.IsNullOrWhiteSpace(_newListing.AuthorAetherphone))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Ui.Gold);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Text(FontAwesomeIcon.ExclamationTriangle.ToIconString());
                ImGui.PopFont();
                ImGui.SameLine(0f, 4f * scale);
                ImGui.Text("WARNING: Aetherphone is selected but no number is configured in the 'Account / Discord' tab.");
                ImGui.PopStyleColor();
            }

            ImGui.EndChild();

            ImGui.Separator();
            ImGui.Spacing();

            string actionLabel = _isEditingPf ? "Save & Update Listing" : "Publish Listing on Replica PF";
            ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
            if (ImGui.Button(actionLabel, new Vector2(260f * scale, 34f * scale)))
            {
                _ = SaveOrPublishListingAsync();
            }
            ImGui.PopStyleColor();

            ImGui.SameLine(0f, 12f * scale);
            if (ImGui.Button("Cancel", new Vector2(100f * scale, 34f * scale)))
            {
                _showCreatePfModal = false;
            }

            if (!string.IsNullOrEmpty(_createStatusMessage))
            {
                ImGui.SameLine(0f, 12f * scale);
                ImGui.TextColored(Ui.Red, _createStatusMessage);
            }

            ImGui.EndPopup();
        }
    }

    private async System.Threading.Tasks.Task SaveOrPublishListingAsync()
    {
        if (!Service.IsAuthenticated)
        {
            _createStatusMessage = "Please link your Discord account first in the 'Account / Discord' tab.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) && string.IsNullOrWhiteSpace(_newListing.AuthorDisplayName))
        {
            _createStatusMessage = "ERROR: Nickname is mandatory. Please set it in 'Account / Discord'.";
            return;
        }

        if (!_newListing.ShareDiscordOnAccept && !_newListing.ShareAetherphoneOnAccept)
        {
            _createStatusMessage = "ERROR: You must enable at least one contact option (Discord or Aetherphone).";
            return;
        }

        if (_newListing.ShareAetherphoneOnAccept && string.IsNullOrWhiteSpace(_plugin.Configuration.AccountAetherphone) && string.IsNullOrWhiteSpace(_newListing.AuthorAetherphone))
        {
            _createStatusMessage = "ERROR: Aetherphone is selected but no number is configured in 'Account / Discord'.";
            return;
        }

        _newListing.Tags = _inputTagsString
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        _createStatusMessage = _isEditingPf ? "Updating listing..." : "Publishing listing...";

        if (!_isEditingPf)
        {
            // Safety check: enforce max listings limit
            int currentCount = GetMyActiveListingCount();
            if (currentCount >= MaxListingsPerUser)
            {
                _createStatusMessage = $"ERROR: You already have {currentCount} active listing(s). Maximum is {MaxListingsPerUser}. Please delete one first.";
                return;
            }

            _newListing.Id = Guid.NewGuid().ToString();
            _newListing.UserId = Service.CurrentUserId;
            _newListing.AuthorDiscordId = Service.CurrentDiscordId ?? string.Empty;
            _newListing.AuthorDiscordTag = Service.CurrentDiscordTag ?? _editingProfile.DiscordTag;
            _newListing.AuthorDisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) 
                ? _plugin.Configuration.AccountPseudo 
                : (!string.IsNullOrWhiteSpace(_editingProfile.DisplayName) ? _editingProfile.DisplayName : _newListing.AuthorDiscordTag);
            _newListing.AuthorAetherphone = _plugin.Configuration.AccountAetherphone;
            _newListing.AuthorAvatarUrl = Service.CurrentAvatarUrl;
            _newListing.CreatedAt = DateTime.UtcNow;
            _newListing.UpdatedAt = DateTime.UtcNow;
            _newListing.BumpedAt = DateTime.UtcNow;
            _newListing.ExpiresAt = DateTime.UtcNow.AddDays(14);
            _newListing.Status = "OPEN";

            bool success = await Service.CreateListingAsync(_newListing);
            if (success)
            {
                _createStatusMessage = "Listing published successfully for 14 days!";
                _ = RefreshListingsAsync();
                await System.Threading.Tasks.Task.Delay(1200);
                _showCreatePfModal = false;
            }
            else
            {
                _createStatusMessage = $"Error: {Service.LastError}";
            }
        }
        else
        {
            _newListing.AuthorDisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) 
                ? _plugin.Configuration.AccountPseudo 
                : _newListing.AuthorDisplayName;
            _newListing.AuthorAetherphone = _plugin.Configuration.AccountAetherphone;
            _newListing.UpdatedAt = DateTime.UtcNow;

            bool success = await Service.UpdateListingAsync(_newListing);
            if (success)
            {
                _createStatusMessage = "Listing updated successfully!";
                _ = RefreshListingsAsync();
                _ = RefreshMyApplicationsAndListingsAsync();
                await System.Threading.Tasks.Task.Delay(1200);
                _showCreatePfModal = false;
            }
            else
            {
                _createStatusMessage = $"Error: {Service.LastError}";
            }
        }
    }
    #endregion

    #region Tab 3: Account / Discord Auth Settings
    private void DrawAuthSettingsTab(float scale, float availWidth)
    {
        if (!Service.IsAuthenticated)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.35f, 0.15f, 0.15f, 0.6f));
            ImGui.BeginChild("##auth_required_banner", new Vector2(availWidth - 40f * scale, 48f * scale), true);
            ImGui.PushStyleColor(ImGuiCol.Text, Ui.Gold);
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.Text(FontAwesomeIcon.Lock.ToIconString());
            ImGui.PopFont();
            ImGui.SameLine(0f, 4f * scale);
            ImGui.Text("Party Finder Locked — Discord Connection Required");
            ImGui.PopStyleColor();
            ImGui.TextColored(Ui.White, "Please connect your Discord account below to unlock Party Finder listings, candidate profiles, and applications.");
            ImGui.EndChild();
            ImGui.PopStyleColor();
            ImGui.Spacing();
        }

        // 1. Identity & Public Nickname
        Ui.SectionHeader(FontAwesomeIcon.User, "Player Identity & Public Nickname");
        ImGui.TextColored(Ui.Dimmed, "Configure your public recruitment nickname (used on all Party Finder listings) and optional virtual phone.");
        ImGui.Spacing();

        ImGui.TextColored(Ui.White, "Nickname / Display Name (Mandatory) *");
        string pseudo = _plugin.Configuration.AccountPseudo;
        ImGui.SetNextItemWidth((availWidth - 40f * scale) / 4f);
        if (ImGui.InputTextWithHint("##account_pseudo", "Enter your public nickname...", ref pseudo, 64))
        {
            _plugin.Configuration.AccountPseudo = pseudo;
            _editingProfile.DisplayName = pseudo;
            _plugin.Configuration.Save();
        }

        if (string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo))
        {
            ImGui.TextColored(Ui.Red, "Nickname is mandatory to create a PF or apply!");
        }

        ImGui.Spacing();
        ImGui.TextColored(Ui.White, "Aetherphone (Optional - Virtual in-game phone from another plugin)");
        string aphone = _plugin.Configuration.AccountAetherphone;
        ImGui.SetNextItemWidth((availWidth - 40f * scale) / 4f);
        if (ImGui.InputTextWithHint("##account_aetherphone", "Ex: #555-0199 or Aetherphone handle...", ref aphone, 64))
        {
            _plugin.Configuration.AccountAetherphone = aphone;
            _editingProfile.Aetherphone = aphone;
            _plugin.Configuration.Save();
        }
        ImGui.SameLine(0f, 6f * scale);
        if (Ui.IconButton(FontAwesomeIcon.ExternalLinkAlt, "", "aetherphone_install_link", new Vector2(28f * scale, 0f), scale))
        {
            Dalamud.Utility.Util.OpenLink("https://www.aetherphone.net");
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Visit https://www.aetherphone.net to get / install Aetherphone");
        }

        ImGui.Spacing();
        ImGui.Separator();

        // 2. Privacy & Sharing Preferences
        Ui.SectionHeader(FontAwesomeIcon.ShareAlt, "Privacy & Contact Sharing Preferences");
        ImGui.TextColored(Ui.Dimmed, "Choose what contact coordinates will be revealed to the recruiter or applicant once an application is ACCEPTED.");
        ImGui.Spacing();

        bool shareDiscord = _plugin.Configuration.ShareDiscordOnAccept;
        if (ImGui.Checkbox("Share my Discord Tag when an application is accepted", ref shareDiscord))
        {
            _plugin.Configuration.ShareDiscordOnAccept = shareDiscord;
            _editingProfile.ShareDiscordOnAccept = shareDiscord;
            _plugin.Configuration.Save();
        }

        bool shareAetherphone = _plugin.Configuration.ShareAetherphoneOnAccept;
        if (ImGui.Checkbox("Share my Aetherphone number when an application is accepted", ref shareAetherphone))
        {
            _plugin.Configuration.ShareAetherphoneOnAccept = shareAetherphone;
            _editingProfile.ShareAetherphoneOnAccept = shareAetherphone;
            _plugin.Configuration.Save();
        }

        if (!_plugin.Configuration.ShareDiscordOnAccept && !_plugin.Configuration.ShareAetherphoneOnAccept)
        {
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.35f, 0.1f, 0.1f, 0.5f));
            ImGui.BeginChild("##sharing_warning", new Vector2(availWidth - 40f * scale, 34f * scale), true);
            ImGui.TextColored(Ui.Red, "You must enable at least one contact sharing option (Discord or Aetherphone) to apply or create a PF.");
            ImGui.EndChild();
            ImGui.PopStyleColor();
        }

        ImGui.Spacing();
        ImGui.Separator();

        // 3. Discord Account Link
        Ui.SectionHeader(FontAwesomeIcon.Comments, "Discord Account Link");
        ImGui.Spacing();

        if (Service.IsAuthenticated)
        {
            ImGui.TextColored(Ui.Green, $"Connected as: {Service.CurrentDiscordTag}");
            ImGui.Spacing();

            if (ImGui.Button("Disconnect from Discord", new Vector2(220f * scale, 28f * scale)))
            {
                Service.RestoreFromCache(null, null, null, null, null, null, null);
                _plugin.Configuration.DiscordAuthToken = null;
                _plugin.Configuration.DiscordRefreshToken = null;
                _plugin.Configuration.DiscordUserId = null;
                _plugin.Configuration.DiscordTag = null;
                _plugin.Configuration.DiscordAvatarUrl = null;
                _plugin.Configuration.DiscordTokenExpiresAt = 0;
                _plugin.Configuration.Save();
                DeleteSessionFromDisk();
                _authStatusMessage = "Disconnected.";
            }
        }
        else
        {
            ImGui.TextColored(Ui.Dimmed, "Click the button below: your browser will open to authorize Discord and Replica will log you in automatically!");
            ImGui.Spacing();

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.35f, 0.40f, 0.95f, 1f));
            if (ImGui.Button("Connect with Discord (Automatic)", new Vector2(340f * scale, 38f * scale)))
            {
                _ = DiscordOAuthListener.StartAndOpenAuthAsync(
                    Service.SupabaseUrl,
                    async (token) => await LoginWithTokenAsync(token),
                    (status) => _authStatusMessage = status
                );
            }
            ImGui.PopStyleColor();

            ImGui.Spacing();
            ImGui.Dummy(new Vector2(0f, 10f * scale));
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextColored(Ui.Dimmed, "Manual fallback (if automatic flow is blocked by your firewall):");
            ImGui.Spacing();

            ImGui.SetNextItemWidth(availWidth - 140f * scale);
            ImGui.InputTextWithHint("##redirect_url_input", "Paste redirect URL or token here...", ref _inputRedirectUrlOrToken, 2048);

            ImGui.SameLine(0f, 8f * scale);
            if (ImGui.Button("Validate###manual_val", new Vector2(100f * scale, 26f * scale)))
            {
                _ = LoginWithTokenAsync(_inputRedirectUrlOrToken);
            }
        }

        if (!string.IsNullOrEmpty(_authStatusMessage))
        {
            ImGui.Spacing();
            ImGui.TextColored(Service.IsAuthenticated ? Ui.Green : Ui.Gold, _authStatusMessage);
        }
    }

    private async System.Threading.Tasks.Task LoginWithTokenAsync(string rawTokenOrUrl, string? explicitRefreshToken = null)
    {
        _authStatusMessage = "Checking token...";
        bool success = await Service.LoginWithTokenAsync(rawTokenOrUrl, explicitRefreshToken);
        if (success)
        {
            _plugin.Configuration.DiscordAuthToken = Service.AuthToken;
            _plugin.Configuration.DiscordRefreshToken = Service.RefreshToken;

            if (string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) && !string.IsNullOrEmpty(Service.CurrentDiscordTag))
            {
                _plugin.Configuration.AccountPseudo = Service.CurrentDiscordTag;
            }

            _plugin.Configuration.Save();

            if (!string.IsNullOrEmpty(Service.CurrentDiscordTag))
            {
                _editingProfile.DiscordTag = Service.CurrentDiscordTag;
                _editingProfile.DisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) ? _plugin.Configuration.AccountPseudo : Service.CurrentDiscordTag;
                _editingProfile.DiscordId = Service.CurrentDiscordId ?? string.Empty;
                _editingProfile.UserId = Service.CurrentUserId;
            }

            // Fetch static API key from candidate_profiles for permanent silent reconnection
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await FetchAndStoreApiKeyAsync();
                }
                catch (Exception ex)
                {
                    Plugin.Log.Warning($"[Replica] Could not fetch API key after login: {ex.Message}");
                }
            });

            _authStatusMessage = $"Successfully connected: {Service.CurrentDiscordTag}!";
            _selectedSubTab = 0;
            _ = RefreshListingsAsync();
        }
        else
        {
            _authStatusMessage = $"Error: {Service.LastError}";
        }
    }

    /// <summary>
    /// Fetches the static replica_api_key via Supabase RPC and stores it locally in replica_session.json.
    /// This enables permanent silent reconnection without needing OAuth refresh tokens.
    /// </summary>
    private async System.Threading.Tasks.Task FetchAndStoreApiKeyAsync()
    {
        try
        {
            var apiKey = await Service.FetchOrCreateApiKeyAsync();
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Persist to disk immediately
                SaveSessionToDisk(
                    Service.AuthToken, Service.RefreshToken, Service.CurrentUserId,
                    Service.CurrentDiscordTag, Service.CurrentDiscordId,
                    Service.CurrentAvatarUrl, Service.TokenExpiresAt, apiKey
                );
                Plugin.Log.Information("[Replica] Session successfully saved with permanent API key!");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[Replica] FetchAndStoreApiKeyAsync exception: {ex.Message}");
        }
    }

    #endregion

    #region Apply Modal Dialog
    private void DrawApplyModal(float scale)
    {
        ImGui.OpenPopup("Apply to Listing###ApplyModal");

        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(560f * scale, 480f * scale), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(460f * scale, 360f * scale), new Vector2(1600f * scale, 1200f * scale));

        if (ImGui.BeginPopupModal("Apply to Listing###ApplyModal", ref _showApplyModal, ImGuiWindowFlags.None))
        {
            if (!ValidateContactSettings(out var contactWarning))
            {
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.35f, 0.1f, 0.1f, 0.5f));
                ImGui.BeginChild("##apply_contact_warning", new Vector2(0f, 34f * scale), true);
                ImGui.TextColored(Ui.Red, contactWarning);
                ImGui.EndChild();
                ImGui.PopStyleColor();
                ImGui.Spacing();
            }

            float bottomReserved = 48f * scale;
            ImGui.BeginChild("##apply_modal_scroll", new Vector2(0f, -bottomReserved), false);

            ImGui.TextColored(Ui.Gold, $"Application for: {_selectedListingDetails?.TargetDuty}");
            string authorName = !string.IsNullOrWhiteSpace(_selectedListingDetails?.AuthorDisplayName) ? _selectedListingDetails.AuthorDisplayName : "Static Leader";
            ImGui.TextColored(Ui.Dimmed, $"Author: {authorName}");
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextColored(Ui.White, "Jobs you can play:");
            var userJobs = _editingProfile.MainJobs.Concat(_editingProfile.SecondaryJobs).Distinct().ToList();
            if (userJobs.Count > 0)
            {
                ImGui.BeginGroup();
                for (int i = 0; i < userJobs.Count; i++)
                {
                    var job = userJobs[i];
                    var tex = GetJobIconTexture(job);
                    var wrap = tex?.GetWrapOrDefault();
                    if (wrap != null)
                    {
                        ImGui.Image(wrap.Handle, new Vector2(20f * scale, 20f * scale));
                        ImGui.SameLine(0f, 4f * scale);
                    }
                    ImGui.TextColored(Ui.Accent, job);
                    if (i < userJobs.Count - 1)
                    {
                        ImGui.SameLine(0f, 12f * scale);
                    }
                }
                ImGui.EndGroup();
            }
            else
            {
                ImGui.TextColored(Ui.Red, "No jobs selected in your profile! Please edit your profile first.");
            }

            ImGui.Spacing();
            ImGui.TextColored(Ui.White, "Preview of your profile that will be sent:");
            string candidatePseudo = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) ? _plugin.Configuration.AccountPseudo : _editingProfile.DisplayName;
            ImGui.BulletText($"Candidate: {candidatePseudo} (ilvl {_editingProfile.Ilvl})");
            
            ImGui.Bullet();
            ImGui.SameLine(0f, 4f * scale);
            ImGui.Text("Contacts shared on acceptance: ");
            if (!_plugin.Configuration.ShareDiscordOnAccept && !_plugin.Configuration.ShareAetherphoneOnAccept)
            {
                ImGui.SameLine(0f, 0f);
                ImGui.TextColored(Ui.Red, "None (Blocked)");
            }
            else
            {
                if (_plugin.Configuration.ShareDiscordOnAccept)
                {
                    ImGui.SameLine(0f, 0f);
                    ImGui.TextColored(Ui.DiscordColor, "Discord");
                }
                if (_plugin.Configuration.ShareAetherphoneOnAccept)
                {
                    if (_plugin.Configuration.ShareDiscordOnAccept)
                    {
                        ImGui.SameLine(0f, 0f);
                        ImGui.Text(" & ");
                    }
                    ImGui.SameLine(0f, 0f);
                    ImGui.TextColored(Ui.AetherphoneColor, "Aetherphone");
                }
            }

            string npwPreview = !string.IsNullOrWhiteSpace(_editingProfile.NightsPerWeek) ? $" | {_editingProfile.NightsPerWeek} nights/week" : "";
            ImGui.BulletText($"Availabilities: {string.Join(", ", _editingProfile.AvailableDays)} ({_editingProfile.PreferredTimeStart}-{_editingProfile.PreferredTimeEnd}){npwPreview}");
            var activePlugins = new List<string>();
            if (_editingProfile.PluginsUsed.BossMod) activePlugins.Add("BossMod");
            if (_editingProfile.PluginsUsed.Splatoon) activePlugins.Add("Splatoon");
            if (_editingProfile.PluginsUsed.Wrath) activePlugins.Add("Wrath Combo");
            if (_editingProfile.PluginsUsed.Rsr) activePlugins.Add("Rotation Solver Reborn");
            if (_editingProfile.PluginsUsed.Replica) activePlugins.Add("Replica");
            if (_editingProfile.PluginsUsed.Artisan) activePlugins.Add("Artisan");
            if (_editingProfile.PluginsUsed.Cactbot) activePlugins.Add("Cactbot / ACT");
            if (_editingProfile.PluginsUsed.ModBeast) activePlugins.Add("ModBeast");
            string activePluginsStr = activePlugins.Count > 0 ? string.Join(", ", activePlugins) : "None";
            ImGui.BulletText($"Plugins: {activePluginsStr}");

            ImGui.Spacing();
            ImGui.TextColored(Ui.White, "Cover message (Optional):");
            ImGui.InputTextMultiline("##custom_msg", ref _applyMessage, 512, new Vector2(-1f, 70f * scale));

            ImGui.EndChild();

            ImGui.Separator();
            ImGui.Spacing();

            ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
            if (Ui.IconButton(FontAwesomeIcon.Envelope, "Confirm & Send Application", "confirm_send_app", new Vector2(280f * scale, 34f * scale), scale))
            {
                _ = SubmitApplicationAsync();
            }
            ImGui.PopStyleColor();

            ImGui.SameLine(0f, 10f * scale);
            if (ImGui.Button("Cancel", new Vector2(100f * scale, 34f * scale)))
            {
                _showApplyModal = false;
            }

            if (!string.IsNullOrEmpty(_applyStatusMessage))
            {
                ImGui.SameLine(0f, 12f * scale);
                ImGui.TextColored(Ui.Green, _applyStatusMessage);
            }

            ImGui.EndPopup();
        }
    }

    private async System.Threading.Tasks.Task SubmitApplicationAsync()
    {
        if (_selectedListingDetails == null) return;

        if (!ValidateContactSettings(out var validationError))
        {
            _applyStatusMessage = validationError;
            return;
        }

        _applyStatusMessage = "Submitting application...";

        // Ensure profile snapshot is populated with latest account identity & contact sharing flags
        _editingProfile.DisplayName = !string.IsNullOrWhiteSpace(_plugin.Configuration.AccountPseudo) 
            ? _plugin.Configuration.AccountPseudo 
            : _editingProfile.DisplayName;
        _editingProfile.Aetherphone = _plugin.Configuration.AccountAetherphone;
        _editingProfile.ShareDiscordOnAccept = _plugin.Configuration.ShareDiscordOnAccept;
        _editingProfile.ShareAetherphoneOnAccept = _plugin.Configuration.ShareAetherphoneOnAccept;
        _applySelectedJob = string.Join(", ", _editingProfile.MainJobs.Concat(_editingProfile.SecondaryJobs).Distinct());

        bool success = await Service.ApplyToListingAsync(
            _selectedListingDetails.Id,
            _applySelectedJob,
            _applySelectedRole,
            _applyMessage,
            _editingProfile
        );

        if (success)
        {
            _applyStatusMessage = "Application sent successfully to the static leader!";

            RecruitmentToastOverlay.AddToast(new RecruitmentToast
            {
                Kind = RecruitmentToastKind.ApplicationSent,
                Title = "Application Sent!",
                Message = $"Your application for {_selectedListingDetails.TargetDuty} has been delivered to the static leader!",
                Icon = FontAwesomeIcon.PaperPlane,
                OnOpen = () => _plugin.ShowRecruitment(2)
            });

            await System.Threading.Tasks.Task.Delay(1200);
            _showApplyModal = false;
        }
        else
        {
            _applyStatusMessage = $"Error: {Service.LastError}";
        }
    }

    private async System.Threading.Tasks.Task CancelApplicationAsync(string appId)
    {
        bool success = await Service.DeleteApplicationAsync(appId);
        if (success)
        {
            _mySentApplications.RemoveAll(a => a.Id == appId);
            
            // Also remove from any received applications cached lists so it disappears immediately from listing cards
            foreach (var kvp in _receivedApplicationsByListing)
            {
                kvp.Value.RemoveAll(a => a.Id == appId);
            }
            
            RecruitmentToastOverlay.AddToast(new RecruitmentToast
            {
                Kind = RecruitmentToastKind.GeneralInfo,
                Title = "Application Cancelled",
                Message = "Your application has been cancelled and deleted.",
                Icon = FontAwesomeIcon.TrashAlt
            });
        }
        else
        {
            RecruitmentToastOverlay.AddToast(new RecruitmentToast
            {
                Kind = RecruitmentToastKind.GeneralInfo,
                Title = "Error",
                Message = $"Failed to cancel application: {Service.LastError}",
                Icon = FontAwesomeIcon.ExclamationTriangle
            });
        }
    }
    #endregion

    public void Dispose()
    {
        Service.Dispose();
    }

    private static string FormatSchedule(RecruitmentListing listing)
    {
        var days = (listing.ScheduleDays != null && listing.ScheduleDays.Count > 0) ? string.Join(", ", listing.ScheduleDays) : "Not specified";
        if (string.IsNullOrWhiteSpace(listing.ScheduleTimeStart) || string.IsNullOrWhiteSpace(listing.ScheduleTimeEnd) || string.IsNullOrWhiteSpace(listing.ScheduleTimezone))
        {
            return days;
        }

        try
        {
            var sourceTz = TimeZoneInfo.FindSystemTimeZoneById(listing.ScheduleTimezone);
            var localTz = TimeZoneInfo.Local;

            // If they have the same offset/rules, just display standard format
            if (sourceTz.Id == localTz.Id || sourceTz.HasSameRules(localTz))
            {
                return $"{days} ({listing.ScheduleTimeStart} - {listing.ScheduleTimeEnd} {listing.ScheduleTimezone})";
            }

            if (TimeSpan.TryParse(listing.ScheduleTimeStart, out var startTime) && TimeSpan.TryParse(listing.ScheduleTimeEnd, out var endTime))
            {
                var now = DateTime.UtcNow;
                var sourceToday = TimeZoneInfo.ConvertTimeFromUtc(now, sourceTz);

                var sourceStart = new DateTime(sourceToday.Year, sourceToday.Month, sourceToday.Day, startTime.Hours, startTime.Minutes, 0, DateTimeKind.Unspecified);
                var sourceEnd = new DateTime(sourceToday.Year, sourceToday.Month, sourceToday.Day, endTime.Hours, endTime.Minutes, 0, DateTimeKind.Unspecified);

                if (endTime < startTime)
                {
                    sourceEnd = sourceEnd.AddDays(1);
                }

                var localStart = TimeZoneInfo.ConvertTime(sourceStart, sourceTz, localTz);
                var localEnd = TimeZoneInfo.ConvertTime(sourceEnd, sourceTz, localTz);

                var localStartStr = localStart.ToString("HH:mm");
                var localEndStr = localEnd.ToString("HH:mm");

                var dayShift = (localStart.Date - sourceStart.Date).Days;
                if (dayShift != 0 && listing.ScheduleDays != null && listing.ScheduleDays.Count > 0)
                {
                    var shiftedDays = new List<string>();
                    foreach (var d in listing.ScheduleDays)
                    {
                        if (Enum.TryParse<DayOfWeek>(d, true, out var dow))
                        {
                            var shiftedDow = (DayOfWeek)(((int)dow + dayShift + 7) % 7);
                            shiftedDays.Add(shiftedDow.ToString());
                        }
                        else
                        {
                            shiftedDays.Add(d);
                        }
                    }
                    var localDaysStr = string.Join(", ", shiftedDays);
                    return $"local: {localDaysStr} ({localStartStr} - {localEndStr}) | orig: {days} ({listing.ScheduleTimeStart} - {listing.ScheduleTimeEnd} {listing.ScheduleTimezone})";
                }

                return $"{days} ({localStartStr} - {localEndStr} local) | orig: {listing.ScheduleTimeStart} - {listing.ScheduleTimeEnd} {listing.ScheduleTimezone}";
            }
        }
        catch
        {
            // Fallback on exception
        }

        return $"{days} ({listing.ScheduleTimeStart} - {listing.ScheduleTimeEnd} {listing.ScheduleTimezone})";
    }

    #region Job Icons Helpers
    private ISharedImmediateTexture? GetJobIconTexture(string jobAbbr)
    {
        if (string.IsNullOrEmpty(jobAbbr)) return null;
        if (_jobIconTextures.TryGetValue(jobAbbr, out var tex)) return tex;

        var sheet = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.ClassJob>();
        if (sheet != null)
        {
            foreach (var row in sheet)
            {
                if (row.Abbreviation.ToString().Equals(jobAbbr, StringComparison.OrdinalIgnoreCase))
                {
                    uint iconId = 62100 + row.RowId;
                    var t = Plugin.TextureProvider.GetFromGameIcon(new Dalamud.Interface.Textures.GameIconLookup(iconId));
                    _jobIconTextures[jobAbbr] = t;
                    return t;
                }
            }
        }
        return null;
    }

    private void DrawJobSelection(float scale, List<string> selectedJobs, Action<string, bool> onToggle, string uniqueId)
    {
        string[] roles = ["Tank", "PureHealer", "ShieldHealer", "Melee", "PhysRanged", "Caster"];
        float spacing = 6f * scale;
        bool first = true;

        foreach (var role in roles)
        {
            if (!RecruitmentConstants.JobsByRole.TryGetValue(role, out var jobs) || jobs == null)
                continue;

            if (!first)
            {
                ImGui.SameLine(0f, 18f * scale);
            }
            first = false;

            foreach (var job in jobs)
            {
                var tex = GetJobIconTexture(job);
                var wrap = tex?.GetWrapOrDefault();
                bool isSelected = selectedJobs.Contains(job);

                if (wrap != null)
                {
                    if (isSelected)
                    {
                        Vector4 borderCol = uniqueId == "main" ? Ui.Accent : Ui.Blue;
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(borderCol.X, borderCol.Y, borderCol.Z, 0.35f));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(borderCol.X, borderCol.Y, borderCol.Z, 0.55f));
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0f, 0f, 0f, 0.2f));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 0.4f));
                    }

                    ImGui.PushID($"{uniqueId}_{job}");
                    Vector4 tintColor = isSelected ? new Vector4(1f, 1f, 1f, 1f) : new Vector4(0.45f, 0.45f, 0.45f, 0.55f);
                    if (ImGui.ImageButton(wrap.Handle, new Vector2(24f * scale, 24f * scale), Vector2.Zero, Vector2.One, 2, Vector4.Zero, tintColor))
                    {
                        onToggle(job, !isSelected);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(job);
                    }
                    ImGui.PopID();
                    ImGui.PopStyleColor(2);
                }
                else
                {
                    // Fallback to text button
                    if (isSelected) ImGui.PushStyleColor(ImGuiCol.Button, uniqueId == "main" ? Ui.Accent : Ui.Blue);
                    if (ImGui.Button($"{job}##{uniqueId}_{job}", new Vector2(48f * scale, 26f * scale)))
                    {
                        onToggle(job, !isSelected);
                    }
                    if (isSelected) ImGui.PopStyleColor();
                }
                ImGui.SameLine(0f, spacing);
            }
        }
        ImGui.NewLine();
    }

    private void DrawTimePicker(string label, ref string timeString, float scale)
    {
        int hour = 20;
        int minute = 0;
        if (!string.IsNullOrEmpty(timeString))
        {
            var parts = timeString.Split(':');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int h) && int.TryParse(parts[1], out int m))
            {
                hour = Math.Clamp(h, 0, 23);
                int r = m % 15;
                if (r < 8) minute = m - r;
                else minute = m + (15 - r);
                if (minute >= 60)
                {
                    minute = 0;
                    hour = (hour + 1) % 24;
                }
                minute = Math.Clamp(minute, 0, 45);
            }
        }

        ImGui.BeginGroup();
        ImGui.TextColored(Ui.White, label);
        
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6f, 4f) * scale);
        
        ImGui.SetNextItemWidth(60f * scale);
        if (ImGui.BeginCombo($"##{label}_hour", $"{hour:D2}h"))
        {
            for (int h = 0; h < 24; h++)
            {
                bool isSelected = (h == hour);
                if (ImGui.Selectable($"{h:D2}h", isSelected))
                {
                    hour = h;
                    timeString = $"{hour:D2}:{minute:D2}";
                }
                if (isSelected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        
        ImGui.SameLine(0f, 4f * scale);
        ImGui.TextColored(Ui.Dimmed, ":");
        ImGui.SameLine(0f, 4f * scale);
        
        ImGui.SetNextItemWidth(60f * scale);
        if (ImGui.BeginCombo($"##{label}_min", $"{minute:D2}m"))
        {
            int[] minutes = [0, 15, 30, 45];
            for (int i = 0; i < minutes.Length; i++)
            {
                int m = minutes[i];
                bool isSelected = (m == minute);
                if (ImGui.Selectable($"{m:D2}m", isSelected))
                {
                    minute = m;
                    timeString = $"{hour:D2}:{minute:D2}";
                }
                if (isSelected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        
        ImGui.PopStyleVar();
        ImGui.EndGroup();
    }
    #endregion
}
