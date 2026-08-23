using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Bridge.BossMod.Overlay;
using Replica.Engine.Bridge.BossMod.Reflection;
using Replica.Engine.Bridge.BossMod.Vfx;
using Replica.Engine.Element;
using Replica.Engine.Util;
using Replica.Logging;
using static Replica.Engine.Bridge.BossMod.Core.BossModConstants;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Processors;

public sealed class BossModMechanicsProcessor
{
    private readonly BossModShapeMapper _shapeMapper;
    private readonly BossModVfxEmitter _vfxEmitter;
    private readonly BossModOverlayRenderer _overlay;

    public BossModMechanicsProcessor(
        BossModShapeMapper shapeMapper,
        BossModVfxEmitter vfxEmitter,
        BossModOverlayRenderer overlay)
    {
        _shapeMapper = shapeMapper;
        _vfxEmitter = vfxEmitter;
        _overlay = overlay;
    }

    public void Process(object comp, BossModContext ctx)
    {
        var config = ctx.Plugin.Configuration;

        // 1. Chaser lists (Chaser: Shape + Target + PrevPos)
        if (config.BossModMirrorAOEs)
        {
            var chasers = GetField(comp, "Chasers") as IEnumerable
                       ?? GetField(comp, "_chasers") as IEnumerable
                       ?? GetField(comp, "Chases") as IEnumerable;

            if (chasers != null)
            {
                foreach (var ch in chasers)
                {
                    try { EmitChaser(ch, comp, ctx.GroundY); } catch { }
                }
            }
        }

        // 2. Bait lists (Bait: Source + Target + Shape)
        if (config.BossModMirrorAOEs)
        {
            var baits = GetField(comp, "ActiveBaits") as IEnumerable
                     ?? GetField(comp, "CurrentBaits") as IEnumerable
                     ?? GetField(comp, "Baits") as IEnumerable
                     ?? GetField(comp, "_baits") as IEnumerable;

            if (baits != null)
            {
                foreach (var b in baits)
                {
                    try { EmitBaitItem(b, comp, ctx.GroundY, ColBait); } catch { }
                }
            }
        }

        // 3. Spread & Stack lists
        if (config.BossModMirrorSpreadsStacks)
        {
            var spreads = GetField(comp, "ActiveSpreads") as IEnumerable
                       ?? GetField(comp, "Spreads") as IEnumerable
                       ?? GetField(comp, "_spreads") as IEnumerable;

            if (spreads != null)
            {
                foreach (var s in spreads)
                {
                    try { EmitSpreadItem(s, ctx.GroundY, comp); } catch { }
                }
            }

            var stacks = GetField(comp, "ActiveStacks") as IEnumerable
                      ?? GetField(comp, "Stacks") as IEnumerable
                      ?? GetField(comp, "_stacks") as IEnumerable;

            if (stacks != null)
            {
                var stackList = new List<object>();
                foreach (var s in stacks) if (s != null) stackList.Add(s);
                int totalCount = stackList.Count;

                foreach (var s in stackList)
                {
                    try { EmitStackItem(s, ctx.GroundY, comp, totalCount); } catch { }
                }
            }
        }

        // 4. Line Stacks
        if (config.BossModMirrorLineStacks)
        {
            try { ProcessLineStacks(comp, ctx.GroundY, ctx.PcSlot, config); } catch { }
        }

        // 5. Exaflares & Moving AOEs
        if (config.BossModMirrorExaflares)
        {
            try { ProcessExaflares(comp, ctx.GroundY, config); } catch { }
        }

        // 6. Towers & Safezones
        if (config.BossModMirrorSafeZones)
        {
            bool towerExtracted = false;
            try
            {
                var towerExtractor = BossModFastExtractors.GetActiveTowersExtractor(comp.GetType());
                if (towerExtractor != null)
                {
                    towerExtractor(comp, ctx.PcSlot, ctx.PcActor, tow => EmitTowerItem(tow, ctx.GroundY, ctx.PcSlot, config, comp));
                    towerExtracted = true;
                }
            }
            catch { }

            if (!towerExtracted)
            {
                var towers = GetField(comp, "ActiveTowers") as IEnumerable
                          ?? GetField(comp, "Towers") as IEnumerable
                          ?? GetField(comp, "_towers") as IEnumerable;

                if (towers != null)
                {
                    foreach (var tow in towers)
                    {
                        try { EmitTowerItem(tow, ctx.GroundY, ctx.PcSlot, config, comp); } catch { }
                    }
                }
            }
        }

        // 7. Wild Charges (GenericWildCharge / InverseWildCharge)
        if (config.BossModMirrorSpreadsStacks)
        {
            try { EmitWildCharge(comp, ctx.GroundY, ctx.PcSlot); } catch { }
        }
    }

