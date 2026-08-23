using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class DarkElusiveJump : ISpecialAction
{
	public override string Name => "Dark Elusive Jump (tower)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2757 && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0119_trap_02t",
				radiusX = 5f,
				radiusY = 5f,
				radiusZ = 5f,
				refOffsetZ = 15f,
				drawOnObject = true,
				delayDrawTime = (int)(info.Time - 3f) * 1000,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 26384u },
					HitTarget = Svc.Objects.LocalPlayer
				}
			}, Svc.Objects.LocalPlayer);
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "m5fa_count5s_x",
				drawOnObject = true,
				delayDrawTime = (int)(info.Time - 5f) * 1000
			}, Svc.Objects.LocalPlayer);
		}
	}
}
