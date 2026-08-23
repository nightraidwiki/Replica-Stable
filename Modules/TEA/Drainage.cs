using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TEA;

public class Drainage : ISpecialAction
{
	public override string Name => "Drainage";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id != 3 || base.NumCasts > 0)
		{
			return;
		}
		base.NumCasts++;
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 6f,
				radiusZ = 6f,
				drawOnObject = true,
				TetherCheck = new TetherCheck
				{
					CheckType = 1,
					TetherID = new HashSet<int> { 3 }
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 18471u }
				}
			}, allPlayer);
		}
	}
}
