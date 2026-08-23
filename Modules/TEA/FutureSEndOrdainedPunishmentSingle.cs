using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.Memory;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class FutureSEndOrdainedPunishmentSingle : ISpecialAction
{
	public override string Name => "Future's End α + Ordained Punishment (single)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18597u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IGameObject target = Data.TetherPlayer.FirstOrDefault((TetherInfo tether) => info.Target.GameObjectId == tether.To).From.GameObject();
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 1f,
			radiusZ = 1f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18529u }
			}
		}, target);
	}
}
