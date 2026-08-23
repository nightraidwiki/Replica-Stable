using System;
using System.Globalization;

namespace Replica.Engine.Util;

public readonly struct Angle(float rad)
{
	public readonly float Rad = rad;

	public const float RadToDeg = 180f / (float)Math.PI;

	public const float DegToRad = (float)Math.PI / 180f;

	public const float HalfPi = (float)Math.PI / 2f;

	public const float DoublePI = (float)Math.PI * 2f;

	public static readonly Angle[] AnglesIntercardinals = new Angle[4]
	{
		(-45.003f).Degrees(),
		44.998f.Degrees(),
		134.999f.Degrees(),
		(-135.005f).Degrees()
	};

	public static readonly Angle[] AnglesCardinals = new Angle[4]
	{
		(-90.004f).Degrees(),
		(-0.003f).Degrees(),
		180f.Degrees(),
		89.999f.Degrees()
	};

	public float Deg => Rad * (180f / (float)Math.PI);

	public static Angle FromDirection(WDir dir)
	{
		return new Angle(MathF.Atan2(dir.X, dir.Z));
	}

	public WDir ToDirection()
	{
		var (num, num2) = Math.SinCos(Rad);
		return new WDir((float)num, (float)num2);
	}

	public static bool operator ==(Angle left, Angle right)
	{
		return left.Rad == right.Rad;
	}

	public static bool operator !=(Angle left, Angle right)
	{
		return left.Rad != right.Rad;
	}

	public static Angle operator +(Angle a, Angle b)
	{
		return new Angle(a.Rad + b.Rad);
	}

	public static Angle operator -(Angle a, Angle b)
	{
		return new Angle(a.Rad - b.Rad);
	}

	public static Angle operator -(Angle a)
	{
		return new Angle(0f - a.Rad);
	}

	public static Angle operator *(Angle a, float b)
	{
		return new Angle(a.Rad * b);
	}

	public static Angle operator *(float a, Angle b)
	{
		return new Angle(a * b.Rad);
	}

	public static Angle operator /(Angle a, float b)
	{
		return new Angle(a.Rad / b);
	}

	public static bool operator >(Angle a, Angle b)
	{
		return a.Rad > b.Rad;
	}

	public static bool operator <(Angle a, Angle b)
	{
		return a.Rad < b.Rad;
	}

	public static bool operator >=(Angle a, Angle b)
	{
		return a.Rad >= b.Rad;
	}

	public static bool operator <=(Angle a, Angle b)
	{
		return a.Rad <= b.Rad;
	}

	public Angle Abs()
	{
		return new Angle(Math.Abs(Rad));
	}

	public float Sin()
	{
		return (float)Math.Sin(Rad);
	}

	public float Cos()
	{
		return (float)Math.Cos(Rad);
	}

	public float Tan()
	{
		return (float)Math.Tan(Rad);
	}

	public static Angle Atan2(float y, float x)
	{
		return new Angle(MathF.Atan2(y, x));
	}

	public static Angle Asin(float x)
	{
		return new Angle((float)Math.Asin(x));
	}

	public static Angle Acos(float x)
	{
		return new Angle((float)Math.Acos(x));
	}

	public Angle Round(float roundToNearestDeg)
	{
		return (MathF.Round(Deg / roundToNearestDeg) * roundToNearestDeg).Degrees();
	}

	public Angle Normalized()
	{
		float num;
		for (num = Rad; num < -(float)Math.PI; num += (float)Math.PI * 2f)
		{
		}
		while (num > (float)Math.PI)
		{
			num -= (float)Math.PI * 2f;
		}
		return new Angle(num);
	}

	public bool AlmostEqual(Angle other, float epsRad)
	{
		return Math.Abs((this - other).Normalized().Rad) <= epsRad;
	}

	public Angle DistanceToAngle(Angle other)
	{
		return (other - this).Normalized();
	}

	public Angle DistanceToRange(Angle min, Angle max)
	{
		Angle angle = (max - min) * 0.5f;
		Angle angle2 = DistanceToAngle((min + max) * 0.5f);
		if (angle2.Rad > angle.Rad)
		{
			return angle2 - angle;
		}
		if (angle2.Rad < 0f - angle.Rad)
		{
			return angle2 + angle;
		}
		return default(Angle);
	}

	public Angle ClosestInRange(Angle min, Angle max)
	{
		Angle angle = (max - min) * 0.5f;
		Angle angle2 = DistanceToAngle((min + max) * 0.5f);
		if (angle2.Rad > angle.Rad)
		{
			return min;
		}
		if (angle2.Rad < 0f - angle.Rad)
		{
			return max;
		}
		return this;
	}

	public override string ToString()
	{
		return Deg.ToString("f3", CultureInfo.InvariantCulture);
	}

	public bool Equals(Angle other)
	{
		return Rad == other.Rad;
	}

	public override bool Equals(object? obj)
	{
		if (obj is Angle other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Rad.GetHashCode();
	}
}
