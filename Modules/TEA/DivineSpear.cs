using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class DivineSpear : ISpecialAction
{
	public override string Name => "Divine Spear";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 19072u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "gl_fan090_1bf",
			radiusX = 24.2f,
			radiusZ = 24.2f,
			drawOnObject = true,
			alwaysFaceCurrentTarget = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 19074u },
				TargetHitCount = 3
			}
		}, info.Source);
	}
}
