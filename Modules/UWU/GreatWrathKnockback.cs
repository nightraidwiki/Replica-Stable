using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.UWU;

public class GreatWrathKnockback : ISpecialAction
{
	public override string Name => "Great Wrath (knockback)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11111u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 24f,
			drawOnObject = true,
			destroyTime = 4000f,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = info.SourceId.GameObject().Position,
				Antiable = false
			}
		}, Svc.Objects.LocalPlayer);
	}
}
