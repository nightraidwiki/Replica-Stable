using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.WorqorLarDor;

public class IceRing : ISpecialAction
{
	public override string Name => "Ice Ring";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36272u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customCircle",
			radiusX = 15f,
			radiusZ = 15f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 36272u }
			},
			StatusCheck = new StatusCheck
			{
				CheckObject = info.SourceId.GameObject(),
				Status = 3445u,
				Reverse = true
			}
		}, info.SourceId.GameObject());
	}
}
