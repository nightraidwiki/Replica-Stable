using System;

namespace Replica.Engine.Util;

public static class AngleExtensions
{
	public static Angle Radians(this float radians)
	{
		return new Angle(radians);
	}

	public static Angle Degrees(this float degrees)
	{
		return new Angle(degrees * ((float)Math.PI / 180f));
	}

	public static Angle Degrees(this int degrees)
	{
		return new Angle((float)degrees * ((float)Math.PI / 180f));
	}
}
