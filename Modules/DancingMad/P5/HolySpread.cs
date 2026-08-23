using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P5;

public class HolySpread : ISpecialAction
{
	public override string Name => "Holy Spread";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 5351)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 5f,
				radiusZ = 5f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47958u }
				}
			}, info.TargetID.GameObject());
		}
	}
}
