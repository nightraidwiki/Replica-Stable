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

public class FutureSEndOrdainedPunishmentStack : ISpecialAction
{
	public override string Name => "Future's End α + Ordained Punishment (stack)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18598u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IGameObject target = Data.TetherPlayer.FirstOrDefault((TetherInfo tether) => info.Target.GameObjectId == tether.To).From.GameObject();
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bpxf",
			radiusX = 4f,
			radiusZ = 4f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18530u }
			}
		}, target);
	}
}
