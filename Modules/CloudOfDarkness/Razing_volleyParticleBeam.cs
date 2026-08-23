using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CloudOfDarkness;

public class Razing_volleyParticleBeam : ISpecialAction
{
	public override string Name => "Razing-volley Particle Beam";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40511u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			drawAvfx = "general02xf",
			radiusX = 4f,
			radiusZ = 45f,
			drawOnObject = true,
			destroyTime = 4000f,
			delayDrawTime = 4000f
		};
		aoes.Add(DrawManager.Draw(element, info.SourceId.GameObject()));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}
