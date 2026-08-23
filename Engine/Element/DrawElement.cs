using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Util;

namespace Replica.Engine.Element;

public class DrawElement
{
	public Actor? Actor { get; set; }

	public bool Enable { get; set; } = true;

	public ElementType drawType { get; set; }

	public string drawAvfx { get; set; } = string.Empty;

	public Vector3 Position { get; set; }

	public float radiusX { get; set; } = 3f;

	public float radiusY { get; set; } = 10f;

	public float radiusZ { get; set; } = 3f;

	public float refOffsetX { get; set; }

	public float refOffsetY { get; set; }

	public float refOffsetZ { get; set; }

	public Angle refRotation { get; set; }

	public Angle refOffsetRotation { get; set; }

	public bool fixRotation { get; set; }

	public IGameObject? target { get; set; }

	public Vector3 targetPosition { get; set; } = new Vector3(float.MinValue, float.MinValue, float.MinValue);

	public bool drawOnObject { get; set; } = true;

	public bool alwaysFaceCurrentTarget { get; set; }

	public bool alwaysDrawOnCurrentTarget { get; set; }

	public bool endToTarget { get; set; }

	public bool OnlyVisible { get; set; }

	public float destroyTime { get; set; } = 3000f;

	public float delayDrawTime { get; set; }

	public float LoopInterval { get; set; }

	public float refRadian { get; set; } = 1f;

	public Vector4 refColor { get; set; } = new Vector4(1f, 1f, 1f, Plugin.Config.CustomAlpha);

	public Vector4 refTargetColor { get; set; } = new Vector4(1f, 1f, 1f, Plugin.Config.CustomAlpha);

	public DistanceCheck? distanceCheck { get; set; }

	public TetherCheck? TetherCheck { get; set; }

	public StatusCheck? StatusCheck { get; set; }

	public HitCounter? hitCounter { get; set; }

	public CountCheck? CountCheck { get; set; }

	public KnockBackCheck? KnockBackCheck { get; set; }

	public WatchCheck? WatchCheck { get; set; }

	public Func<Vector3>? PositionCustomAction { get; set; }

	public Func<Vector3>? TargetPositionCustomAction { get; set; }

	public Func<Angle>? RotationCustomAction { get; set; }
}
