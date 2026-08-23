using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class FutureSEndSpread : ISpecialAction
{
	public override string Name => "Future's End β (spread)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18592u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "loc06sp_05ak1",
				delayDrawTime = 28000f
			}, allPlayer);
		}
	}
}
