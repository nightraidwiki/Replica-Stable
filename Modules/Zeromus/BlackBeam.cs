using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zeromus;

public class BlackBeam : ISpecialAction
{
	public override string Name => "Black Beam";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 35567u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02pxf",
			radiusX = 6f,
			radiusZ = 60f,
			drawOnObject = true,
			target = info.Target,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 35643u },
				TargetHitCount = 6
			}
		}, info.Source);
	}
}
