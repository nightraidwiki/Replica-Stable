namespace Replica.Engine.Helper;

public static class OmenHelper
{
	public static string LockOn(this string str)
	{
		return "vfx/lockon/eff/" + str + ".avfx";
	}

	public static string UnLockOn(this string str)
	{
		if (str.Length <= 20)
		{
			return string.Empty;
		}
		return str.Substring(15, str.Length - 5 - 15);
	}

	public static string Omen(this string str)
	{
		return "vfx/omen/eff/" + str + ".avfx";
	}

	public static string UnOmen(this string str)
	{
		if (str.Length <= 18)
		{
			return string.Empty;
		}
		return str.Substring(13, str.Length - 5 - 13);
	}

	public static string Channeling(this string str)
	{
		return "vfx/channeling/eff/" + str + ".avfx";
	}

	public static string UnChanneling(this string str)
	{
		if (str.Length <= 18)
		{
			return string.Empty;
		}
		return str.Substring(19, str.Length - 5 - 19);
	}
}
