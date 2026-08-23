using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UnendingCoil;

public class StormSWingTethers : ISpecialAction
{
	public override string Name => "Storm's Wing (tethers)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id != 4 || base.NumCasts != 0)
		{
			return;
		}
		base.NumCasts++;
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 5f,
				radiusZ = 5f,
				drawOnObject = true,
				TetherCheck = new TetherCheck
				{
					CheckType = 0,
					TetherID = new HashSet<int> { 4 }
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 9944u }
				}
			}, allPlayer);
		}
	}
}
