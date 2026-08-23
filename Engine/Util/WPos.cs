using System;
using System.Numerics;

namespace Replica.Engine.Util;

public readonly struct WPos(float x, float z)
{
	public readonly float X = x;

	public readonly float Z = z;

	public WPos(Vector2 v)
		: this(v.X, v.Y)
	{
	}

	public WPos(Vector3 v)
		: this(v.X, v.Z)
	{
	}

	public Vector2 ToVec2()
	{
		return new Vector2(X, Z);
	}

	public Vector3 ToVec3(float y = 0f)
	{
		return new Vector3(X, y, Z);
	}

	public Vector4 ToVec4(float y = 0f, float w = 0f)
	{
		return new Vector4(X, y, Z, w);
	}

	public WDir ToWDir()
	{
		return new WDir(X, Z);
	}

	public static bool operator ==(WPos left, WPos right)
	{
		if (left.X == right.X)
		{
			return left.Z == right.Z;
		}
		return false;
	}

	public static bool operator !=(WPos left, WPos right)
	{
		return !(left == right);
	}

	public static WPos operator *(WPos a, float b)
	{
		return new WPos(a.X * b, a.Z * b);
	}

	public static WPos operator +(WPos a, float b)
	{
		return new WPos(a.X + b, a.Z + b);
	}

	public static WPos operator /(WPos a, int b)
	{
		float num = 1f / (float)b;
		return new WPos(a.X * num, a.Z * num);
	}

	public static WPos operator /(WPos a, float b)
	{
		float num = 1f / b;
		return new WPos(a.X * num, a.Z * num);
	}

	public static WPos operator +(WPos a, WDir b)
	{
		return new WPos(a.X + b.X, a.Z + b.Z);
	}

	public static WPos operator +(WDir a, WPos b)
	{
		return new WPos(a.X + b.X, a.Z + b.Z);
	}

	public static WPos operator -(WPos a, WDir b)
	{
		return new WPos(a.X - b.X, a.Z - b.Z);
	}

	public static WDir operator -(WPos a, WPos b)
	{
		return new WDir(a.X - b.X, a.Z - b.Z);
	}

	public bool AlmostEqual(WPos b, float eps)
	{
		if (Math.Abs(X - b.X) <= eps)
		{
			return Math.Abs(Z - b.Z) <= eps;
		}
		return false;
	}

	public WPos Scaled(float multiplier)
	{
		return new WPos(X * multiplier, Z * multiplier);
	}

	public WPos Rounded()
	{
		return new WPos(MathF.Round(X), MathF.Round(Z));
	}

	public WPos Rounded(float precision)
	{
		return Scaled(1f / precision).Rounded().Scaled(precision);
	}

	public static WPos Lerp(WPos from, WPos to, float progress)
	{
		return new WPos(from.ToVec2() * (1f - progress) + to.ToVec2() * progress);
	}

	public WPos Quantized()
	{
		return new WPos(((float)(int)MathF.Round(X * 32.7675f) - 0.5f) * 0.030518044f, ((float)(int)MathF.Round(Z * 32.7675f) - 0.5f) * 0.030518044f);
	}

	public static WPos RotateAroundOrigin(float rotateByDegrees, WPos origin, WPos point)
	{
		(double Sin, double Cos) tuple = Math.SinCos(rotateByDegrees * ((float)Math.PI / 180f));
		double item = tuple.Sin;
		double item2 = tuple.Cos;
		float num = (float)item;
		float num2 = (float)item2;
		float num3 = point.X - origin.X;
		float num4 = point.Z - origin.Z;
		float num5 = num2 * num3 - num * num4;
		float num6 = num * num3 + num2 * num4;
		return new WPos(origin.X + num5, origin.Z + num6);
	}

	public static WPos[] GenerateRotatedVertices(WPos center, WPos[] vertices, float rotationAngle)
	{
		WPos[] array = new WPos[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i] = RotateAroundOrigin(rotationAngle, center, vertices[i]);
		}
		return array;
	}

	public override string ToString()
	{
		return $"[{X:f3}, {Z:f3}]";
	}

	public bool Equals(WPos other)
	{
		return this == other;
	}

	public override bool Equals(object? obj)
	{
		if (obj is WPos other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (X, Z).GetHashCode();
	}

	public bool InTri(WPos v1, WPos v2, WPos v3)
	{
		float num = (v2.X - v1.X) * (Z - v1.Z) - (v2.Z - v1.Z) * (X - v1.X);
		float num2 = (v3.X - v2.X) * (Z - v2.Z) - (v3.Z - v2.Z) * (X - v2.X);
		if (num < 0f != num2 < 0f && num != 0f && num2 != 0f)
		{
			return false;
		}
		float num3 = (v1.X - v3.X) * (Z - v3.Z) - (v1.Z - v3.Z) * (X - v3.X);
		if (num3 != 0f)
		{
			return num3 < 0f == num + num2 <= 0f;
		}
		return true;
	}

	public bool InRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth)
	{
		return (this - origin).InRect(direction, lenFront, lenBack, halfWidth);
	}

	public bool InRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
	{
		return (this - origin).InRect(direction.ToDirection(), lenFront, lenBack, halfWidth);
	}

	public bool InRect(WPos origin, WDir startToEnd, float halfWidth)
	{
		float num = startToEnd.Length();
		return InRect(origin, startToEnd / num, num, 0f, halfWidth);
	}

	public bool InRect(WPos origin, WPos end, float halfWidth)
	{
		return InRect(origin, end - origin, halfWidth);
	}

	public bool InSquare(WPos origin, float halfWidth, Angle rotation)
	{
		return (this - origin).InRect(rotation.ToDirection(), halfWidth, halfWidth, halfWidth);
	}

	public bool InSquare(WPos origin, float halfWidth, WDir rotation)
	{
		return (this - origin).InRect(rotation, halfWidth, halfWidth, halfWidth);
	}

	public bool InSquare(WPos origin, float halfWidth)
	{
		if (Math.Abs(X - origin.X) <= halfWidth)
		{
			return Math.Abs(Z - origin.Z) <= halfWidth;
		}
		return false;
	}

	public bool InRect(WPos origin, float halfWidth, float halfHeight)
	{
		if (Math.Abs(X - origin.X) <= halfWidth)
		{
			return Math.Abs(Z - origin.Z) <= halfHeight;
		}
		return false;
	}

	public bool InCross(WPos origin, Angle direction, float length, float halfWidth)
	{
		return (this - origin).InCross(direction.ToDirection(), length, halfWidth);
	}

	public bool InCross(WPos origin, WDir direction, float length, float halfWidth)
	{
		return (this - origin).InCross(direction, length, halfWidth);
	}

	public bool InCircle(WPos origin, float radius)
	{
		return (this - origin).LengthSq() <= radius * radius;
	}

	public bool InDonut(WPos origin, float innerRadius, float outerRadius)
	{
		if (InCircle(origin, outerRadius))
		{
			return !InCircle(origin, innerRadius);
		}
		return false;
	}

	public bool InCone(WPos origin, WDir direction, Angle halfAngle)
	{
		return (this - origin).Normalized().Dot(direction) >= halfAngle.Cos();
	}

	public bool InCone(WPos origin, Angle direction, Angle halfAngle)
	{
		return InCone(origin, direction.ToDirection(), halfAngle);
	}

	public bool InCircleCone(WPos origin, float radius, WDir direction, Angle halfAngle)
	{
		if (InCircle(origin, radius))
		{
			return InCone(origin, direction, halfAngle);
		}
		return false;
	}

	public bool InCircleCone(WPos origin, float radius, Angle direction, Angle halfAngle)
	{
		if (InCircle(origin, radius))
		{
			return InCone(origin, direction, halfAngle);
		}
		return false;
	}

	public bool InDonutCone(WPos origin, float innerRadius, float outerRadius, WDir direction, Angle halfAngle)
	{
		if (InDonut(origin, innerRadius, outerRadius))
		{
			return InCone(origin, direction, halfAngle);
		}
		return false;
	}

	public bool InDonutCone(WPos origin, float innerRadius, float outerRadius, Angle direction, Angle halfAngle)
	{
		if (InDonut(origin, innerRadius, outerRadius))
		{
			return InCone(origin, direction, halfAngle);
		}
		return false;
	}

	public bool InCapsule(WPos origin, WDir direction, float radius, float length)
	{
		float num = Math.Clamp((this - origin).Dot(direction), 0f, length);
		WPos wPos = origin + num * direction;
		return (this - wPos).LengthSq() <= radius * radius;
	}

	public bool InArcCapsule(WPos start, WDir toOrbitCenter, Angle angularLength, float tubeRadius)
	{
		return InArcCapsule(start, start + toOrbitCenter, angularLength, tubeRadius);
	}

	public bool InArcCapsule(WPos start, WPos orbitCenter, Angle angularLength, float tubeRadius)
	{
		float num = tubeRadius * tubeRadius;
		float x = orbitCenter.X;
		float z = orbitCenter.Z;
		WDir dir = new WDir(start.X - x, start.Z - z);
		float num2 = dir.Length();
		WDir wDir = dir.Rotate(angularLength);
		WPos wPos = new WPos(x + wDir.X, z + wDir.Z);
		if ((this - start).LengthSq() <= num)
		{
			return true;
		}
		if ((this - wPos).LengthSq() <= num)
		{
			return true;
		}
		float num3 = X - x;
		float num4 = Z - z;
		float num5 = num3 * num3 + num4 * num4;
		float num6 = num2 - tubeRadius;
		float num7 = num2 + tubeRadius;
		if (num5 < num6 * num6 || num5 > num7 * num7)
		{
			return false;
		}
		Angle angle = angularLength.Abs() * 0.5f;
		float num8 = ((angle.Rad > (float)Math.PI / 2f) ? (-1f) : 1f);
		Angle angle2 = Angle.FromDirection(dir) + angularLength * 0.5f;
		Angle angle3 = 90f.Degrees();
		WDir wDir2 = num8 * (angle2 + angle + angle3).ToDirection();
		WDir wDir3 = num8 * (angle2 - angle - angle3).ToDirection();
		float num9 = num3 * wDir2.X + num4 * wDir2.Z;
		float num10 = num3 * wDir3.X + num4 * wDir3.Z;
		if (num8 > 0f)
		{
			if (num9 <= 0f)
			{
				return num10 <= 0f;
			}
			return false;
		}
		if (!(num9 >= 0f))
		{
			return num10 >= 0f;
		}
		return true;
	}
}
