using System.Collections.Generic;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Struct.Vfx;
using Replica.Engine.Vfx;

namespace Replica.Engine.Managers;

public static class FrameworkUpdateManager
{
	public static List<TimeHelper> TimeHelpers = new List<TimeHelper>();

	public static List<StaticVfx> StaticVfxs = new List<StaticVfx>();

	public static List<ActorVfx> ActorVfxs = new List<ActorVfx>();

	public static void Tick()
	{
		if (Svc.Objects.LocalPlayer != null)
		{
			FightClientState.PollEnmity();
			VFXList.SyncVfxHandles();
			TimeHelper[] array = TimeHelpers.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Update();
			}
			ActorVfx[] array2 = ActorVfxs.ToArray();
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].Update();
			}
			StaticVfx[] array3 = StaticVfxs.ToArray();
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k].Update();
			}
		}
	}
}
