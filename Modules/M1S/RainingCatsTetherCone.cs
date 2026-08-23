using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class RainingCatsTetherCone : ISpecialAction
{
	public override string Name => "Raining Cats (tether cone)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39611u, 39612u, 39613u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan045_1bf",
				radiusX = 100f,
				radiusZ = 100f,
				drawOnObject = true,
				target = allPlayer,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38045u }
				},
				TetherCheck = new TetherCheck
				{
					CheckType = 0,
					TetherID = new HashSet<int> { 89 }
				}
			}, info.SourceId.GameObject());
		}
	}
}
