using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P3N;

public class BranchOfAtis : ISpecialAction
{
	public override string Name => "Branch of Atis";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30714u, 30717u, 30719u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 30714:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				radiusX = 19f,
				radiusZ = 19f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			}, info.SourceId.GameObject());
			break;
		case 30719:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				radiusX = 25f,
				radiusZ = 25f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			}, info.SourceId.GameObject());
			break;
		case 30717:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02wf",
				radiusX = 12.5f,
				radiusZ = 50f,
				refOffsetZ = 10f,
				refRotation = info.Facing,
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			}, info.SourceId.GameObject());
			break;
		}
	}
}
