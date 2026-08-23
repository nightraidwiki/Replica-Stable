using System.Numerics;
using Replica.QuickDraws;

namespace Replica.Strats;

public sealed class RoleSpot
{
	public StratRole Role { get; set; }

	public bool Enabled { get; set; } = true;

	public Vector3 Position { get; set; } = new Vector3(100f, 0f, 100f);

	public SpotAnchor Anchor { get; set; }

	public uint TetherId { get; set; }

	public QuickShape Shape { get; set; }

	public Vector4 Color { get; set; } = new Vector4(1f, 0.55f, 0.1f, 0.45f);

	public float Radius { get; set; } = 1.5f;

	public bool ShowLeash { get; set; } = true;

	public Vector4 LeashColor { get; set; } = new Vector4(0.3f, 0.85f, 1f, 0.6f);

	public float Duration { get; set; } = 8f;

	public RoleSpot Clone()
	{
		return (RoleSpot)MemberwiseClone();
	}
}
