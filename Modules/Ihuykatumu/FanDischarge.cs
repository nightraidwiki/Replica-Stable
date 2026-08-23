using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Ihuykatumu;

public class FanDischarge : ISpecialAction
{
	public override string Name => "Fan Discharge";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36348u, 36351u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 36351)
		{
			base.NumCasts++;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customFan",
				refRadian = 45.Degrees().Rad,
				radiusX = 40f,
				radiusZ = 40f,
				drawOnObject = true,
				delayDrawTime = ((base.NumCasts > 8) ? 4000 : 0),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 36351u },
					TargetHitCount = base.NumCasts
				},
				refColor = Vector4.One,
				refTargetColor = Vector4.One
			}, info.SourceId.GameObject());
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 36348)
		{
			base.NumCasts = 0;
		}
	}
}
