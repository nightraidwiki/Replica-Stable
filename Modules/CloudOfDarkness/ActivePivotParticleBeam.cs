using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.CloudOfDarkness;

public class ActivePivotParticleBeam : ISpecialAction
{
	public override string Name => "Active Pivot Particle Beam";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40467u, 40469u, 40471u };

	public override uint Phase => 3u;

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

	public override void OnActionCast(ActorCastInfo info)
	{
		Angle angle = info.ActionId switch
		{
			40467 => -22.5f.Degrees(), 
			40469 => 22.5f.Degrees(), 
			_ => default(Angle), 
		};
		if (!(angle == default(Angle)))
		{
			for (int i = 0; i < 5; i++)
			{
				DrawElement element = new DrawElement
				{
					drawAvfx = "general02xf",
					Position = info.Pos,
					drawOnObject = false,
					radiusX = 9f,
					radiusZ = 80f,
					refOffsetZ = 40f,
					refRotation = info.Facing + i * angle,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 40471u },
						TargetHitCount = i + 1
					}
				};
				aoes.Add(DrawManager.Draw(element));
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 40471 && aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}
