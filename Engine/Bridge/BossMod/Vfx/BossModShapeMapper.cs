using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Element;
using Replica.Engine.Util;
using Replica.Logging;
using static Replica.Engine.Bridge.BossMod.Core.BossModConstants;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Vfx;

public sealed class BossModShapeMapper
{
    private readonly BossModVfxEmitter _emitter;
    private readonly List<MapAoe> _mapAoes = new(64);

    public List<MapAoe> MapAoes => _mapAoes;

    public BossModShapeMapper(BossModVfxEmitter emitter)
    {
        _emitter = emitter;
    }

    public void BeginFrame()
    {
        lock (_mapAoes)
        {
            _mapAoes.Clear();
        }
    }

    public void EmitShape(
        object shape,
        float ox, float oz, float rotRad, float groundY,
        Vector4 color,
        ulong srcId, ulong tgtId,
        Func<Vector3>? posAction,
        Func<Angle>? rotAction,
        uint actionId = 0)
    {
        string tn = shape.GetType().Name;
        bool tracked = posAction != null;
        var initPos = new Vector3(ox, groundY, oz);
        bool isSafe = (color.Y > 0.7f && color.X < 0.6f) || color == ColSafe;
        uint sId = EntityId(srcId);
        uint tId = EntityId(tgtId);

        if (tn.Contains("Circle"))
        {
            float r = Ff(GetField(shape, "Radius"), 5f);
            lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Circle, isSafe, ox, oz, 0f, r, 0f, 0f, 0, 0, sId, actionId, tId)); }
            _emitter.Emit(K_CIRCLE, srcId ^ tgtId, ox, oz, 0, r, 0, 0,
                new DrawElement
                {
                    drawAvfx             = "customCircle",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = r,
                    radiusZ              = r,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    fixRotation          = true,
                    PositionCustomAction = posAction
                }, tracked, null);
        }
        else if (tn.Contains("Donut") && tn.Contains("Sector"))
        {
            float outer = Ff(GetField(shape, "OuterRadius"), 15f);
            float inner = Ff(GetField(shape, "InnerRadius"),  0f);
            float ha    = Ff(GetField(GetField(shape, "HalfAngle"), "Rad"), 0.785f);
            float doff  = Ff(GetField(GetField(shape, "DirectionOffset"), "Rad"), 0f);
            float fr    = rotRad + doff;
            lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Donut, isSafe, ox, oz, fr, outer, inner, ha, 0, 0, sId, actionId, tId)); }
            _emitter.Emit(K_DONUTS, srcId ^ tgtId, ox, oz, fr, outer, inner, ha,
                new DrawElement
                {
                    drawAvfx             = "customFan",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = outer,
                    radiusZ              = outer,
                    refRadian            = ha * 2f,
                    refRotation          = new Angle(fr),
                    fixRotation          = true,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    PositionCustomAction = posAction,
                    RotationCustomAction = rotAction
                }, tracked, rotAction);
        }
        else if (tn.Contains("Donut"))
        {
            float outer = Ff(GetField(shape, "OuterRadius"), 15f);
            float inner = Ff(GetField(shape, "InnerRadius"),  5f);
            float ratio = outer > 0f ? Math.Clamp(inner / outer, 0.01f, 0.99f) : 0.5f;
            lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Donut, isSafe, ox, oz, 0f, outer, inner, 0f, 0, 0, sId, actionId, tId)); }
            _emitter.Emit(K_DONUT, srcId ^ tgtId, ox, oz, 0, outer, inner, 0,
                new DrawElement
                {
                    drawAvfx             = "customDonut",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = outer,
                    radiusZ              = outer,
                    refRadian            = ratio,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    fixRotation          = true,
                    PositionCustomAction = posAction
                }, tracked, null);
        }
        else if (tn.Contains("Cone") || tn.Contains("TriCone"))
        {
            float r    = Ff(GetField(shape, "Radius") ?? GetField(shape, "SideLength"), 10f);
            float ha   = Ff(GetField(GetField(shape, "HalfAngle"), "Rad"), 0.785f);
            float doff = Ff(GetField(GetField(shape, "DirectionOffset"), "Rad"), 0f);
            float fr   = rotRad + doff;
            lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Cone, isSafe, ox, oz, fr, r, ha, 0f, 0, 0, sId, actionId, tId)); }
            _emitter.Emit(K_CONE, srcId ^ tgtId, ox, oz, fr, r, ha, 0,
                new DrawElement
                {
                    drawAvfx             = "customFan",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = r,
                    radiusZ              = r,
                    refRadian            = ha * 2f,
                    refRotation          = new Angle(fr),
                    fixRotation          = true,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    PositionCustomAction = posAction,
                    RotationCustomAction = rotAction
                }, tracked, rotAction);
        }
        else if (tn.Contains("Cross"))
        {
            float len  = Ff(GetField(shape, "Length"), 15f);
            float hw   = Ff(GetField(shape, "HalfWidth"), 3f);
            float doff = Ff(GetField(GetField(shape, "DirectionOffset"), "Rad"), 0f);
            float fr   = rotRad + doff;
            float fr2  = fr + MathF.PI / 2f;
            lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Cross, isSafe, ox, oz, fr, len, hw, 0f, 0, 0, sId, actionId, tId)); }

            // Arm 1
            _emitter.Emit(K_CROSS, srcId ^ tgtId, ox, oz, fr, len, hw, 0,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = hw,
                    radiusZ              = len * 2f,
                    refOffsetZ           = len,
                    refRotation          = new Angle(fr),
                    fixRotation          = true,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    PositionCustomAction = posAction,
                    RotationCustomAction = rotAction
                }, tracked, rotAction);

            // Arm 2
            Func<Angle>? rotAction2 = rotAction != null ? () => new Angle(rotAction().Rad + MathF.PI / 2f) : null;
            _emitter.Emit(K_CROSS + 100, srcId ^ tgtId, ox, oz, fr2, len, hw, 0,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = hw,
                    radiusZ              = len * 2f,
                    refOffsetZ           = len,
                    refRotation          = new Angle(fr2),
                    fixRotation          = true,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    PositionCustomAction = posAction,
                    RotationCustomAction = rotAction2
                }, tracked, rotAction2);
        }
        else if (tn.Contains("Capsule") || tn.Contains("ArcCapsule"))
        {
            float r    = Ff(GetField(shape, "Radius"), 3f);
            float len  = Ff(GetField(shape, "Length"), 10f);
            float doff = Ff(GetField(GetField(shape, "DirectionOffset"), "Rad"), 0f);
            float fr   = rotRad + doff;
            lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Rect, isSafe, ox, oz, fr, len, 0f, r, 0, 0, sId, actionId, tId)); }
            _emitter.Emit(K_CAPSULE, srcId ^ tgtId, ox, oz, fr, r, len, 0,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = r,
                    radiusZ              = len,
                    refRotation          = new Angle(fr),
                    fixRotation          = true,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    PositionCustomAction = posAction,
                    RotationCustomAction = rotAction
                }, tracked, rotAction);
        }
        else if (tn.Contains("Custom"))
        {
            var shapes1 = GetField(shape, "Shapes1") as IEnumerable;
            if (shapes1 != null)
            {
                int subIdx = 0;
                foreach (var sub in shapes1)
                {
                    if (sub == null) continue;
                    var subName = sub.GetType().Name;
                    var subPos = GetField(sub, "Center") ?? GetField(sub, "Position");
                    float sx = subPos != null ? FX(subPos) : ox;
                    float sz = subPos != null ? FZ(subPos) : oz;

                    if (subName.Contains("Circle") || subName.Contains("Polygon"))
                    {
                        float r = Ff(GetField(sub, "Radius"), 10f);
                        lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Circle, isSafe, sx, sz, 0f, r, 0f, 0f, 0, 0, sId, actionId, tId)); }
                        _emitter.Emit(K_CIRCLE, srcId ^ (ulong)(subIdx++ * 1009), sx, sz, 0, r, 0, 0,
                            new DrawElement
                            {
                                drawAvfx       = "customCircle",
                                Position       = new Vector3(sx, groundY, sz),
                                drawOnObject   = false,
                                radiusX        = r,
                                radiusZ        = r,
                                refColor       = color,
                                refTargetColor = color,
                                destroyTime    = 60000f,
                                fixRotation    = true,
                            }, false, null);
                    }
                    else if (subName.Contains("Donut"))
                    {
                        float outer = Ff(GetField(sub, "OuterRadius") ?? GetField(sub, "Radius"), 15f);
                        float inner = Ff(GetField(sub, "InnerRadius"), 5f);
                        float ratio = outer > 0f ? Math.Clamp(inner / outer, 0.01f, 0.99f) : 0.5f;
                        lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Donut, isSafe, sx, sz, 0f, outer, inner, 0f, 0, 0, sId, actionId, tId)); }
                        _emitter.Emit(K_DONUT, srcId ^ (ulong)(subIdx++ * 1009), sx, sz, 0, outer, inner, 0,
                            new DrawElement
                            {
                                drawAvfx       = "customDonut",
                                Position       = new Vector3(sx, groundY, sz),
                                drawOnObject   = false,
                                radiusX        = outer,
                                radiusZ        = outer,
                                refRadian      = ratio,
                                refColor       = color,
                                refTargetColor = color,
                                destroyTime    = 60000f,
                                fixRotation    = true,
                            }, false, null);
                    }
                }
            }
        }
        else  // Rect (LengthFront, LengthBack, HalfWidth)
        {
            float lf   = Ff(GetField(shape, "LengthFront") ?? GetField(shape, "Length"), 15f);
            float lb   = Ff(GetField(shape, "LengthBack"), 0f);
            float hw   = Ff(GetField(shape, "HalfWidth"), 3f);
            float doff = Ff(GetField(GetField(shape, "DirectionOffset"), "Rad"), 0f);
            float fr   = rotRad + doff;
            float total = lf + lb;
            lock (_mapAoes) { _mapAoes.Add(new MapAoe(MapAoeKind.Rect, isSafe, ox, oz, fr, lf, lb, hw, 0, 0, sId, actionId, tId)); }

            _emitter.Emit(K_RECT, srcId ^ tgtId, ox, oz, fr, hw, total, lb,
                new DrawElement
                {
                    drawAvfx             = "customRect",
                    Position             = initPos,
                    drawOnObject         = false,
                    radiusX              = hw,
                    radiusZ              = total,
                    refOffsetZ           = lb,
                    refRotation          = new Angle(fr),
                    fixRotation          = true,
                    refColor             = color,
                    refTargetColor       = color,
                    destroyTime          = 60000f,
                    PositionCustomAction = posAction,
                    RotationCustomAction = rotAction
                }, tracked, rotAction);
        }
    }
}
