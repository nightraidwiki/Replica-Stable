using System;
using System.Collections;
using System.Collections.Generic;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Bridge.BossMod.Processors;
using Replica.Engine.Bridge.BossMod.Vfx;
using static Replica.Engine.Bridge.BossMod.Core.BossModConstants;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Reflection;

public sealed class BossModDuckTypeCrawler
{
    private readonly BossModAoeProcessor _aoeProcessor;
    private readonly BossModMechanicsProcessor _mechanicsProcessor;
    private readonly BossModShapeMapper _shapeMapper;

    private static readonly HashSet<string> _knownCollectionSet = new(StringComparer.Ordinal);
    private static readonly string[][] _knownAoeLists = [
        ["Casters"],
        ["AOEs"],
        ["_aoes"],
        ["_casters"],
        ["Lines"],
        ["_lines"],
        ["Sequences"],
        ["Blockers"],
        ["ActiveAOEs"],
        ["ActiveCasters"],
        ["PredictedPositions"],
        ["_charges"],
        ["_rings"],
        ["_aoe"],
    ];
    private static readonly string[] _knownChaserLists = ["Chasers", "_chasers", "Chases"];
    private static readonly string[] _knownBaitLists   = ["CurrentBaits", "ActiveBaits", "Baits", "_baits"];
    private static readonly string[] _knownSpreadLists = ["ActiveSpreads", "Spreads", "_spreads"];
    private static readonly string[] _knownStackLists  = ["ActiveStacks", "Stacks", "_stacks"];
    private static readonly string[] _knownTowerLists  = ["Towers", "ActiveTowers", "_towers"];
    private static readonly string[] _knownSafeSpotLists = [
        "Safezones", "ActiveSafezones", "_safezones", "SafeZones",
        "safespots", "SafeSpots", "_safeSpots", "_safespots",
        "SafePositions", "_safePositions", "SafePoints", "_safePoints",
        "DodgeSpots", "_dodgeSpots", "DodgePoints", "_dodgePoints"
    ];

    static BossModDuckTypeCrawler()
    {
        foreach (var g in _knownAoeLists) foreach (var n in g) _knownCollectionSet.Add(n);
        foreach (var n in _knownChaserLists) _knownCollectionSet.Add(n);
        foreach (var n in _knownBaitLists)   _knownCollectionSet.Add(n);
        foreach (var n in _knownSpreadLists) _knownCollectionSet.Add(n);
        foreach (var n in _knownStackLists)  _knownCollectionSet.Add(n);
        foreach (var n in _knownTowerLists)  _knownCollectionSet.Add(n);
        foreach (var n in _knownSafeSpotLists) _knownCollectionSet.Add(n);
    }

    public BossModDuckTypeCrawler(
        BossModAoeProcessor aoeProcessor,
        BossModMechanicsProcessor mechanicsProcessor,
        BossModShapeMapper shapeMapper)
    {
        _aoeProcessor = aoeProcessor;
        _mechanicsProcessor = mechanicsProcessor;
        _shapeMapper = shapeMapper;
    }

    public void DuckTypeWalk(object comp, float groundY, Configuration config)
    {
        var t = comp.GetType();
        // Standard GenericAOEs components already have authoritative ActiveAOEs extraction
        if (BossModFastExtractors.GetActiveAoesExtractor(t) != null) return;

        foreach (var field in GetAllFieldsCached(t))
        {
            if (_knownCollectionSet.Contains(field.Name)) continue;
            object? val;
            try { val = field.GetValue(comp); } catch { continue; }
            if (val == null) continue;

            // Single AOEShape on component
            if (val.GetType().Name.StartsWith("AOEShape", StringComparison.OrdinalIgnoreCase))
            {
                var originObj = GetField(comp, "Origin") ?? GetField(comp, "Position") ?? GetField(comp, "Caster");
                if (originObj != null)
                {
                    float ox = FX(originObj), oz = FZ(originObj);
                    float rot = RotRad(GetField(comp, "Rotation"));
                    bool isSafe = false;
                    var colObj = GetField(comp, "Color");
                    if (colObj is uint c && (c == 0x80008000 || c == 0xFF00FF00 || ((c & 0xFF) == 0 && (c >> 16 & 0xFF) == 0 && ((c >> 8) & 0xFF) >= 0x60)))
                    {
                        isSafe = true;
                    }
                    _shapeMapper.EmitShape(val, ox, oz, rot, groundY, isSafe ? ColSafe : ColDanger, 0, 0, null, null);
                }
                continue;
            }

            if (val is not IList list || list.Count == 0) continue;
            var item0 = list[0];
            if (item0 == null) continue;
            var it = item0.GetType();

            // AOEInstance-like: Shape + Origin/Position
            if (HasField(it, "Shape") && (HasField(it, "Origin") || HasField(it, "Position")))
            {
                foreach (var item in list)
                {
                    try { _aoeProcessor.EmitAoeItem(item, groundY); } catch { }
                }
            }
            // Nested container: e.g. objects with nested AOEs, _aoes, Casters, Lines, etc.
            else if (HasField(it, "AOEs") || HasField(it, "_aoes") || HasField(it, "Casters") || HasField(it, "Lines"))
            {
                foreach (var item in list)
                {
                    if (item == null) continue;
                    var subList = (GetField(item, "AOEs") ?? GetField(item, "_aoes") ?? GetField(item, "Casters") ?? GetField(item, "Lines")) as IEnumerable;
                    if (subList != null)
                    {
                        foreach (var subItem in subList)
                        {
                            try { _aoeProcessor.EmitAoeItem(subItem, groundY); } catch { }
                        }
                    }
                }
            }
            // BaitTarget-like: Source + Target + Shape
            else if (HasField(it, "Source") && HasField(it, "Target") && HasField(it, "Shape"))
            {
                foreach (var item in list)
                {
                    try { _mechanicsProcessor.EmitBaitItem(item, comp, groundY, ColBait); } catch { }
                }
            }
            // Spread/Stack-like: Target + Radius
            else if (HasField(it, "Target") && HasField(it, "Radius"))
            {
                foreach (var item in list)
                {
                    try { _mechanicsProcessor.EmitSpreadItem(item, groundY); } catch { }
                }
            }
            // Tower-like: Position + MinSoakers/MaxSoakers
            else if (HasField(it, "Position") && (HasField(it, "MinSoakers") || HasField(it, "Radius")))
            {
                foreach (var item in list)
                {
                    try { _mechanicsProcessor.EmitTowerItem(item, groundY, -1, config); } catch { }
                }
            }
        }
    }
}
