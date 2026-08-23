using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P5;

public class MeteorStack : ISpecialAction
{
	public override string Name => "Meteor Stack";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 5350)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 25f,
				radiusZ = 25f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47957u }
				}
			}, info.TargetID.GameObject());
		}
	}
}
