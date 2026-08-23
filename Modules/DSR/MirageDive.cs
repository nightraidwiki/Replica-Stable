using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class MirageDive : ISpecialAction
{
	public override string Name => "Mirage Dive";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 26820u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IGameObject target = info.Target;
		if (target != null)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "lockon3_t0h",
				drawOnObject = true,
				drawType = ElementType.LockOn
			}, target);
		}
	}
}
