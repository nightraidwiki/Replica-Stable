using System.Numerics;
using Replica.Engine.Util;

namespace Replica.Engine.Bridge.BossMod.Overlay;

public readonly record struct OverlayArrow(Vector3 Start, Vector3 End, uint Color);

public readonly record struct OverlaySafeSpot(Vector3 Center, float Radius, uint Color);

public readonly record struct OverlayKnockback(Vector3 Start, Vector3 End, uint Color);

public readonly record struct OverlayTether(Vector3 Source, Vector3 Target, uint Color, float Thickness);

public readonly record struct OverlayGaze(Vector3 Position, Angle Direction, Angle HalfAngle, uint Color);

public readonly record struct OverlayReturnSpot(Vector3 Position, string Label, uint Color);
 
public enum OverlayBannerKind
{
	InfoBlue,
	DangerRed
}

public readonly record struct OverlayBannerHint(string Text, OverlayBannerKind Kind, bool IsRisk);

