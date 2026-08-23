using System;
using System.Collections;
using System.Reflection;
using Replica.Engine.Bridge.BossMod.Reflection;

namespace Replica.Engine.Bridge.BossMod.Core;

public sealed class BossModLocator
{
    private object? _cachedManager;

    public object? GetManager()
    {
        if (_cachedManager != null) return _cachedManager;

        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var asmName = asm.GetName().Name;
                if (!string.Equals(asmName, "BossModReborn", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(asmName, "BossMod",       StringComparison.OrdinalIgnoreCase))
                    continue;

                var svc = asm.GetType("BossMod.Service");
                if (svc == null) continue;
                var wsField = svc.GetField("WindowSystem",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var ws = wsField?.GetValue(null);
                if (ws == null) continue;
                var winProp = ws.GetType().GetProperty("Windows", BindingFlags.Public | BindingFlags.Instance);
                if (winProp?.GetValue(ws) is not IEnumerable wins) continue;
                foreach (var win in wins)
                {
                    if (win?.GetType().Name == "BossModuleMainWindow" || win?.GetType().Name == "BossModuleHintsWindow")
                    {
                        var mgr = BossModReflection.GetField(win, "_mgr") ?? BossModReflection.GetField(win, "_bossmod");
                        if (mgr != null)
                        {
                            _cachedManager = mgr;
                            return mgr;
                        }
                    }
                }
            }
        }
        catch { }
        return null;
    }

    public object? GetActiveModule()
    {
        var mgr = GetManager();
        if (mgr == null) return null;

        var active = BossModReflection.GetField(mgr, "ActiveModule");
        if (active != null) return active;

        // Fallback: Check LoadedModules if active is not explicitly set yet
        if (BossModReflection.GetField(mgr, "LoadedModules") is IList loaded && loaded.Count > 0)
        {
            return loaded[0];
        }

        return null;
    }

    public bool IsBossModActive() => GetActiveModule() != null;

    public string GetStatusText(int activeVfxCount)
    {
        try
        {
            var m = GetActiveModule();
            return m == null
                ? (GetManager() != null ? "BossMod: Connected (no combat)" : "BossMod: Standby")
                : $"BossMod: {m.GetType().Name} | {activeVfxCount} VFX";
        }
        catch
        {
            return "BossMod: Error";
        }
    }
}
