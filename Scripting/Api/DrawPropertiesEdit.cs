using System;
using System.Numerics;

namespace Replica.Scripting.Api;

public class DrawPropertiesEdit
{
	public string Name { get; set; } = "";

	public ulong Owner { get; set; }

	public PositionResolvePatternEnum CentreResolvePattern { get; set; }

	public uint CentreOrderIndex { get; set; } = 1u;

	public PositionResolvePatternEnum TargetResolvePattern { get; set; }

	public uint TargetOrderIndex { get; set; } = 1u;

	public ulong TargetObject { get; set; }

	public Vector3? Position { get; set; }

	public Vector3? TargetPosition { get; set; }

	public ulong FadeCentreObject { get; set; }

	public PositionResolvePatternEnum FadeCentreResolvePattern { get; set; }

	public uint FadeCentreOrderIndex { get; set; } = 1u;

	public Vector3? FadeCentrePosition { get; set; }

	public float FadeDistance { get; set; }

	public FadeMode FadeMode { get; set; }

	public Vector3? Offset { get; set; }

	public float Rotation { get; set; }

	public bool FixRotation { get; set; }

	public float Radian { get; set; } = (float)Math.PI / 2f;

	public Vector2 InnerScale { get; set; }

	public Vector2 Scale { get; set; } = new Vector2(5f, 5f);

	public ScaleMode ScaleMode { get; set; }

	public Vector4 Color { get; set; } = new Vector4(1f, 0.2f, 0.2f, 1f);

	public Vector4? TargetColor { get; set; }

	public bool Wave { get; set; }

	public long DestoryAt { get; set; } = 5000L;

	public long Delay { get; set; }

	public DrawPropertiesEdit Clone()
	{
		return (DrawPropertiesEdit)MemberwiseClone();
	}
}
