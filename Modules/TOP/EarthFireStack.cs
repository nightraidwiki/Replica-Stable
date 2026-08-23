using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class EarthFireStack : ISpecialAction
{
	public override string Name => "Earth Fire (stack)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3426)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bpxf",
				radiusX = 6f,
				radiusZ = 6f,
				drawOnObject = true,
				delayDrawTime = (info.Time - 5f) * 1000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31572u }
				}
			}, info.TargetID.GameObject());
		}
	}
}
