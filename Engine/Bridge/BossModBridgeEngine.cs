using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Bridge.BossMod.Overlay;
using Replica.Engine.Bridge.BossMod.Processors;
using Replica.Engine.Bridge.BossMod.Reflection;
using Replica.Engine.Bridge.BossMod.Vfx;
using Replica.Logging;

namespace Replica.Engine.Bridge;

/// <summary>
/// Mirrors BossMod radar shapes, ground AOEs, gameObject-attached AOEs (spreads, stacks, baits, chasers, wild charges),
/// safe zones, safe spot movement arrows, and knockback hints into the 3D game world.
/// </summary>
[SkipLocalsInit]
public sealed class BossModBridgeEngine : IDisposable
{
    private readonly Plugin _plugin;

    // Sub-components
    private readonly BossModLocator _locator;
    private readonly BossModVfxEmitter _vfxEmitter;
    private readonly BossModShapeMapper _shapeMapper;
    private readonly BossModOverlayRenderer _overlay;

    // Processors
    private readonly BossModAoeProcessor _aoeProcessor;
    private readonly BossModMechanicsProcessor _mechanicsProcessor;
    private readonly BossModTetherGazeProcessor _tetherGazeProcessor;
    private readonly BossModAiHintsProcessor _aiHintsProcessor;
    private readonly BossModHintsProcessor _hintsProcessor;
    private readonly BossModDuckTypeCrawler _duckTypeCrawler;

    private readonly List<MapAoe> _activeMapAoes = new(64);

    private bool _isDisposed;
    private int _throttle;

    public BossModBridgeEngine(Plugin plugin)
    {
        _plugin = plugin;
        _locator = new BossModLocator();
        _vfxEmitter = new BossModVfxEmitter();
        _shapeMapper = new BossModShapeMapper(_vfxEmitter);
        _overlay = new BossModOverlayRenderer();

        _aoeProcessor = new BossModAoeProcessor(_shapeMapper, _vfxEmitter);
        _mechanicsProcessor = new BossModMechanicsProcessor(_shapeMapper, _vfxEmitter, _overlay);
        _tetherGazeProcessor = new BossModTetherGazeProcessor(_vfxEmitter, _overlay);
        _aiHintsProcessor = new BossModAiHintsProcessor(_vfxEmitter, _overlay);
        _hintsProcessor = new BossModHintsProcessor(_overlay);
        _duckTypeCrawler = new BossModDuckTypeCrawler(_aoeProcessor, _mechanicsProcessor, _shapeMapper);
    }

    public List<MapAoe> GetActiveMapAoes()
    {
        lock (_activeMapAoes)
        {
            return new List<MapAoe>(_activeMapAoes);
        }
    }

    public string GetStatusText() => _locator.GetStatusText(_vfxEmitter.ActiveCount);

    public bool IsBossModActive() => _locator.IsBossModActive();

    public object? GetActiveBossModule() => _locator.GetActiveModule();

    public string? GetActiveBossName()
    {
        try
        {
            var mod = _locator.GetActiveModule();
            if (mod == null) return null;
            var primaryActor = BossModReflection.Get(mod, "PrimaryActor");
            if (primaryActor != null)
            {
                var name = BossModReflection.Get(primaryActor, "Name") as string;
                if (!string.IsNullOrWhiteSpace(name) && name != "None")
                    return name;
            }
            var info = BossModReflection.Get(mod, "Info");
            if (info != null)
            {
                var title = BossModReflection.Get(info, "Title") as string;
                if (!string.IsNullOrWhiteSpace(title))
                    return title;
            }
            string modName = mod.GetType().Name;
            if (!string.IsNullOrWhiteSpace(modName) && modName != "BossModule")
                return modName;
        }
        catch { }
        return null;
    }

    public void Tick()
    {
        if (_isDisposed) return;
        if (!_plugin.Configuration.BossModMirrorEnabled)
        {
            ClearAll();
            return;
        }

        try
        {
            DoTick();
        }
        catch (Exception ex)
        {
            if ((_throttle++ & 0x7F) == 0)
                Plugin.Log?.Debug($"[BossModBridge] {ex.Message}");
        }
    }

    public void DrawOverlay()
    {
        if (_isDisposed) return;
        _overlay.Draw(_plugin.Configuration);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        ClearAll();
    }

    private void DoTick()
    {
        var module = _locator.GetActiveModule();
        if (module == null)
        {
            ClearAll();
            return;
        }

        var lp = Plugin.ObjectTable.LocalPlayer;
        if (lp == null)
        {
            ClearAll();
            return;
        }

        float groundY = lp.Position.Y + _plugin.Configuration.BossModHeightOffset;

        _vfxEmitter.BeginFrame();
        _shapeMapper.BeginFrame();
        _overlay.ClearFrame();

        // Retrieve player actor and slot in BossMod
        object? pcActor = null;
        int pcSlot = 0;
        var ws = BossModReflection.Get(module, "WorldState");
        if (ws != null)
        {
            var party = BossModReflection.Get(ws, "Party");
            if (party != null)
            {
                var playerMethod = party.GetType().GetMethod("Player", BindingFlags.Public | BindingFlags.Instance);
                pcActor = playerMethod?.Invoke(party, null);
                if (pcActor == null)
                {
                    var indexer = party.GetType().GetProperty("Item", [typeof(int)]);
                    pcActor = indexer?.GetValue(party, [pcSlot]);
                }
            }
        }

        var ctx = new BossModContext(_plugin, lp, groundY, pcSlot, pcActor, module);
        var config = _plugin.Configuration;

        // Process Components
        if (BossModReflection.Get(module, "Components") is IEnumerable comps)
        {
            foreach (var comp in comps)
            {
                if (comp == null) continue;
                try
                {
                    _aoeProcessor.Process(comp, ctx);
                    _mechanicsProcessor.Process(comp, ctx);
                    _tetherGazeProcessor.Process(comp, ctx);
                    _duckTypeCrawler.DuckTypeWalk(comp, groundY, config);
                }
                catch
                {
                    /* isolate per-component failures */
                }
            }
        }

        // Process AI Hints (Safe Zones from Inverted Forbidden Zones like SDInvertedCircle, SDInvertedCone, etc.)
        try { _aiHintsProcessor.ProcessAIHints(ctx); }
        catch { }

        // Process Movement Hints (Safe Spot Arrows)
        try { _aiHintsProcessor.ProcessMovementHints(ctx); }
        catch { }

        // Process Tactical Hints (Blue Info / Red Danger In-Game Banners)
        try { _hintsProcessor.Process(ctx); }
        catch { }

        // Snapshot 2D Map AOEs for LiveMap & Replays
        lock (_activeMapAoes)
        {
            _activeMapAoes.Clear();
            lock (_shapeMapper.MapAoes)
            {
                _activeMapAoes.AddRange(_shapeMapper.MapAoes);
            }
            _overlay.CopyToMapAoes(_activeMapAoes);
        }

        // Expire stale VFX
        _vfxEmitter.EndFrame();
    }

    private void ClearAll()
    {
        _vfxEmitter.ClearAll();
        _overlay.ClearAll();
        _hintsProcessor.Clear();
        lock (_activeMapAoes)
        {
            _activeMapAoes.Clear();
        }
    }
}
