using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class DragonHead : ISpecialAction
{
	public override string Name => "Dragon Head";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3263 && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId && info.Time == 17f)
		{
			DrawElement obj = new DrawElement
			{
				drawAvfx = "m0119_trap_02t",
				Position = new Vector3(113f, 0f, 100f),
				drawOnObject = false,
				radiusX = 1.5f,
				radiusY = 5f,
				radiusZ = 1.5f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40241u }
				}
			};
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
			obj.Position = new Vector3(87f, 0f, 100f);
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
			SimpleElement.ShowText("Short red — B/D bait 1st dragon head");
		}
	}
}
