using System;
using System.Collections;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.Utility;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Bridge.BossMod.Overlay;
using Replica.Engine.Bridge.BossMod.Reflection;
using Replica.Engine.Bridge.BossMod.Vfx;
using Replica.Engine.Element;
using Replica.Engine.Util;
using static Replica.Engine.Bridge.BossMod.Core.BossModConstants;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Processors;

public sealed class BossModTetherGazeProcessor
{
    private readonly BossModVfxEmitter _vfxEmitter;
    private readonly BossModOverlayRenderer _overlay;

    public BossModTetherGazeProcessor(
        BossModVfxEmitter vfxEmitter,
        BossModOverlayRenderer overlay)
    {
        _vfxEmitter = vfxEmitter;
        _overlay = overlay;
    }

    public void Process(object comp, BossModContext ctx)
    {
        var config = ctx.Plugin.Configuration;

        // 1. Tethers
        if (config.BossModMirrorTethers)
        {
            try { ProcessComponentTethers(comp, ctx.Module, ctx.GroundY, ctx.PcSlot, config); } catch { }
        }

        // 2. Gazes / Eyes
        if (config.BossModMirrorGaze)
        {
            try { ProcessComponentGazes(comp, ctx); } catch { }
        }

        // 3. Return / Rewind spots
        if (config.BossModMirrorReturnSpots)
        {
            try { ProcessComponentReturnSpots(comp, ctx.GroundY, ctx.PcSlot, config); } catch { }
        }

        // 4. Partner Tether Helper (Part 5: Raid Roles & Groups)
        if (config.BossModMirrorPartnerTetherHelper)
        {
            try { ProcessComponentPartners(comp, ctx); } catch { }
        }
    }

    public void ProcessComponentTethers(object comp, object module, float groundY, int pcSlot, Configuration config)
    {
        try
        {
            float thickness = MathF.Max(1.5f, config.BossModTetherThickness * ImGuiHelpers.GlobalScale);

            // 1. Check array _tetherTargets (Actor?[]) indexed by player slot
            if (GetField(comp, "_tetherTargets") is Array targetsArr)
            {
                var ws = Get(module, "WorldState");
                var party = ws != null ? Get(ws, "Party") : null;

                for (int i = 0; i < targetsArr.Length; i++)
                {
                    var targetActor = targetsArr.GetValue(i);
                    if (targetActor == null) continue;

                    object? sourceActor = null;
                    if (party != null)
                    {
                        var indexer = party.GetType().GetProperty("Item", [typeof(int)]);
                        sourceActor = indexer?.GetValue(party, [i]);
                    }

                    if (sourceActor != null)
                    {
                        var srcPos = GetField(sourceActor, "Position");
                        var tgtPos = GetField(targetActor, "Position");
                        if (srcPos != null && tgtPos != null)
                        {
                            Vector3 p1 = new Vector3(FX(srcPos), groundY, FZ(srcPos));
                            Vector3 p2 = new Vector3(FX(tgtPos), groundY, FZ(tgtPos));
                            float distSq = Vector3.DistanceSquared(p1, p2);

                            // Light Rampant logic: dist >= 25m (625) is safe (green), else red
                            uint col = distSq >= 625f ? 0xFF00FF00 : 0xFF0000FF;
                            _overlay.AddTether(new OverlayTether(p1, p2, col, thickness));
                        }
                    }
                }
            }

            // 2. Check collection Tethers / ActiveTethers / _tethers
            var tethersList = GetField(comp, "ActiveTethers") as IEnumerable
                           ?? GetField(comp, "Tethers") as IEnumerable
                           ?? GetField(comp, "_tethers") as IEnumerable;

            if (tethersList != null)
            {
                foreach (var t in tethersList)
                {
                    if (t == null) continue;
                    var source = GetField(t, "Source") ?? GetField(t, "SourceActor") ?? GetField(t, "Item1");
                    var target = GetField(t, "Target") ?? GetField(t, "TargetActor") ?? GetField(t, "Item2");
                    if (source != null && target != null)
                    {
                        var srcPos = GetField(source, "Position");
                        var tgtPos = GetField(target, "Position");
                        if (srcPos != null && tgtPos != null)
                        {
                            Vector3 p1 = new Vector3(FX(srcPos), groundY, FZ(srcPos));
                            Vector3 p2 = new Vector3(FX(tgtPos), groundY, FZ(tgtPos));

                            uint col = 0xFF00FFFF; // Default yellow/cyan tether
                            var colObj = GetField(t, "Color");
                            if (colObj is uint c && c != 0) col = c;

                            _overlay.AddTether(new OverlayTether(p1, p2, col, thickness));
                        }
                    }
                }
            }
        }
        catch { }
    }

