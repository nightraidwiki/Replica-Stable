using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P1N;

public class RubyReflection : ISpecialAction
{
	public override string Name => "Ruby Reflection";

	public override HashSet<uint> ActionID => new HashSet<uint> { 30426u, 30434u };

	public override uint Phase => 1u;

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 30426)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_trialaser_o0p",
				radiusX = 2f,
				radiusZ = 1f,
				drawOnObject = true,
				refRotation = info.Facing,
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			}, info.SourceId.GameObject());
		}
		if (info.ActionId == 30434)
		{
			SimpleElement.Rectangle(info, 7.5f, 7.5f, 7.5f);
		}
	}
}
