using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace Replica.Engine.Util;

public static class MapUtil
{
	public unsafe static nint GetMapEffectModule()
	{
		return *(nint*)((byte*)EventFramework.Instance() + 344);
	}
}
