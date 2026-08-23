using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Bridge.BossMod.Overlay;
using Replica.Engine.Bridge.BossMod.Vfx;
using Replica.Engine.Element;
using Replica.Engine.Util;
using static Replica.Engine.Bridge.BossMod.Core.BossModConstants;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Processors;

public sealed class BossModAiHintsProcessor
{
    private readonly BossModVfxEmitter _vfxEmitter;
    private readonly BossModOverlayRenderer _overlay;

    // AIHints caching
    private object? _aiHints;
    private Type? _aiHintsType;
    private MethodInfo? _aiHintsClearMethod;
    private MethodInfo? _calculateAIHintsMethod;

    private readonly List<OverlayArrow> _tempArrows = new(16);

    public BossModAiHintsProcessor(
        BossModVfxEmitter vfxEmitter,
        BossModOverlayRenderer overlay)
    {
        _vfxEmitter = vfxEmitter;
        _overlay = overlay;
    }

    public void ProcessAIHints(BossModContext ctx)
    {
        var config = ctx.Plugin.Configuration;
        if (!config.BossModMirrorSafeZones || ctx.PcActor == null) return;

        try
        {
            if (_aiHints == null)
            {
                var modAsm = ctx.Module.GetType().Assembly;
                _aiHintsType = modAsm.GetType("BossMod.AIHints");
                if (_aiHintsType != null)
                {
                    _aiHints = Activator.CreateInstance(_aiHintsType);
                    _aiHintsClearMethod = _aiHintsType.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                }
            }

            if (_aiHints == null) return;

            _aiHintsClearMethod?.Invoke(_aiHints, null);

            // Call module.CalculateAIHints(slot, actor, assignment, hints)
            if (_calculateAIHintsMethod == null)
            {
                _calculateAIHintsMethod = ctx.Module.GetType().GetMethod("CalculateAIHints",
                    BindingFlags.Public | BindingFlags.Instance);
            }

            if (_calculateAIHintsMethod != null)
            {
                var paramsInfo = _calculateAIHintsMethod.GetParameters();
                if (paramsInfo.Length == 4)
                {
                    object? assignmentVal = paramsInfo[2].ParameterType.IsValueType
                        ? Activator.CreateInstance(paramsInfo[2].ParameterType)
                        : null;
                    _calculateAIHintsMethod.Invoke(ctx.Module, [ctx.PcSlot, ctx.PcActor, assignmentVal, _aiHints]);
                }
            }

            // Fallback: If CalculateAIHints did not populate, call AddAIHints on individual components
            var forbiddenZones = GetField(_aiHints, "ForbiddenZones") as IEnumerable;
            if (forbiddenZones == null || !forbiddenZones.GetEnumerator().MoveNext())
            {
                if (Get(ctx.Module, "Components") is IEnumerable comps)
                {
                    foreach (var comp in comps)
                    {
                        if (comp == null) continue;
                        var addAiMethod = comp.GetType().GetMethod("AddAIHints", BindingFlags.Public | BindingFlags.Instance);
                        if (addAiMethod != null)
                        {
                            var p = addAiMethod.GetParameters();
                            if (p.Length == 4)
                            {
                                object? assign = p[2].ParameterType.IsValueType ? Activator.CreateInstance(p[2].ParameterType) : null;
                                try { addAiMethod.Invoke(comp, [ctx.PcSlot, ctx.PcActor, assign, _aiHints]); } catch { }
                            }
                        }
                    }
                }
                forbiddenZones = GetField(_aiHints, "ForbiddenZones") as IEnumerable;
            }

            if (forbiddenZones != null)
            {
                foreach (var item in forbiddenZones)
                {
                    if (item == null) continue;
                    var shapeDistance = GetField(item, "Item1") ?? GetField(item, "shapeDistance") ?? GetField(item, "ShapeDistance");
                    var sourceId = UL(GetField(item, "Item3") ?? GetField(item, "Source") ?? GetField(item, "source"));
                    if (shapeDistance != null)
                    {
                        ProcessShapeDistanceSafeZone(shapeDistance, ctx.GroundY, sourceId);
                    }
                }
            }
        }
        catch { }
    }