    public void EmitChaser(object? ch, object? comp, float groundY)
    {
        if (ch == null) return;
        var shape = GetField(ch, "Shape");
        var target = GetField(ch, "Target");
        if (shape == null || target == null) return;

        uint actionId = BossModAoeProcessor.ExtractActionId(ch, comp);
        var prevPos = GetField(ch, "PrevPos");
        float ox = prevPos != null ? FX(prevPos) : FX(GetField(target, "Position") ?? new Vector3());
        float oz = prevPos != null ? FZ(prevPos) : FZ(GetField(target, "Position") ?? new Vector3());

        ulong tgtId = UL(GetField(target, "InstanceID"));
        uint te = EntityId(tgtId);
        var initPos = new Vector3(ox, groundY, oz);

        Func<Vector3>? posAction = te != 0
            ? () =>
            {
                var g = Plugin.ObjectTable.SearchById(te);
                return g != null ? g.Position with { Y = groundY } : initPos;
            }
            : null;

        _shapeMapper.EmitShape(shape, ox, oz, 0f, groundY, ColDanger, tgtId, 0, posAction, null, actionId);
    }

    public void EmitBaitItem(object? bait, object? comp, float groundY, Vector4 color)
    {
        if (bait == null) return;
        var source = GetField(bait, "Source");
        var target = GetField(bait, "Target");
        var shape = GetField(bait, "Shape");
        if (source == null || shape == null) return;

        uint actionId = BossModAoeProcessor.ExtractActionId(bait, comp);
        ulong srcId = UL(GetField(source, "InstanceID"));
        ulong tgtId = target != null ? UL(GetField(target, "InstanceID")) : 0;
        uint se = EntityId(srcId), te = EntityId(tgtId);

        bool centerAtTarget = comp != null && (GetField(comp, "CenterAtTarget") is bool cat && cat);

        var offsetObj = GetField(bait, "Offset");
        float offX = offsetObj != null ? FX(offsetObj) : 0f;
        float offZ = offsetObj != null ? FZ(offsetObj) : 0f;

        var srcPosObj = GetField(source, "Position");
        float sx = (srcPosObj != null ? FX(srcPosObj) : 0) + (centerAtTarget ? 0 : offX);
        float sz = (srcPosObj != null ? FZ(srcPosObj) : 0) + (centerAtTarget ? 0 : offZ);

        var tgtPosObj = target != null ? GetField(target, "Position") : null;
        float tx = (tgtPosObj != null ? FX(tgtPosObj) : sx) + (centerAtTarget ? offX : 0);
        float tz = (tgtPosObj != null ? FZ(tgtPosObj) : sz) + (centerAtTarget ? offZ : 0);

        float ox = centerAtTarget ? tx : sx;
        float oz = centerAtTarget ? tz : sz;

        var initSrc = new Vector3(sx, groundY, sz);
        var initTgt = new Vector3(tx, groundY, tz);
        var initOrigin = new Vector3(ox, groundY, oz);

        Func<Vector3>? posAction = null;
        if (centerAtTarget && te != 0)
        {
            posAction = () =>
            {
                var tg = Plugin.ObjectTable.SearchById(te);
                return (tg != null ? tg.Position with { Y = groundY } : initOrigin) + new Vector3(offX, 0f, offZ);
            };
        }
        else if (!centerAtTarget && se != 0)
        {
            posAction = () =>
            {
                var sg = Plugin.ObjectTable.SearchById(se);
                return (sg != null ? sg.Position with { Y = groundY } : initOrigin) + new Vector3(offX, 0f, offZ);
            };
        }

        Func<Vector3>? targetPosAction = te != 0
            ? () =>
            {
                var tg = Plugin.ObjectTable.SearchById(te);
                return tg != null ? tg.Position with { Y = groundY } : initTgt;
            }
            : null;

        float doff = Ff(GetField(GetField(shape, "DirectionOffset"), "Rad"), 0f);

        Func<Angle>? rotAction = null;
        float initRot = 0f;
        var customRot = GetField(bait, "CustomRotation");
        if (customRot != null)
        {
            initRot = RotRad(customRot) + doff;
        }
        else if (target != null)
        {
            initRot = MathF.Atan2(tx - sx, tz - sz) + doff;
            if (se != 0 && te != 0)
            {
                rotAction = () =>
                {
                    var sg = Plugin.ObjectTable.SearchById(se);
                    var tg = Plugin.ObjectTable.SearchById(te);
                    if (sg == null || tg == null) return new Angle(initRot);
                    var d = tg.Position - sg.Position;
                    return new Angle(MathF.Atan2(d.X, d.Z) + doff);
                };
            }
        }

        string tn = shape.GetType().Name;
        if (tn.Contains("Rect") || (!tn.Contains("Circle") && !tn.Contains("Donut") && !tn.Contains("Cone") && !tn.Contains("Cross") && !tn.Contains("Capsule")))
        {
            float lf = Ff(GetField(shape, "LengthFront") ?? GetField(shape, "Length"), 0f);
            float lb = Ff(GetField(shape, "LengthBack"), 0f);
            float hw = Ff(GetField(shape, "HalfWidth"), 3f);

            float dist = Vector3.Distance(initSrc, initTgt);
            float totalLen = lf > 0 ? (lf + lb) : (dist > 0.5f ? dist : 25f);
            bool endToTarget = lf == 0 && targetPosAction != null;

            lock (_shapeMapper.MapAoes)
            {
                _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Rect, false, ox, oz, initRot, totalLen, lb, hw, 0, 0, se, actionId, te));
            }

