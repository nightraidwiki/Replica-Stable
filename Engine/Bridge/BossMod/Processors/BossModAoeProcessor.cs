using System;
using System.Collections;
using System.Numerics;
using System.Reflection;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Bridge.BossMod.Reflection;
using Replica.Engine.Bridge.BossMod.Vfx;
using Replica.Engine.Element;
using Replica.Logging;
using static Replica.Engine.Bridge.BossMod.Core.BossModConstants;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Processors;

public sealed class BossModAoeProcessor
{
    private readonly BossModShapeMapper _shapeMapper;
    private readonly BossModVfxEmitter _vfxEmitter;

    public BossModAoeProcessor(BossModShapeMapper shapeMapper, BossModVfxEmitter vfxEmitter)
    {
        _shapeMapper = shapeMapper;
        _vfxEmitter = vfxEmitter;
    }

    public static uint ParseActionId(object? obj)
    {
        if (obj == null) return 0;
        if (obj is uint u) return u;
        if (obj is int i && i > 0) return (uint)i;
        if (obj is System.Enum e)
        {
            try { return Convert.ToUInt32(e); } catch { }
        }
        var id = GetField(obj, "ID") ?? GetField(obj, "Raw");
        if (id is uint u2) return u2;
        if (id is int i2 && i2 > 0) return (uint)i2;
        if (id is System.Enum e2)
        {
            try { return Convert.ToUInt32(e2); } catch { }
        }
        return 0;
    }

    public static uint ExtractActionId(object? item, object? comp)
    {
        if (item != null)
        {
            var a = GetField(item, "Action") ?? GetField(item, "ActionID") ?? GetField(item, "AID") ?? GetField(item, "CastInfo");
            if (a != null && a.GetType().Name.Contains("CastInfo"))
            {
                var ca = GetField(a, "Action");
                uint cid = ParseActionId(ca);
                if (cid != 0) return cid;
            }
            uint aid = ParseActionId(a);
            if (aid != 0) return aid;
        }
        if (comp != null)
        {
            var ca = GetField(comp, "WatchedAction") ?? GetField(comp, "Action") ?? GetField(comp, "AID") ?? GetField(comp, "CastAction");
            uint aid = ParseActionId(ca);
            if (aid != 0) return aid;
        }
        return 0;
    }

    public void Process(object comp, BossModContext ctx)
    {
        var config = ctx.Plugin.Configuration;
        if (!config.BossModMirrorAOEs) return;

        bool handledByActiveAoes = false;

        // 1. Universal BossMod GenericAOEs extraction (ActiveAOEs method returning ReadOnlySpan<AOEInstance>)
        try
        {
            var extractor = BossModFastExtractors.GetActiveAoesExtractor(comp.GetType());
            if (extractor != null)
            {
                extractor(comp, ctx.PcSlot, ctx.PcActor, aoe => EmitAoeItem(aoe, comp, ctx.GroundY));
                handledByActiveAoes = true;
            }
        }
        catch { }

        // 2. Standard AOE lists (AOEInstance: Shape + Origin) - fallback & supplementary only when not handled by ActiveAOEs
        if (!handledByActiveAoes)
        {
            var aoeList = GetField(comp, "ActiveCasters") as IEnumerable
                       ?? GetField(comp, "Casters") as IEnumerable
                       ?? GetField(comp, "AOEs") as IEnumerable
                       ?? GetField(comp, "_aoes") as IEnumerable
                       ?? GetField(comp, "_casters") as IEnumerable
                       ?? GetField(comp, "Lines") as IEnumerable
                       ?? GetField(comp, "_lines") as IEnumerable
                       ?? GetField(comp, "Sequences") as IEnumerable
                       ?? GetField(comp, "Blockers") as IEnumerable
                       ?? GetField(comp, "ActiveAOEs") as IEnumerable
                       ?? GetField(comp, "_charges") as IEnumerable
                       ?? GetField(comp, "_rings") as IEnumerable
                       ?? GetField(comp, "_aoe") as IEnumerable;

            if (aoeList != null)
            {
                foreach (var item in aoeList)
                {
                    try { EmitAoeItem(item, comp, ctx.GroundY); } catch { }
                }
            }

            // Single AOE fields
            var curAoe = GetField(comp, "CurAOE") ?? GetField(comp, "ActiveAOE") ?? GetField(comp, "_curAOE") ?? GetField(comp, "_activeAOE");
            if (curAoe != null)
            {
                try { EmitAoeItem(curAoe, comp, ctx.GroundY); } catch { }
            }
        }

        // Voidzones
        try { ProcessVoidzone(comp, ctx.Module, ctx.GroundY); } catch { }

        // Twisters
        try { ProcessTwister(comp, ctx.GroundY); } catch { }

        // Proteans (GenericProtean)
        try { ProcessProtean(comp, ctx.GroundY); } catch { }

        // Shared Tankbusters (GenericSharedTankbuster)
        try { ProcessSharedTankbuster(comp, ctx.GroundY); } catch { }
    }

