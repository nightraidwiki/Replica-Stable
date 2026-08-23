using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Origenics;

public class TelekinesisRepel : ISpecialAction
{
	public override string Name => "Telekinesis Repel";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36433u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "movepos_mark_01t",
			radiusX = 30f,
			radiusY = 5f,
			radiusZ = 30f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 36433u }
			}
		}, info.SourceId.GameObject());
	}
}
