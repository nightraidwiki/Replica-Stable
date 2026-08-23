using System;

namespace Replica.Engine.Interop;

internal static class InteropHelpers
{
	public static void Log(this Exception e)
	{
		try
		{
			Plugin.Log?.Error(e, "[Replica] " + e.Message);
		}
		catch
		{
		}
	}
}