            _vfxEmitter.Emit(K_RECT, srcId ^ tgtId, 0, 0, 0, hw, 0, 0,
                new DrawElement
                {
                    drawAvfx                   = "customRect",
                    Position                   = initOrigin,
                    drawOnObject               = false,
                    radiusX                    = hw,
                    radiusZ                    = totalLen,
                    refOffsetZ                 = lb,
                    refRotation                = new Angle(initRot),
                    fixRotation                = true,
                    refColor                   = color,
                    refTargetColor             = color,
                    destroyTime                = 60000f,
                    endToTarget                = endToTarget,
                    targetPosition             = initTgt,
                    PositionCustomAction       = posAction,
                    TargetPositionCustomAction = targetPosAction,
                    RotationCustomAction       = rotAction
                }, true, rotAction);
        }
        else
        {
            _shapeMapper.EmitShape(shape, ox, oz, initRot - doff, groundY, color, srcId, tgtId, posAction, rotAction, actionId);
        }
    }

    public void EmitSpreadItem(object? item, float groundY, object? comp = null)
    {
        if (item == null) return;
        var target = GetField(item, "Target");
        if (target == null) return;

        uint actionId = BossModAoeProcessor.ExtractActionId(item, comp);
        float radius = Ff(GetField(item, "Radius"), 5f);
        ulong tgtId = UL(GetField(target, "InstanceID"));
        uint te = EntityId(tgtId);

        var tgtPos = GetField(target, "Position");
        float ox = tgtPos != null ? FX(tgtPos) : 0;
        float oz = tgtPos != null ? FZ(tgtPos) : 0;
        var initPos = new Vector3(ox, groundY, oz);

        lock (_shapeMapper.MapAoes)
        {
            _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Circle, false, ox, oz, 0f, radius, 0f, 0f, 0, 0, te, actionId, te));
        }

        Func<Vector3>? posAction = te != 0
            ? () =>
            {
                var g = Plugin.ObjectTable.SearchById(te);
                return g != null ? g.Position with { Y = groundY } : initPos;
            }
            : null;

        var color = new Vector4(1f, 0.30f, 0.30f, 1.5f);
        _vfxEmitter.Emit(K_CIRCLE, tgtId, 0, 0, 0, radius, 0, 0,
            new DrawElement
            {
                drawAvfx             = "customCircle",
                Position             = initPos,
                drawOnObject         = false,
                radiusX              = radius,
                radiusZ              = radius,
                refColor             = color,
                refTargetColor       = color,
                destroyTime          = 60000f,
                fixRotation          = true,
                PositionCustomAction = posAction,
            }, posAction != null, null);
    }

    public void EmitStackItem(object? item, float groundY, object? comp = null, int totalCount = 1)
    {
        if (item == null) return;
        var target = GetField(item, "Target");
        if (target == null) return;

        uint actionId = BossModAoeProcessor.ExtractActionId(item, comp);
        float radius = Ff(GetField(item, "Radius"), 6f);
        ulong tgtId = UL(GetField(target, "InstanceID"));
        uint te = EntityId(tgtId);

        var tgtPos = GetField(target, "Position");
        float ox = tgtPos != null ? FX(tgtPos) : 0;
        float oz = tgtPos != null ? FZ(tgtPos) : 0;
        var initPos = new Vector3(ox, groundY, oz);

        var stackKind = BossModStackClassifier.Classify(item, comp, totalCount);

        uint mapColor = stackKind == BossModStackKind.Pair ? 0xFFFFFFFF : 0xFF00FFFF;
        lock (_shapeMapper.MapAoes)
        {
            _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Circle, false, ox, oz, 0f, radius, 0f, 0f, mapColor, 0, te, actionId, te));
        }

        Func<Vector3>? posAction = te != 0
            ? () =>
            {
                var g = Plugin.ObjectTable.SearchById(te);
                return g != null ? g.Position with { Y = groundY } : initPos;
            }
            : null;

        if (stackKind == BossModStackKind.Pair)
        {
            // 2-man Pair: White 2-person ground pair VFX (share2_6m)
            _vfxEmitter.Emit(K_PAIR, tgtId, 0, 0, 0, radius, 0, 0,
                new DrawElement
                {
                    drawAvfx             = "share2_6m",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = 1f,
                    radiusZ              = 2f,
                    refColor             = ColStack,
                    refTargetColor       = ColStack,
                    destroyTime          = 60000f,
                    LoopInterval         = 4900f,
                    fixRotation          = true,
                    PositionCustomAction = posAction,
                }, posAction != null, null);
        }
        else
        {
            // 4-man Light Party Stack or 8-man Raid Stack:
            // 1. 6m ground circle in gold/yellow ColPartyStack
            _vfxEmitter.Emit(K_PARTY4, tgtId, 0, 0, 0, radius, 0, 0,
                new DrawElement
                {
                    drawAvfx             = "customCircle",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = radius,
                    radiusZ              = radius,
                    refColor             = ColPartyStack,
                    refTargetColor       = ColPartyStack,
                    destroyTime          = 60000f,
                    fixRotation          = true,
                    PositionCustomAction = posAction,
                }, posAction != null, null);

            // 2. Official FFXIV Stack Marker (overhead 4 converging arrows)
            if (te != 0)
            {
                var targetObj = Plugin.ObjectTable.SearchById(te);
                if (targetObj != null)
                {
                    _vfxEmitter.EmitLockOn(K_PARTY4, tgtId, targetObj);
                }
            }
        }
    }

    public void EmitTowerItem(object? tow, float groundY, int pcSlot, Configuration config, object? comp = null)
    {
        if (tow == null) return;
        var shape = GetField(tow, "Shape");
        var pos = GetField(tow, "Position");
        if (pos == null) return;

        uint actionId = BossModAoeProcessor.ExtractActionId(tow, comp);
        float ox = FX(pos), oz = FZ(pos);
        float rot = RotRad(GetField(tow, "Rotation"));
        ulong actorId = UL(GetField(tow, "ActorID"));
        uint entityId = EntityId(actorId);

        Vector4 towerColor = ColTower;
        uint ringColor = 0xFF00FFFF;
        bool isMyTower = false;

        if (config.BossModMirrorSmartTowers && pcSlot >= 0)
        {
            var forbiddenObj = GetField(tow, "ForbiddenSoakers") ?? GetField(tow, "ForbiddenPlayers") ?? GetField(tow, "_forbidden");
            if (forbiddenObj != null)
            {
                bool isForbidden = false;
                ulong rawMask = 0;
                var rawField = GetField(forbiddenObj, "Raw");
                if (rawField is ulong r) rawMask = r;
                else if (forbiddenObj is ulong ul) rawMask = ul;
                else if (forbiddenObj is int iVal) rawMask = (ulong)iVal;
                else if (forbiddenObj is uint uiVal) rawMask = uiVal;

                var indexer = forbiddenObj.GetType().GetProperty("Item", [typeof(int)]);
                if (indexer != null)
                {
                    try { isForbidden = (bool)(indexer.GetValue(forbiddenObj, [pcSlot]) ?? false); } catch { }
                }
                else if (rawMask != 0)
                {
                    isForbidden = (rawMask & (1UL << pcSlot)) != 0;
                }

                if (isForbidden)
                {
                    towerColor = new Vector4(1f, 0.20f, 0.20f, 1.2f); // Danger Red
                    ringColor = 0xFF0000FF;
                }
                else
                {
                    isMyTower = true;
                    towerColor = ColSafe; // Bright Green
                    ringColor = 0xFF00FF00;
                }
            }

            var correctObj = GetField(tow, "CorrectSoakers");
            if (correctObj != null)
            {
                var indexer = correctObj.GetType().GetProperty("Item", [typeof(int)]);
                if (indexer != null)
                {
                    try
                    {
                        if ((bool)(indexer.GetValue(correctObj, [pcSlot]) ?? false))
                        {
                            isMyTower = true;
                            towerColor = ColSafe;
                            ringColor = 0xFF00FF00;
                        }
                    }
                    catch { }
                }
            }
        }

        Func<Vector3>? posAction = null;
        if (entityId != 0)
        {
            var initPos = new Vector3(ox, groundY, oz);
            posAction = () =>
            {
                var g = Plugin.ObjectTable.SearchById(entityId);
                return g != null ? g.Position with { Y = groundY } : initPos;
            };
        }

        if (shape != null)
        {
            _shapeMapper.EmitShape(shape, ox, oz, rot, groundY, towerColor, actorId, 0, posAction, null, actionId);
        }
        else
        {
            float radius = Ff(GetField(tow, "Radius"), 4f);
            var initPos = new Vector3(ox, groundY, oz);
            lock (_shapeMapper.MapAoes)
            {
                _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Circle, isMyTower, ox, oz, 0f, radius, 0f, 0f, 0, 0, entityId, actionId, 0));
            }
            _vfxEmitter.Emit(K_TOWER, actorId, ox, oz, 0, radius, 0, 0,
                new DrawElement
                {
                    drawAvfx             = "customCircle",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = radius,
                    radiusZ              = radius,
                    refColor             = towerColor,
                    refTargetColor       = towerColor,
                    destroyTime          = 60000f,
                    fixRotation          = true,
                    PositionCustomAction = posAction,
                }, posAction != null, null);
        }

        if (isMyTower)
        {
            float r = shape != null ? 3.5f : Ff(GetField(tow, "Radius"), 4f);
            _overlay.AddSafeSpot(new OverlaySafeSpot(new Vector3(ox, groundY, oz), r, ringColor));
        }
    }

    public void EmitWildCharge(object comp, float groundY, int pcSlot = -1)
    {
        var source = GetField(comp, "Source");
        if (source == null) return;
        var playerRoles = GetField(comp, "PlayerRoles") as System.Array;
        if (playerRoles == null) return;

        uint actionId = BossModAoeProcessor.ExtractActionId(null, comp);
        float hw = Ff(GetField(comp, "HalfWidth"), 3f);
        float fl = Ff(GetField(comp, "FixedLength"), 0f);
        float distBehind = 0f;
        foreach (var f in GetAllFieldsCached(comp.GetType()))
        {
            if (f.Name.Contains("distancebehind", StringComparison.OrdinalIgnoreCase))
            {
                distBehind = Ff(f.GetValue(comp));
                break;
            }
        }

        ulong srcId = UL(GetField(source, "InstanceID"));
        uint se = EntityId(srcId);
        var sp = GetField(source, "Position");
        if (sp == null) return;
        float sx = FX(sp), sz = FZ(sp);

        var module = GetField(comp, "Module");
        if (module == null) return;
        var raid = GetField(module, "Raid");
        if (raid == null) return;

        var withSlot = raid.GetType().GetMethod("WithSlot",
            BindingFlags.Public | BindingFlags.Instance,
            null, [typeof(bool), typeof(bool), typeof(bool)], null);
        if (withSlot == null) return;

        IEnumerable? slotPairs = null;
        try { slotPairs = withSlot.Invoke(raid, [true, false, false]) as IEnumerable; } catch { }
        if (slotPairs == null) return;

        foreach (var pair in slotPairs)
        {
            try
            {
                int slot = (int)(GetField(pair, "Item1") ?? -1);
                var actor = GetField(pair, "Item2");
                if (slot < 0 || slot >= playerRoles.Length || actor == null) continue;
                var roleVal = playerRoles.GetValue(slot);
                if (roleVal == null) continue;
                int roleInt = (int)roleVal;
                if (roleInt != 1 && roleInt != 2) continue; // Target / TargetNotFirst

                ulong tgtId = UL(GetField(actor, "InstanceID"));
                uint te = EntityId(tgtId);
                var tp = GetField(actor, "Position");
                if (tp == null) continue;

                float tx = FX(tp), tz = FZ(tp);
                float dx = tx - sx, dz = tz - sz;
                float dist = MathF.Sqrt(dx * dx + dz * dz);
                if (dist < 0.5f) continue;
                float rot = MathF.Atan2(dx, dz);

                var initSrc = new Vector3(sx, groundY, sz);

                float normX = dx / dist, normZ = dz / dist;
                float endTx = tx + normX * distBehind;
                float endTz = tz + normZ * distBehind;
                var initTgt = new Vector3(endTx, groundY, endTz);

                Func<Vector3>? posAction = se != 0
                    ? () =>
                    {
                        var g = Plugin.ObjectTable.SearchById(se);
                        return g != null ? g.Position with { Y = groundY } : initSrc;
                    }
                    : null;

                Func<Vector3>? targetPosAction = te != 0
                    ? () =>
                    {
                        var tg = Plugin.ObjectTable.SearchById(te);
                        var sg = se != 0 ? Plugin.ObjectTable.SearchById(se) : null;
                        if (tg == null) return initTgt;
                        var sPos = sg != null ? sg.Position : initSrc;
                        var tPos = tg.Position;
                        var diff = tPos - sPos;
                        float l = MathF.Sqrt(diff.X * diff.X + diff.Z * diff.Z);
                        if (l > 0.001f && distBehind > 0f)
                        {
                            var dir = diff / l;
                            return (tPos + dir * distBehind) with { Y = groundY };
                        }
                        return tPos with { Y = groundY };
                    }
                    : null;

                Func<Angle>? rotAction = (se != 0 && te != 0)
                    ? () =>
                    {
                        var sg = Plugin.ObjectTable.SearchById(se);
                        var tg = Plugin.ObjectTable.SearchById(te);
                        if (sg == null || tg == null) return new Angle(rot);
                        var d = tg.Position - sg.Position;
                        return new Angle(MathF.Atan2(d.X, d.Z));
                    }
                    : null;

                float totalLen = fl > 0 ? fl : (dist + distBehind);
                bool endToTarget = fl == 0 && targetPosAction != null;

                bool dangerous = false;
                if (pcSlot >= 0 && pcSlot < playerRoles.Length)
                {
                    var myRoleVal = playerRoles.GetValue(pcSlot);
                    if (myRoleVal != null && (int)myRoleVal == 5) // PlayerRole.Avoid
                        dangerous = true;
                }
                Vector4 color = dangerous ? ColDanger : ColStack;

                lock (_shapeMapper.MapAoes)
                {
                    _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Rect, !dangerous, sx, sz, rot, totalLen, 0f, hw, 0, 0, se, actionId, te));
                }

                _vfxEmitter.Emit(K_RECT, srcId ^ tgtId, 0, 0, 0, hw, 0, 0,
                    new DrawElement
                    {
                        drawAvfx                   = "customRect",
                        Position                   = initSrc,
                        drawOnObject               = false,
                        radiusX                    = hw,
                        radiusZ                    = totalLen,
                        refRotation                = new Angle(rot),
                        fixRotation                = true,
                        refColor                   = color,
                        refTargetColor             = color,
                        destroyTime                = 60000f,
                        endToTarget                = endToTarget,
                        targetPosition             = initTgt,
                        PositionCustomAction       = posAction,
                        TargetPositionCustomAction = targetPosAction,
                        RotationCustomAction       = rotAction
                    }, true, rotAction);
            }
            catch { }
        }
    }

    public void ProcessLineStacks(object comp, float groundY, int pcSlot, Configuration config)
    {
        if (!config.BossModMirrorLineStacks) return;

        try
        {
            var lineStacks = GetField(comp, "ActiveLineStacks") as IEnumerable
                          ?? GetField(comp, "LineStacks") as IEnumerable
                          ?? GetField(comp, "_lineStacks") as IEnumerable;

            if (lineStacks == null && comp.GetType().Name.Contains("LineStack", StringComparison.OrdinalIgnoreCase))
            {
                var caster = GetField(comp, "Caster") ?? GetField(comp, "Source");
                var target = GetField(comp, "Target") ?? GetField(comp, "TargetActor");
                if (caster != null && target != null)
                {
                    EmitSingleLineStack(caster, target, comp, groundY);
                }
            }
            else if (lineStacks != null)
            {
                foreach (var ls in lineStacks)
                {
                    if (ls == null) continue;
                    var caster = GetField(ls, "Caster") ?? GetField(ls, "Source") ?? GetField(comp, "Caster");
                    var target = GetField(ls, "Target") ?? GetField(ls, "TargetActor");
                    if (caster != null && target != null)
                    {
                        EmitSingleLineStack(caster, target, ls, groundY);
                    }
                }
            }
        }
        catch { }
    }

    private void EmitSingleLineStack(object caster, object target, object info, float groundY)
    {
        var srcPos = GetField(caster, "Position");
        var tgtPos = GetField(target, "Position");
        if (srcPos == null || tgtPos == null) return;

        uint actionId = BossModAoeProcessor.ExtractActionId(info, null);
        float ox = FX(srcPos), oz = FZ(srcPos);
        float tx = FX(tgtPos), tz = FZ(tgtPos);
        float dx = tx - ox, dz = tz - oz;
        float len = MathF.Sqrt(dx * dx + dz * dz);
        if (len < 0.1f) return;

        float rot = MathF.Atan2(dx, dz);
        float hw = Ff(GetField(info, "HalfWidth") ?? GetField(info, "Radius"), 3f);
        float totalLen = Math.Max(len + 5f, 40f);

        ulong srcId = UL(GetField(caster, "InstanceID"));
        ulong tgtId = UL(GetField(target, "InstanceID"));
        uint se = EntityId(srcId), te = EntityId(tgtId);
        var pos = new Vector3(ox, groundY, oz);

        lock (_shapeMapper.MapAoes)
        {
            _shapeMapper.MapAoes.Add(new MapAoe(MapAoeKind.Rect, false, ox, oz, rot, totalLen, 0f, hw, 0xFF00FFFF, 0, se, actionId, te));
        }

        _vfxEmitter.Emit(K_RECT, (srcId ^ tgtId) ^ 0x57AC, ox, oz, rot, hw, totalLen, 0,
            new DrawElement
            {
                drawAvfx       = "customRect",
                Position       = pos,
                drawOnObject   = false,
                radiusX        = hw,
                radiusZ        = totalLen,
                refRotation    = new Angle(rot),
                fixRotation    = true,
                refColor       = ColStack,
                refTargetColor = ColStack,
                destroyTime    = 60000f,
            }, false, null);
    }

    public void ProcessExaflares(object comp, float groundY, Configuration config)
    {
        if (!config.BossModMirrorExaflares) return;

        try
        {
            var lines = GetField(comp, "Lines") as IEnumerable
                     ?? GetField(comp, "_lines") as IEnumerable
                     ?? GetField(comp, "Sequences") as IEnumerable;

            if (lines != null)
            {
                uint actionId = BossModAoeProcessor.ExtractActionId(null, comp);
                var compShape = GetField(comp, "Shape") ?? GetField(comp, "_shape");
                int lineIndex = 0;

                foreach (var line in lines)
                {
                    if (line == null) continue;
                    var next = GetField(line, "Next") ?? GetField(line, "NextOrigin") ?? GetField(line, "Position");
                    var advance = GetField(line, "Advance");
                    if (next == null) continue;

                    var shape = compShape ?? GetField(line, "Shape") ?? GetField(line, "NextShape");
                    if (shape == null) continue;

                    float ox = FX(next), oz = FZ(next);
                    float advX = advance != null ? FX(advance) : 0f;
                    float advZ = advance != null ? FZ(advance) : 0f;
                    float rot = RotRad(GetField(line, "Rotation"));
                    int explosionsLeft = Ff(GetField(line, "ExplosionsLeft"), 1) is float el ? (int)el : 1;
                    int maxShown = Ff(GetField(line, "MaxShownExplosions"), 1) is float ms ? (int)ms : 1;

                    ulong lineHash = (ulong)comp.GetType().GetHashCode() ^ (ulong)(lineIndex++) * 10007UL;

                    // 1. Current imminent explosion
                    _shapeMapper.EmitShape(shape, ox, oz, rot, groundY, ColDanger, lineHash, 0, null, null, actionId);

                    // 2. Projected future step
                    if (maxShown > 1 && (MathF.Abs(advX) > 0.001f || MathF.Abs(advZ) > 0.001f) && explosionsLeft > 1)
                    {
                        float fOx = ox + advX;
                        float fOz = oz + advZ;
                        var fColor = new Vector4(1f, 0.60f, 0.15f, 0.6f);

                        _shapeMapper.EmitShape(shape, fOx, fOz, rot, groundY, fColor, lineHash + 1, 0, null, null, actionId);

                        // Add trajectory arrow to overlay
                        _overlay.AddArrow(
                            new OverlayArrow(
                                new Vector3(ox, groundY, oz),
                                new Vector3(ox + advX * 2.5f, groundY, oz + advZ * 2.5f),
                                0xFF00A5FF
                            )
                        );
                    }
                }
            }
        }
        catch { }
    }
}
