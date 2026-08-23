using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class AeroKnockback : ISpecialAction
{
	public override string Name => "Aero (knockback)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2463 && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			DrawElement obj = new DrawElement
			{
				drawAvfx = "m0119_trap_02t",
				Position = new Vector3(88.2f, 0f, 115.2f),
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
			obj.Position = new Vector3(112.2f, 0f, 115.2f);
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
			SimpleElement.ShowText("Wind — wait bottom-left / bottom-right");
		}
	}
}
