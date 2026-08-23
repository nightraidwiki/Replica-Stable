using System;

namespace Replica.Engine.Util;

public static class Intersect
{
	public static bool CircleCone(WDir circleOffset, float circleRadius, float coneRadius, WDir coneDir, Angle halfAngle)
	{
		float num = circleOffset.LengthSq();
		float num2 = circleRadius * circleRadius;
		if (num <= num2)
		{
			return true;
		}
		float num3 = circleRadius + coneRadius;
		if (num > num3 * num3)
		{
			return false;
		}
		if (halfAngle.Rad >= (float)Math.PI)
		{
			return true;
		}
		bool flag = circleOffset.Dot(coneDir) > 0f;
		WDir wDir = coneDir.OrthoL();
		float num4 = halfAngle.Sin();
		float num5 = circleOffset.Dot(wDir);
		float num6 = halfAngle.Rad - (float)Math.PI / 2f;
		bool num7;
		if (!(num6 < 0f))
		{
			if (!(num6 > 0f))
			{
				num7 = flag;
			}
			else
			{
				if (flag)
				{
					goto IL_00b1;
				}
				num7 = num5 * num5 >= num * num4 * num4;
			}
		}
		else
		{
			if (!flag)
			{
				goto IL_00b3;
			}
			num7 = num5 * num5 <= num * num4 * num4;
		}
		if (num7)
		{
			goto IL_00b1;
		}
		goto IL_00b3;
		IL_00b3:
		if (num5 < 0f)
		{
			wDir = -wDir;
		}
		WDir wDir2 = coneDir * halfAngle.Cos() + wDir * num4;
		if (Math.Abs(circleOffset.Cross(wDir2)) > circleRadius)
		{
			return false;
		}
		float num8 = circleOffset.Dot(wDir2);
		if (num8 < 0f)
		{
			return false;
		}
		if (num8 <= coneRadius)
		{
			return true;
		}
		return (circleOffset - wDir2 * coneRadius).LengthSq() <= num2;
		IL_00b1:
		return true;
	}

	public static bool CircleCone(WPos circleCenter, float circleRadius, WPos coneCenter, float coneRadius, WDir coneDir, Angle halfAngle)
	{
		return CircleCone(circleCenter - coneCenter, circleRadius, coneRadius, coneDir, halfAngle);
	}
}
