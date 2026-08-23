using System;
using System.Numerics;

namespace Replica.Engine.Util;

public readonly struct WDir(float x, float z)
{
	public readonly float X = x;

	public readonly float Z = z;

	public WDir(Vector2 v)
		: this(v.X, v.Y)
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

	public WPos ToWPos()
	{
		return new WPos(X, Z);
	}

	public static bool operator ==(WDir left, WDir right)
	{
		if (left.X == right.X)
		{
			return left.Z == right.Z;
		}
		return false;
	}

	public static bool operator !=(WDir left, WDir right)
	{
		return !(left == right);
	}

	public static WDir operator +(WDir a, WDir b)
	{
		return new WDir(a.X + b.X, a.Z + b.Z);
	}

	public static WDir operator -(WDir a, WDir b)
	{
		return new WDir(a.X - b.X, a.Z - b.Z);
	}

	public static WDir operator -(WDir a)
	{
		return new WDir(0f - a.X, 0f - a.Z);
	}

	public static WDir operator -(WDir a, WPos b)
	{
		return new WDir(a.X - b.X, a.Z - b.Z);
	}

	public static WDir operator *(WDir a, float b)
	{
		return new WDir(a.X * b, a.Z * b);
	}

	public static WDir operator *(float a, WDir b)
	{
		return new WDir(a * b.X, a * b.Z);
	}

	public static WDir operator /(WDir a, float b)
	{
		float num = 1f / b;
		return new WDir(a.X * num, a.Z * num);
	}

	public WDir Abs()
	{
		return new WDir(Math.Abs(X), Math.Abs(Z));
	}

	public WDir Sign()
	{
		return new WDir(Math.Sign(X), Math.Sign(Z));
	}

	public WDir OrthoL()
	{
		return new WDir(Z, 0f - X);
	}

	public WDir OrthoR()
	{
		return new WDir(0f - Z, X);
	}

	public WDir MirrorX()
	{
		return new WDir(0f - X, Z);
	}

	public WDir MirrorZ()
	{
		return new WDir(X, 0f - Z);
	}

	public float Dot(WDir a)
	{
		return X * a.X + Z * a.Z;
	}

	public float Cross(WDir b)
	{
		return X * b.Z - Z * b.X;
	}

	public WDir Rotate(WDir dir)
	{
		return new WDir(X * dir.Z + Z * dir.X, Z * dir.Z - X * dir.X);
	}

	public WDir Rotate(Angle dir)
	{
		return Rotate(dir.ToDirection());
	}

	public float LengthSq()
	{
		return X * X + Z * Z;
	}

	public float Length()
	{
		return MathF.Sqrt(LengthSq());
	}

	public WDir Normalized()
	{
		float num = MathF.Sqrt(X * X + Z * Z);
		if (!(num > 0f))
		{
			return default(WDir);
		}
		return this / num;
	}

	public bool AlmostEqual(WDir b, float eps)
	{
		if (Math.Abs(X - b.X) <= eps)
		{
			return Math.Abs(Z - b.Z) <= eps;
		}
		return false;
	}

	public WDir Scaled(float multiplier)
	{
		return new WDir(X * multiplier, Z * multiplier);
	}

	public WDir Rounded()
	{
		return new WDir(MathF.Round(X), MathF.Round(Z));
	}

	public WDir Rounded(float precision)
	{
		return Scaled(1f / precision).Rounded().Scaled(precision);
	}

	public WDir Floor()
	{
		return new WDir(MathF.Floor(X), MathF.Floor(Z));
	}

	public Angle ToAngle()
	{
		return new Angle(MathF.Atan2(X, Z));
	}

	public override string ToString()
	{
		return $"({X:f3}, {Z:f3})";
	}

	public bool Equals(WDir other)
	{
		return this == other;
	}

	public override bool Equals(object? obj)
	{
		if (obj is WDir other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (X, Z).GetHashCode();
	}

	public bool InRect(WDir direction, float lenFront, float lenBack, float halfWidth)
	{
		float num = Dot(direction);
		float value = Dot(direction.OrthoL());
		if (num >= 0f - lenBack && num <= lenFront)
		{
			return Math.Abs(value) <= halfWidth;
		}
		return false;
	}

	public bool InCross(WDir direction, float length, float halfWidth)
	{
		float num = Dot(direction);
		float num2 = Math.Abs(Dot(direction.OrthoL()));
		bool num3 = num >= 0f - length && num <= length && num2 <= halfWidth;
		bool flag = num >= 0f - halfWidth && num <= halfWidth && num2 <= length;
		return num3 | flag;
	}
}
