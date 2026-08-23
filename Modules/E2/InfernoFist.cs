using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.E2;

public class InfernoFist : ISpecialAction
{
	public override string Name => "Inferno Fist";

	public override HashSet<uint> ActionID => new HashSet<uint> { 19711u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 3f,
				drawOnObject = true,
				target = allPlayer,
				endToTarget = true,
				TetherCheck = new TetherCheck
				{
					CheckType = 0,
					TetherID = new HashSet<int> { 106, 105, 104 }
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 19434u, 19435u }
				}
			}, info.SourceId.GameObject());
		}
	}
}