    public void EmitAoeItem(object? item, object? comp, float groundY, bool forceSafe = false)
    {
        if (item == null) return;
        uint actionId = ExtractActionId(item, comp);

        if (item is SyntheticAOE saoe)
        {
            float sox = FX(saoe.Origin), soz = FZ(saoe.Origin);
            float srot = saoe.Rotation != null ? RotRad(saoe.Rotation) : 0f;
            Vector4 scolor = saoe.IsSafe || forceSafe ? ColSafe : ColDanger;
            ulong skey = (ulong)saoe.Origin.GetHashCode() ^ (ulong)saoe.Shape.GetHashCode();
            _shapeMapper.EmitShape(saoe.Shape, sox, soz, srot, groundY, scolor, skey, 0, null, null, actionId);
            return;
        }

        var shape = GetField(item, "Shape");
        var origin = GetField(item, "Origin") ?? GetField(item, "Position");
        if (shape == null || origin == null) return;

        bool isSafe = forceSafe;
        var colorObj = GetField(item, "Color");
        if (!isSafe && colorObj is uint col && col != 0)
        {
            uint r = col & 0xFF;
            uint g = (col >> 8) & 0xFF;
            uint bVal = (col >> 16) & 0xFF;
            if (col == 0x80008000 || col == 0xFF00FF00 || (r == 0 && bVal == 0 && g >= 0x60))
            {
                isSafe = true;
            }
        }

        // InvertForbiddenZone also indicates safe spot
        if (!isSafe)
        {
            var ifz = GetField(shape, "InvertForbiddenZone");
            if (ifz is bool ifzVal && ifzVal) isSafe = true;
        }

        var risky = GetField(item, "Risky");
        if (!isSafe && risky is bool b && !b) return;

        Vector4 color = isSafe ? ColSafe : ColDanger;
        float ox = FX(origin), oz = FZ(origin);
        float rot = RotRad(GetField(item, "Rotation"));

        ulong actorId = UL(GetField(item, "ActorID") ?? GetField(GetField(item, "Caster"), "InstanceID") ?? GetField(GetField(item, "Source"), "InstanceID"));
        
        if (actionId == 0 && actorId != 0)
        {
            uint eid = EntityId(actorId);
            if (eid != 0 && Plugin.ObjectTable.SearchById(eid) is IBattleChara bc && bc.IsCasting)
            {
                actionId = bc.CastActionId;
            }
        }

        _shapeMapper.EmitShape(shape, ox, oz, rot, groundY, color, actorId, 0, null, null, actionId);
    }

    public void EmitAoeItem(object? item, float groundY, bool forceSafe = false) => EmitAoeItem(item, null, groundY, forceSafe);

    private void ProcessVoidzone(object comp, object module, float groundY)
    {
        var shape = GetField(comp, "Shape");
        if (shape == null) return;
        uint actionId = ExtractActionId(null, comp);

        var sourcesObj = GetField(comp, "Sources");
        if (sourcesObj is Delegate del)
        {
            try
            {
                var result = del.DynamicInvoke(module) as IEnumerable;
                if (result != null)
                {
                    foreach (var src in result)
                    {
                        if (src == null) continue;
                        var p = GetField(src, "Position");
                        if (p == null) continue;
                        float ox = FX(p), oz = FZ(p);
                        float rot = RotRad(GetField(src, "Rotation"));
                        ulong id = UL(GetField(src, "InstanceID"));
                        uint eid = EntityId(id);
                        var initPos = new Vector3(ox, groundY, oz);
                        Func<Vector3>? posAction = eid != 0
                            ? () =>
                            {
                                var g = Plugin.ObjectTable.SearchById(eid);
                                return g != null ? g.Position with { Y = groundY } : initPos;
                            }
                            : null;
                        _shapeMapper.EmitShape(shape, ox, oz, rot, groundY, ColDanger, id, 0, posAction, null, actionId);
                    }
                }
            }
            catch { }
        }
    }