    public void ProcessShapeDistanceSafeZone(object shapeDistance, float groundY, ulong sourceId)
    {
        if (shapeDistance == null) return;
        string tn = shapeDistance.GetType().Name;

        // Inverted Circle: standard circular safe spot
        if (tn.Contains("InvertedCircle") || (tn.Contains("Circle") && tn.Contains("Invert")))
        {
            float ox = Ff(GetField(shapeDistance, "originX") ?? GetField(shapeDistance, "OriginX") ?? GetField(GetField(shapeDistance, "Origin"), "X"));
            float oz = Ff(GetField(shapeDistance, "originZ") ?? GetField(shapeDistance, "OriginZ") ?? GetField(GetField(shapeDistance, "Origin"), "Z"));
            float radius = Ff(GetField(shapeDistance, "radius") ?? GetField(shapeDistance, "Radius"), 3f);

            if (radius <= 0f) radius = 3f;
            var pos = new Vector3(ox, groundY, oz);

            _vfxEmitter.Emit(K_CIRCLE, sourceId ^ 0x5AFE, ox, oz, 0, radius, 0, 0,
                new DrawElement
                {
                    drawAvfx             = "customCircle",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = radius,
                    radiusZ              = radius,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                    fixRotation          = true,
                }, false, null);

            _overlay.AddSafeSpot(new OverlaySafeSpot(pos, radius, 0xFF00FF00));
        }
        // Inverted Donut Sector
        else if (tn.Contains("InvertedDonutSector") || (tn.Contains("DonutSector") && tn.Contains("Invert")))
        {
            float ox = Ff(GetField(shapeDistance, "originX") ?? GetField(GetField(shapeDistance, "Origin"), "X"));
            float oz = Ff(GetField(shapeDistance, "originZ") ?? GetField(GetField(shapeDistance, "Origin"), "Z"));
            float outer = Ff(GetField(shapeDistance, "outerRadius") ?? GetField(shapeDistance, "OuterRadius"), 15f);
            float inner = Ff(GetField(shapeDistance, "innerRadius") ?? GetField(shapeDistance, "InnerRadius"), 0f);
            float ha = Ff(GetField(GetField(shapeDistance, "HalfAngle"), "Rad") ?? GetField(shapeDistance, "halfAngle"), 0.785f);
            float cd = RotRad(GetField(shapeDistance, "CenterDir") ?? GetField(shapeDistance, "centerDir"));
            var pos = new Vector3(ox, groundY, oz);

            _vfxEmitter.Emit(K_DONUTS, sourceId ^ 0x5AFE, ox, oz, cd, outer, inner, ha,
                new DrawElement
                {
                    drawAvfx             = "customFan",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = outer,
                    radiusZ              = outer,
                    refRadian            = ha * 2f,
                    refRotation          = new Angle(cd),
                    fixRotation          = true,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                }, false, null);
        }
        // Inverted Donut
        else if (tn.Contains("InvertedDonut") || (tn.Contains("Donut") && tn.Contains("Invert")))
        {
            float ox = Ff(GetField(shapeDistance, "originX") ?? GetField(GetField(shapeDistance, "Origin"), "X"));
            float oz = Ff(GetField(shapeDistance, "originZ") ?? GetField(GetField(shapeDistance, "Origin"), "Z"));
            float outer = Ff(GetField(shapeDistance, "outerRadius") ?? GetField(shapeDistance, "OuterRadius"), 15f);
            float inner = Ff(GetField(shapeDistance, "innerRadius") ?? GetField(shapeDistance, "InnerRadius"), 5f);
            float ratio = outer > 0f ? Math.Clamp(inner / outer, 0.01f, 0.99f) : 0.5f;
            var pos = new Vector3(ox, groundY, oz);

            _vfxEmitter.Emit(K_DONUT, sourceId ^ 0x5AFE, ox, oz, 0, outer, inner, 0,
                new DrawElement
                {
                    drawAvfx             = "customDonut",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = outer,
                    radiusZ              = outer,
                    refRadian            = ratio,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                    fixRotation          = true,
                }, false, null);
        }
        // Inverted Cone
        else if (tn.Contains("InvertedCone") || (tn.Contains("Cone") && tn.Contains("Invert")))
        {
            float ox = Ff(GetField(shapeDistance, "originX") ?? GetField(GetField(shapeDistance, "Origin"), "X"));
            float oz = Ff(GetField(shapeDistance, "originZ") ?? GetField(GetField(shapeDistance, "Origin"), "Z"));
            float r = Ff(GetField(shapeDistance, "radius") ?? GetField(shapeDistance, "Radius"), 10f);
            float ha = Ff(GetField(GetField(shapeDistance, "HalfAngle"), "Rad") ?? GetField(shapeDistance, "halfAngle"), 0.785f);
            float cd = RotRad(GetField(shapeDistance, "CenterDir") ?? GetField(shapeDistance, "centerDir"));
            var pos = new Vector3(ox, groundY, oz);

            _vfxEmitter.Emit(K_CONE, sourceId ^ 0x5AFE, ox, oz, cd, r, ha, 0,
                new DrawElement
                {
                    drawAvfx             = "customFan",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = r,
                    radiusZ              = r,
                    refRadian            = ha * 2f,
                    refRotation          = new Angle(cd),
                    fixRotation          = true,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                }, false, null);
        }
        // Inverted Rect
        else if (tn.Contains("InvertedRect") || (tn.Contains("Rect") && tn.Contains("Invert")))
        {
            float ox = Ff(GetField(shapeDistance, "originX") ?? GetField(GetField(shapeDistance, "Origin"), "X"));
            float oz = Ff(GetField(shapeDistance, "originZ") ?? GetField(GetField(shapeDistance, "Origin"), "Z"));
            float lf = Ff(GetField(shapeDistance, "lenFront") ?? GetField(shapeDistance, "LenFront") ?? GetField(shapeDistance, "lengthFront"), 15f);
            float lb = Ff(GetField(shapeDistance, "lenBack") ?? GetField(shapeDistance, "LenBack") ?? GetField(shapeDistance, "lengthBack"), 0f);
            float hw = Ff(GetField(shapeDistance, "halfWidth") ?? GetField(shapeDistance, "HalfWidth"), 3f);
            float rot = RotRad(GetField(shapeDistance, "Direction") ?? GetField(shapeDistance, "direction") ?? GetField(shapeDistance, "Dir"));
            float total = lf + lb;
            var pos = new Vector3(ox, groundY, oz);

            _vfxEmitter.Emit(K_RECT, sourceId ^ 0x5AFE, ox, oz, rot, hw, total, lb,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = hw,
                    radiusZ              = total,
                    refOffsetZ           = lb,
                    refRotation          = new Angle(rot),
                    fixRotation          = true,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                }, false, null);
        }
        // Inverted Cross
        else if (tn.Contains("InvertedCross") || (tn.Contains("Cross") && tn.Contains("Invert")))
        {
            float ox = Ff(GetField(shapeDistance, "originX") ?? GetField(GetField(shapeDistance, "Origin"), "X"));
            float oz = Ff(GetField(shapeDistance, "originZ") ?? GetField(GetField(shapeDistance, "Origin"), "Z"));
            float len = Ff(GetField(shapeDistance, "length") ?? GetField(shapeDistance, "Length"), 15f);
            float hw = Ff(GetField(shapeDistance, "halfWidth") ?? GetField(shapeDistance, "HalfWidth"), 3f);
            float rot = RotRad(GetField(shapeDistance, "Direction") ?? GetField(shapeDistance, "direction"));
            var pos = new Vector3(ox, groundY, oz);

            _vfxEmitter.Emit(K_CROSS, sourceId ^ 0x5AFE, ox, oz, rot, len, hw, 0,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = hw,
                    radiusZ              = len * 2f,
                    refOffsetZ           = len,
                    refRotation          = new Angle(rot),
                    fixRotation          = true,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                }, false, null);

