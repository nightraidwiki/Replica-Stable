using System;
using System.Runtime.InteropServices;

namespace Replica.Logging;

public enum MapAoeKind : byte
{
	Circle = 0,
	Donut = 1,
	Cone = 2,
	Rect = 3,
	Cross = 4,
	MovementArrow = 5,
	SafeSpot = 6
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct MapAoe
{
	public readonly MapAoeKind Kind;
	public readonly bool IsSafe;
	public readonly ushort Flags;
	public readonly float X;
	public readonly float Z;
	public readonly float Rot;       // Radians
	public readonly float Param1;    // Circle: Radius | Donut: OuterRadius | Cone: Radius | Rect: LengthFront | Cross: Length
	public readonly float Param2;    // Donut: InnerRadius | Cone: HalfAngle | Rect: LengthBack | Arrow: TargetX | Cross: HalfWidth
	public readonly float Param3;    // Cone: DirectionOffset | Rect: HalfWidth | Arrow: TargetZ
	public readonly uint Color;      // 0 = auto/default
	public readonly uint SourceId;   // Caster / Entity source ID
	public readonly uint ActionId;   // FFXIV Action ID (e.g. 39626)
	public readonly uint TargetId;   // Target Entity ID (e.g. for stack, spread, bait, tether)

	public MapAoe(
		MapAoeKind kind,
		bool isSafe,
		float x,
		float z,
		float rot = 0f,
		float param1 = 0f,
		float param2 = 0f,
		float param3 = 0f,
		uint color = 0,
		ushort flags = 0,
		uint sourceId = 0,
		uint actionId = 0,
		uint targetId = 0)
	{
		Kind = kind;
		IsSafe = isSafe;
		Flags = flags;
		X = x;
		Z = z;
		Rot = rot;
		Param1 = param1;
		Param2 = param2;
		Param3 = param3;
		Color = color;
		SourceId = sourceId;
		ActionId = actionId;
		TargetId = targetId;
	}

	public string GetShapeName()
	{
		return Kind switch
		{
			MapAoeKind.Circle => "Circle",
			MapAoeKind.Donut => Param3 > 0.01f && Param3 < MathF.PI - 0.01f ? "Donut Sector" : "Donut",
			MapAoeKind.Cone => "Cone / Fan",
			MapAoeKind.Rect => "Rectangle",
			MapAoeKind.Cross => "Cross",
			MapAoeKind.MovementArrow => "Movement / Arrow",
			MapAoeKind.SafeSpot => "Safe Spot",
			_ => Kind.ToString()
		};
	}

	public string GetShapeDescription()
	{
		return Kind switch
		{
			MapAoeKind.Circle => $"Radius: {Param1:F1}y",
			MapAoeKind.SafeSpot => $"Radius: {(Param1 > 0 ? Param1 : 2f):F1}y",
			MapAoeKind.Donut => Param3 > 0.01f && Param3 < MathF.PI - 0.01f
				? $"Outer: {Param1:F1}y, Inner: {Param2:F1}y, Angle: {Param3 * 2f * 180f / MathF.PI:F0}°"
				: $"Outer: {Param1:F1}y, Inner: {Param2:F1}y",
			MapAoeKind.Cone => $"Radius: {Param1:F1}y, Angle: {(Param2 > 0.01f ? Param2 * 2f * 180f / MathF.PI : 90f):F0}°",
			MapAoeKind.Rect => $"Length: {(Param1 + Param2):F1}y (Fwd {Param1:F1}y / Back {Param2:F1}y), Width: {(Param3 * 2f):F1}y",
			MapAoeKind.Cross => $"Length: {Param1:F1}y, Width: {(Param2 * 2f):F1}y",
			MapAoeKind.MovementArrow => $"Target: ({Param2:F1}, {Param3:F1})",
			_ => $"P1={Param1:F1}, P2={Param2:F1}, P3={Param3:F1}"
		};
	}

	/// <summary>
	/// Tests if a 2D world point (wx, wz) is inside this AOE shape.
	/// </summary>
	public bool ContainsPoint(float wx, float wz)
	{
		float dx = wx - X;
		float dz = wz - Z;
		float distSq = dx * dx + dz * dz;

		switch (Kind)
		{
			case MapAoeKind.Circle:
			{
				float r = MathF.Max(1.5f, Param1);
				return distSq <= r * r;
			}

			case MapAoeKind.SafeSpot:
			{
				float r = MathF.Max(2f, Param1 > 0 ? Param1 : 2.5f);
				return distSq <= r * r;
			}

			case MapAoeKind.Donut:
			{
				float rOuter = MathF.Max(2f, Param1);
				float rInner = MathF.Max(0f, Param2);
				if (distSq < rInner * rInner || distSq > rOuter * rOuter)
					return false;

				float ha = Param3;
				if (ha > 0.01f && ha < MathF.PI - 0.01f)
				{
					float angle = MathF.Atan2(dx, dz);
					float diff = MathF.Abs(NormalizeAngle(angle - Rot));
					if (diff > ha)
						return false;
				}
				return true;
			}

			case MapAoeKind.Cone:
			{
				float r = MathF.Max(1.5f, Param1);
				if (distSq > r * r)
					return false;

				float ha = Param2 > 0.01f ? Param2 : 0.785f;
				float angle = MathF.Atan2(dx, dz);
				float diff = MathF.Abs(NormalizeAngle(angle - Rot));
				return diff <= ha;
			}

			case MapAoeKind.Rect:
			{
				float lf = Param1;
				float lb = Param2;
				float hw = MathF.Max(1.0f, Param3);
				float rot = Rot;

				float fwdX = MathF.Sin(rot);
				float fwdZ = MathF.Cos(rot);
				float rightX = MathF.Cos(rot);
				float rightZ = -MathF.Sin(rot);

				float fwdDist = dx * fwdX + dz * fwdZ;
				float rightDist = dx * rightX + dz * rightZ;

				return fwdDist >= -lb && fwdDist <= lf && MathF.Abs(rightDist) <= hw;
			}

			case MapAoeKind.Cross:
			{
				float len = MathF.Max(1.5f, Param1);
				float hw = MathF.Max(1.0f, Param2);

				for (int arm = 0; arm < 2; arm++)
				{
					float armRot = Rot + (arm == 1 ? MathF.PI * 0.5f : 0f);
					float fwdX = MathF.Sin(armRot);
					float fwdZ = MathF.Cos(armRot);
					float rightX = MathF.Cos(armRot);
					float rightZ = -MathF.Sin(armRot);

					float fwdDist = dx * fwdX + dz * fwdZ;
					float rightDist = dx * rightX + dz * rightZ;

					if (fwdDist >= -len && fwdDist <= len && MathF.Abs(rightDist) <= hw)
						return true;
				}
				return false;
			}

			case MapAoeKind.MovementArrow:
			{
				float x1 = X, z1 = Z;
				float x2 = Param2, z2 = Param3;
				float segLenSq = (x2 - x1) * (x2 - x1) + (z2 - z1) * (z2 - z1);
				if (segLenSq <= 0.01f)
					return distSq <= 4f;

				float t = Math.Clamp(((wx - x1) * (x2 - x1) + (wz - z1) * (z2 - z1)) / segLenSq, 0f, 1f);
				float projX = x1 + t * (x2 - x1);
				float projZ = z1 + t * (z2 - z1);
				float dProjSq = (wx - projX) * (wx - projX) + (wz - projZ) * (wz - projZ);
				return dProjSq <= 4.0f; // 2 yards tolerance
			}

			default:
				return distSq <= 4f;
		}
	}

	private static float NormalizeAngle(float rad)
	{
		while (rad > MathF.PI) rad -= 2f * MathF.PI;
		while (rad < -MathF.PI) rad += 2f * MathF.PI;
		return rad;
	}
}
