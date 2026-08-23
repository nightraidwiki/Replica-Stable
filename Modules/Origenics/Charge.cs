using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Origenics;

public class Charge : ISpecialAction
{
	public override string Name => "Charge";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38953u, 38954u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 38953)
		{
			base.NumCasts++;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 5f,
				drawOnObject = true,
				targetPosition = new Vector3(info.Pos.X, info.Pos.Y, info.Pos.Z),
				endToTarget = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38954u, 36431u },
					TargetHitCount = base.NumCasts
				}
			}, info.SourceId.GameObject());
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 38954)
		{
			base.NumCasts = 0;
		}
	}
}