            _vfxEmitter.Emit(K_CROSS + 100, sourceId ^ 0x5AFE, ox, oz, rot + MathF.PI / 2f, len, hw, 0,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = hw,
                    radiusZ              = len * 2f,
                    refOffsetZ           = len,
                    refRotation          = new Angle(rot + MathF.PI / 2f),
                    fixRotation          = true,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                }, false, null);
        }
        // Inverted Capsule / ArcCapsule
        else if (tn.Contains("InvertedCapsule") || (tn.Contains("Capsule") && tn.Contains("Invert")))
        {
            float ox = Ff(GetField(shapeDistance, "originX") ?? GetField(GetField(shapeDistance, "Origin"), "X"));
            float oz = Ff(GetField(shapeDistance, "originZ") ?? GetField(GetField(shapeDistance, "Origin"), "Z"));
            float len = Ff(GetField(shapeDistance, "length") ?? GetField(shapeDistance, "Length"), 10f);
            float r = Ff(GetField(shapeDistance, "radius") ?? GetField(shapeDistance, "Radius"), 3f);
            float rot = RotRad(GetField(shapeDistance, "Direction") ?? GetField(shapeDistance, "direction"));
            var pos = new Vector3(ox, groundY, oz);

            _vfxEmitter.Emit(K_CAPSULE, sourceId ^ 0x5AFE, ox, oz, rot, r, len, 0,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = pos,
                    drawOnObject         = false,
                    radiusX              = r,
                    radiusZ              = len,
                    refRotation          = new Angle(rot),
                    fixRotation          = true,
                    refColor             = ColSafe,
                    refTargetColor       = ColSafe,
                    destroyTime          = 60000f,
                }, false, null);
        }
        // Inverted Union: recursively process sub-zones
        else if (tn.Contains("Union"))
        {
            var zones = GetField(shapeDistance, "Zones") as IEnumerable
                     ?? GetField(shapeDistance, "_zones") as IEnumerable;
            if (zones != null)
            {
                foreach (var z in zones)
                {
                    if (z != null) ProcessShapeDistanceSafeZone(z, groundY, sourceId);
                }
            }
        }
    }

    public void ProcessMovementHints(BossModContext ctx)
    {
        var config = ctx.Plugin.Configuration;
        if ((config.BossModMirrorMovementArrows || config.BossModMirrorSafeZones) && ctx.PcActor != null)
        {
            var moveHintsMethod = ctx.Module.GetType().GetMethod("CalculateMovementHintsForRaidMember", BindingFlags.Public | BindingFlags.Instance);
            if (moveHintsMethod != null)
            {
                try
                {
                    var hintsResult = moveHintsMethod.Invoke(ctx.Module, [ctx.PcSlot, ctx.PcActor]);
                    if (hintsResult is IEnumerable hintsList)
                    {
                        _tempArrows.Clear();
                        foreach (var hintItem in hintsList)
                        {
                            if (hintItem != null)
                            {
                                ProcessMovementHintItem(hintItem, ctx.GroundY, _tempArrows);
                            }
                        }
                        _overlay.SetArrows(_tempArrows);
                        return;
                    }
                }
                catch
                {
                    _overlay.ClearArrows();
                    return;
                }
            }
        }

        _overlay.ClearArrows();
    }

    public static void ProcessMovementHintItem(object hintItem, float groundY, List<OverlayArrow> list)
    {
        try
        {
            var startObj = GetField(hintItem, "Item1") ?? GetField(hintItem, "from");
            var endObj   = GetField(hintItem, "Item2") ?? GetField(hintItem, "to");
            var colorObj = GetField(hintItem, "Item3") ?? GetField(hintItem, "color");

            if (startObj == null || endObj == null) return;

            float startX = FX(startObj), startZ = FZ(startObj);
            float endX   = FX(endObj),   endZ   = FZ(endObj);
            uint col = colorObj is uint c && c != 0 ? c : 0xFF00FF00;

            list.Add(new OverlayArrow(
                new Vector3(startX, groundY, startZ),
                new Vector3(endX,   groundY, endZ),
                col
            ));
        }
        catch { }
    }

    public void ProcessKnockbackItem(object mov, float groundY)
    {
        try
        {
            var fromObj = GetField(mov, "Item1") ?? GetField(mov, "from");
            var toObj   = GetField(mov, "Item2") ?? GetField(mov, "to");

            if (fromObj == null || toObj == null) return;

            float fromX = FX(fromObj), fromZ = FZ(fromObj);
            float toX   = FX(toObj),   toZ   = FZ(toObj);

            _overlay.AddKnockback(new OverlayKnockback(
                new Vector3(fromX, groundY, fromZ),
                new Vector3(toX,   groundY, toZ),
                0xFFFFA500 // Vibrant orange for knockback
            ));
        }
        catch { }
    }
}
