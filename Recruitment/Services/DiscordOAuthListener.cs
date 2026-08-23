using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Replica.Recruitment.Services;

public static class DiscordOAuthListener
{
    private const int Port = 45892;
    private static readonly string Prefix = $"http://127.0.0.1:{Port}/";
    private static HttpListener? _listener;
    private static CancellationTokenSource? _cts;

    public static async Task StartAndOpenAuthAsync(string supabaseUrl, Func<string, Task> onTokenReceived, Action<string> onStatus)
    {
        Replica.Plugin.Log.Information("[Replica] Starting Discord OAuth local listener...");
        Stop();

        try
        {
            _cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            _listener = new HttpListener();
            _listener.Prefixes.Add(Prefix);
            _listener.Start();
            Replica.Plugin.Log.Information($"[Replica] Discord OAuth local listener started on {Prefix}");

            onStatus("Waiting for Discord authorization in your browser...");

            var redirectUri = Uri.EscapeDataString($"http://127.0.0.1:{Port}/callback/");
            var authUrl = $"{supabaseUrl.TrimEnd('/')}/auth/v1/authorize?provider=discord&redirect_to={redirectUri}";

            Replica.Plugin.Log.Information($"[Replica] Opening browser link for Discord OAuth: {authUrl}");
            Dalamud.Utility.Util.OpenLink(authUrl);

            _ = Task.Run(async () =>
            {
                while (_listener != null && _listener.IsListening && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        var request = context.Request;
                        var response = context.Response;

                        Replica.Plugin.Log.Information($"[Replica] Local listener received request: {request.HttpMethod} {request.RawUrl}");

                        if (request.HttpMethod == "GET" && (request.RawUrl?.StartsWith("/callback") ?? false))
                        {
                            Replica.Plugin.Log.Information("[Replica] Serving callback HTML page to browser.");
                            // Serves HTML that captures the #access_token= hash and POSTs it back to /token
                            string html = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Replica - Discord Authentication</title>
    <style>
        body { background: #121212; color: #f0f0f0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; align-items: center; justify-content: center; height: 100vh; margin: 0; }
        .card { background: #1e1e1e; padding: 40px; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,0.6); text-align: center; max-width: 440px; border: 1px solid #d73f4a; }
        h2 { color: #d73f4a; margin-top: 0; }
        p { color: #aaa; line-height: 1.5; font-size: 15px; }
        .success { color: #4ade80; font-weight: bold; font-size: 16px; margin-top: 20px; }
    </style>
</head>
<body>
    <div class='card'>
        <h2>⚔️ Replica Party Finder</h2>
        <p id='msg'>Finalizing Discord authentication...</p>
        <div class='success' id='succ' style='display:none;'>✅ Authentication successful! You can now close this tab.</div>
    </div>
    <script>
        const hash = window.location.hash;
        if (hash && hash.includes('access_token=')) {
            fetch('/token', {
                method: 'POST',
                headers: { 'Content-Type': 'text/plain' },
                body: hash
            }).then(() => {
                document.getElementById('msg').style.display = 'none';
                document.getElementById('succ').style.display = 'block';
                setTimeout(() => window.close(), 2500);
            }).catch(e => {
                document.getElementById('msg').innerText = 'Error during token transmission: ' + e;
            });
        } else {
            document.getElementById('msg').innerText = 'Token not detected in URL.';
        }
    </script>
</body>
</html>";

                            byte[] buffer = Encoding.UTF8.GetBytes(html);
                            response.ContentType = "text/html; charset=utf-8";
                            response.ContentLength64 = buffer.Length;
                            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            response.OutputStream.Close();
                        }
                        else if (request.HttpMethod == "POST" && request.RawUrl == "/token")
                        {
                            Replica.Plugin.Log.Information("[Replica] Reading token body from POST request.");
                            using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
                            string body = await reader.ReadToEndAsync();
                            Replica.Plugin.Log.Information($"[Replica] Token body read completed. Length: {body?.Length ?? 0}");

                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.Close();

                            if (!string.IsNullOrEmpty(body))
                            {
                                await onTokenReceived(body);
                                break;
                            }
                        }
                        else
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Replica.Plugin.Log.Error($"[Replica] Exception in local listener request processing: {ex.Message}");
                        break;
                    }
                }

                Stop();
            }, _cts.Token);
        }
        catch (Exception ex)
        {
            Replica.Plugin.Log.Error($"[Replica] Failed to start Discord OAuth local listener: {ex.Message}");
            onStatus($"Local listener error: {ex.Message}");
            Stop();
        }
    }

    public static void Stop()
    {
        try
        {
            if (_cts != null || _listener != null)
            {
                Replica.Plugin.Log.Information("[Replica] Stopping Discord OAuth local listener...");
            }
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_listener != null)
            {
                if (_listener.IsListening)
                {
                    _listener.Stop();
                }
                _listener.Close();
                _listener = null;
                Replica.Plugin.Log.Information("[Replica] Discord OAuth local listener stopped.");
            }
        }
        catch (Exception ex)
        {
            Replica.Plugin.Log.Error($"[Replica] Error stopping local listener: {ex.Message}");
        }
    }
}