    public void ProcessComponentGazes(object comp, BossModContext ctx)
    {
        try
        {
            string typeName = comp.GetType().Name;
            bool isGazeComp = typeName.Contains("Gaze", StringComparison.OrdinalIgnoreCase)
                           || typeName.Contains("Eye", StringComparison.OrdinalIgnoreCase);

            if (!isGazeComp) return;

            // 1. Dynamic ActiveEyes extraction
            bool gazeExtracted = false;
            try
            {
                var gazeExtractor = BossModFastExtractors.GetActiveEyesExtractor(comp.GetType());
                if (gazeExtractor != null)
                {
                    gazeExtractor(comp, ctx.PcSlot, ctx.PcActor, eye => ProcessEyeItem(eye, ctx.GroundY));
                    gazeExtracted = true;
                }
            }
            catch { }

            if (!gazeExtracted)
            {
                var gazesList = GetField(comp, "ActiveGazes") as IEnumerable
                             ?? GetField(comp, "Gazes") as IEnumerable
                             ?? GetField(comp, "Eyes") as IEnumerable
                             ?? GetField(comp, "_gazes") as IEnumerable;

                if (gazesList != null)
                {
                    foreach (var g in gazesList)
                    {
                        if (g == null) continue;
                        var actor = GetField(g, "Actor") ?? GetField(g, "Source") ?? GetField(g, "Origin") ?? GetField(g, "Item1");
                        var posObj = actor != null ? (GetField(actor, "Position") ?? actor) : GetField(g, "Position");
                        if (posObj != null)
                        {
                            float ox = FX(posObj), oz = FZ(posObj);
                            var worldPos = new Vector3(ox, ctx.GroundY + 1.8f, oz);
                            ulong actorId = actor != null ? UL(GetField(actor, "InstanceID")) : 0;

                            _vfxEmitter.Emit(K_GAZE, actorId ^ 0x6A2E, ox, oz, 0, 1f, 0, 0,
                                new DrawElement
                                {
                                    drawAvfx     = "eye_warn",
                                    Position     = worldPos,
                                    drawOnObject = true,
                                    radiusX      = 1f,
                                    radiusZ      = 1f,
                                    refColor     = ColDanger,
                                    destroyTime  = 60000f,
                                    fixRotation  = true,
                                    }, false, null);

                            _overlay.AddGaze(new OverlayGaze(worldPos, new Angle(0), new Angle(MathF.PI), 0xFFFF00FF));
                        }
                    }
                }
                else
                {
                    var actor = GetField(comp, "Source") ?? GetField(comp, "Actor") ?? GetField(comp, "Caster");
                    var posObj = actor != null ? GetField(actor, "Position") : GetField(comp, "Position");
                    if (posObj != null)
                    {
                        float ox = FX(posObj), oz = FZ(posObj);
                        var worldPos = new Vector3(ox, ctx.GroundY + 1.8f, oz);
                        ulong actorId = actor != null ? UL(GetField(actor, "InstanceID")) : 0;

                        _vfxEmitter.Emit(K_GAZE, actorId ^ 0x6A2E, ox, oz, 0, 1f, 0, 0,
                            new DrawElement
                            {
                                drawAvfx     = "eye_warn",
                                Position     = worldPos,
                                drawOnObject = true,
                                radiusX      = 1f,
                                radiusZ      = 1f,
                                refColor     = ColDanger,
                                destroyTime  = 60000f,
                                fixRotation  = true,
                            }, false, null);

                        _overlay.AddGaze(new OverlayGaze(worldPos, new Angle(0), new Angle(MathF.PI), 0xFFFF00FF));
                    }
                }
            }
        }
        catch { }
    }

