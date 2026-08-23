using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuVc;

public class MermaidDariaCharmingCommand : ISpecialAction
{
	public override string Name => "Mermaid Daria Charming Command";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID - 2161 <= 3 && info.TargetID == Svc.Objects.LocalPlayer?.GameObjectId)
		{
			Angle refRotation = info.StatusID switch
			{
				2161u => 0.Degrees(), 
				2162u => 180.Degrees(), 
				2163u => 90.Degrees(), 
				2164u => -90.Degrees(), 
				_ => 0.Degrees(), 
			};
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				radiusZ = 20f,
				drawOnObject = true,
				refRotation = refRotation,
				destroyTime = info.Time * 1000f,
				StatusCheck = new StatusCheck
				{
					CheckObject = Svc.Objects.LocalPlayer,
					Status = info.StatusID
				}
			}, Svc.Objects.LocalPlayer);
		}
	}
}