    private void ProcessTwister(object comp, float groundY)
    {
        float radius = Ff(GetField(GetField(comp, "_shape"), "Radius"), 2f);
        uint actionId = ExtractActionId(null, comp);

        if (GetField(comp, "PredictedPositions") is IEnumerable preds)
        {
            foreach (var p in preds)
            {
                if (p == null) continue;
                float ox = FX(p), oz = FZ(p);
                var pos = new Vector3(ox, groundY, oz);
                lock (_shapeMapper.MapAoes) { _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Circle, false, ox, oz, 0f, radius, 0f, 0f, 0, 0, 0, actionId, 0)); }
                _vfxEmitter.Emit(K_CIRCLE, 0, ox, oz, 0, radius, 0, 0,
                    new DrawElement
                    {
                        drawAvfx       = "customCircle",
                        Position       = pos,
                        drawOnObject   = false,
                        radiusX        = radius,
                        radiusZ        = radius,
                        refColor       = ColDanger,
                        refTargetColor = ColDanger,
                        destroyTime    = 60000f,
                        fixRotation    = true,
                    }, false, null);
            }
        }

        var twisters = GetField(comp, "ActiveTwisters") as IEnumerable
                    ?? GetField(comp, "Twisters") as IEnumerable;
        if (twisters != null)
        {
            foreach (var tw in twisters)
            {
                if (tw == null) continue;
                var p = GetField(tw, "Position");
                if (p == null) continue;
                float ox = FX(p), oz = FZ(p);
                ulong id = UL(GetField(tw, "InstanceID"));
                var pos = new Vector3(ox, groundY, oz);
                lock (_shapeMapper.MapAoes) { _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Circle, false, ox, oz, 0f, radius, 0f, 0f, 0, 0, EntityId(id), actionId, 0)); }
                _vfxEmitter.Emit(K_CIRCLE, id, ox, oz, 0, radius, 0, 0,
                    new DrawElement
                    {
                        drawAvfx       = "customCircle",
                        Position       = pos,
                        drawOnObject   = false,
                        radiusX        = radius,
                        radiusZ        = radius,
                        refColor       = ColDanger,
                        refTargetColor = ColDanger,
                        destroyTime    = 60000f,
                        fixRotation    = true,
                    }, false, null);
            }
        }
    }

    private void ProcessProtean(object comp, float groundY)
    {
        var shape = GetField(comp, "Shape");
        if (shape == null) return;
        uint actionId = ExtractActionId(null, comp);

        var activeAoesMethod = comp.GetType().GetMethod("ActiveAOEs",
            BindingFlags.Public | BindingFlags.Instance,
            null, Type.EmptyTypes, null);

        if (activeAoesMethod != null)
        {
            try
            {
                if (activeAoesMethod.Invoke(comp, null) is IEnumerable pairs)
                {
                    foreach (var pair in pairs)
                    {
                        if (pair == null) continue;
                        var source = GetField(pair, "Item1") ?? GetField(pair, "source") ?? GetField(pair, "Source");
                        var target = GetField(pair, "Item2") ?? GetField(pair, "target") ?? GetField(pair, "Target");
                        if (source == null || target == null) continue;

                        var sPos = GetField(source, "Position");
                        var tPos = GetField(target, "Position");
                        if (sPos == null || tPos == null) continue;

                        float sx = FX(sPos), sz = FZ(sPos);
                        float tx = FX(tPos), tz = FZ(tPos);
                        float dx = tx - sx, dz = tz - sz;
                        float rot = MathF.Atan2(dx, dz);

                        ulong srcId = UL(GetField(source, "InstanceID"));
                        ulong tgtId = UL(GetField(target, "InstanceID"));

                        _shapeMapper.EmitShape(shape, sx, sz, rot, groundY, ColDanger, srcId, tgtId, null, null, actionId);
                    }
                }
            }
            catch { }
        }
    }

    private void ProcessSharedTankbuster(object comp, float groundY)
    {
        var shape = GetField(comp, "Shape");
        if (shape == null) return;
        uint actionId = ExtractActionId(null, comp);

        var source = GetField(comp, "Source");
        if (source == null) return;
        var target = GetField(comp, "Target");
        if (target == null) return;

        bool originAtTarget = GetField(comp, "OriginAtTarget") is bool oat && oat;
        var originActor = originAtTarget ? target : source;
        var posObj = GetField(originActor, "Position");
        if (posObj == null) return;

        float ox = FX(posObj), oz = FZ(posObj);

        var sPos = GetField(source, "Position");
        var tPos = GetField(target, "Position");
        float rot = 0f;
        if (sPos != null && tPos != null)
        {
            float dx = FX(tPos) - FX(sPos), dz = FZ(tPos) - FZ(sPos);
            rot = MathF.Atan2(dx, dz);
        }

        ulong srcId = UL(GetField(source, "InstanceID"));
        ulong tgtId = UL(GetField(target, "InstanceID"));

        _shapeMapper.EmitShape(shape, ox, oz, rot, groundY, ColStack, srcId, tgtId, null, null, actionId);
    }
}