    public void ProcessEyeItem(object? eye, float groundY)
    {
        if (eye == null) return;
        var posObj = GetField(eye, "Position");
        if (posObj == null) return;
        float ox = FX(posObj), oz = FZ(posObj);
        var worldPos = new Vector3(ox, groundY + 1.8f, oz);
        var fwd = GetField(eye, "Forward");
        float rot = fwd != null ? RotRad(fwd) : 0f;

        _vfxEmitter.Emit(K_GAZE, (ulong)posObj.GetHashCode() ^ 0x6A2E, ox, oz, rot, 1f, 0, 0,
            new DrawElement
            {
                drawAvfx     = "eye_warn",
                Position     = worldPos,
                drawOnObject = true,
                radiusX      = 1f,
                radiusZ      = 1f,
                refColor     = ColDanger,
                destroyTime  = 60000f,
                fixRotation  = true,
            }, false, null);

        _overlay.AddGaze(new OverlayGaze(worldPos, new Angle(rot), new Angle(MathF.PI), 0xFFFF00FF));
    }

    public void ProcessComponentReturnSpots(object comp, float groundY, int pcSlot, Configuration config)
    {
        if (!config.BossModMirrorReturnSpots) return;

        try
        {
            if (GetField(comp, "States") is Array statesArr && pcSlot >= 0 && pcSlot < statesArr.Length)
            {
                var myState = statesArr.GetValue(pcSlot);
                if (myState != null)
                {
                    var rpos = GetField(myState, "ReturnPos") ?? GetField(myState, "ReturnPosition");
                    if (rpos != null)
                    {
                        float rx = FX(rpos), rz = FZ(rpos);
                        if (MathF.Abs(rx) > 0.001f || MathF.Abs(rz) > 0.001f)
                        {
                            var pos = new Vector3(rx, groundY, rz);
                            _overlay.AddReturnSpot(new OverlayReturnSpot(pos, "RETURN", 0xFF00FFFF));
                        }
                    }
                }
            }

            var retPos = GetField(comp, "ReturnPos")
                      ?? GetField(comp, "ReturnPosition")
                      ?? GetField(comp, "_returnPos")
                      ?? GetField(comp, "SavedPosition")
                      ?? GetField(comp, "_savedPosition");
            if (retPos != null)
            {
                float rx = FX(retPos), rz = FZ(retPos);
                if (MathF.Abs(rx) > 0.001f || MathF.Abs(rz) > 0.001f)
                {
                    var pos = new Vector3(rx, groundY, rz);
                    _overlay.AddReturnSpot(new OverlayReturnSpot(pos, "RETURN", 0xFF00FFFF));
                }
            }
        }
        catch { }
    }

    public void ProcessComponentPartners(object comp, BossModContext ctx)
    {
        int pcSlot = ctx.PcSlot;
        if (pcSlot < 0 || pcSlot >= 64) return;

        var partner = ExtractPartner(comp, ctx, pcSlot);
        if (partner == null) return;

        if (IsActor(partner))
        {
            ulong pcId = ctx.PcActor != null ? UL(GetField(ctx.PcActor, "InstanceID")) : 0;
            ulong partnerId = UL(GetField(partner, "InstanceID"));
            if (partnerId != 0 && partnerId != pcId)
            {
                var partnerPos = GetField(partner, "Position");
                if (partnerPos != null)
                {
                    float thickness = MathF.Max(1.5f, ctx.Plugin.Configuration.BossModTetherThickness * ImGuiHelpers.GlobalScale);
                    Vector3 p1 = new Vector3(ctx.LocalPlayer.Position.X, ctx.GroundY, ctx.LocalPlayer.Position.Z);
                    Vector3 p2 = new Vector3(FX(partnerPos), ctx.GroundY, FZ(partnerPos));

                    uint col = ctx.Plugin.Configuration.BossModPartnerTetherColor;
                    _overlay.AddTether(new OverlayTether(p1, p2, col, thickness));
                }
            }
        }
    }

