using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SpheneDarkEx;

public class AzureImpact : ISpecialAction
{
	public override string Name => "Azure Impact";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44592u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "er_gl_fan100_o0v",
			radiusX = 100f,
			radiusZ = 100f,
			alwaysFaceCurrentTarget = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 44593u },
				TargetHitCount = 2
			}
		}, info.Source);
	}
}