    private object? ExtractPartner(object comp, BossModContext ctx, int pcSlot)
    {
        var type = comp.GetType();

        // 1. FindPartner method
        var findPartner = type.GetMethod("FindPartner",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (findPartner != null)
        {
            try
            {
                var res = findPartner.Invoke(comp, [pcSlot]);
                if (res != null) return ResolvePartner(res, ctx);
            }
            catch { }
        }

        // 2. Partner / GetPartner methods
        var partnerMethod = type.GetMethod("Partner",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null, [typeof(int)], null)
            ?? type.GetMethod("GetPartner",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null, [typeof(int)], null);
        if (partnerMethod != null)
        {
            try
            {
                var res = partnerMethod.Invoke(comp, [pcSlot]);
                if (res != null) return ResolvePartner(res, ctx);
            }
            catch { }
        }

        // 3. Fields
        var fields = GetAllFieldsCached(type);
        foreach (var f in fields)
        {
            var name = f.Name;
            if (name.Contains("partner", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("tether", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("assign", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("soaker", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var val = f.GetValue(comp);
                    if (val != null)
                    {
                        var res = GetValueFromCollection(val, pcSlot);
                        if (res != null)
                        {
                            var resolved = ResolvePartner(res, ctx);
                            if (resolved != null) return resolved;
                        }
                    }
                }
                catch { }
            }
        }

        // 4. Properties
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var p in props)
        {
            var name = p.Name;
            if (name.Contains("partner", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("tether", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("assign", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("soaker", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var val = p.GetValue(comp);
                    if (val != null)
                    {
                        var res = GetValueFromCollection(val, pcSlot);
                        if (res != null)
                        {
                            var resolved = ResolvePartner(res, ctx);
                            if (resolved != null) return resolved;
                        }
                    }
                }
                catch { }
            }
        }

        return null;
    }

    private object? GetValueFromCollection(object obj, int index)
    {
        if (obj is Array arr)
        {
            if (index >= 0 && index < arr.Length)
            {
                return arr.GetValue(index);
            }
        }
        else if (obj is System.Collections.IList list)
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
        }
        return null;
    }

    private object? ResolvePartner(object obj, BossModContext ctx)
    {
        if (obj == null) return null;

        // If it's directly an Actor
        if (IsActor(obj)) return obj;

        // If it's a tuple (Item1, Item2)
        var item1 = GetField(obj, "Item1");
        if (item1 != null && IsActor(item1)) return item1;

        // If it's a slot index (numeric value)
        if (obj is int slotInt)
        {
            return GetActorBySlot(slotInt, ctx);
        }
        if (obj is uint slotUint)
        {
            return GetActorBySlot((int)slotUint, ctx);
        }
        if (obj is byte slotByte)
        {
            return GetActorBySlot((int)slotByte, ctx);
        }

        return null;
    }

    private object? GetActorBySlot(int slot, BossModContext ctx)
    {
        if (slot < 0 || slot >= 64) return null;
        var raid = Get(ctx.Module, "Raid");
        if (raid != null)
        {
            var indexer = raid.GetType().GetProperty("Item", [typeof(int)]);
            return indexer?.GetValue(raid, [slot]);
        }
        return null;
    }

    private static bool IsActor(object obj)
    {
        if (obj == null) return false;
        return GetField(obj, "Position") != null && GetField(obj, "InstanceID") != null;
    }
}
