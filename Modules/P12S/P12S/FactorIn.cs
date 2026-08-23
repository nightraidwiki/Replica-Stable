using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.P12S.P12S;

public class FactorIn : ISpecialAction
{
	public override string Name => "Factor In";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id != 84)
		{
			return;
		}
		base.NumCasts++;
		if (base.NumCasts > 1)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 20f,
				radiusZ = 20f,
				drawOnObject = true,
				delayDrawTime = 5000f,
				TetherCheck = new TetherCheck
				{
					CheckType = 1,
					TetherID = new HashSet<int> { 84 }
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 33607u }
				}
			}, allPlayer);
		}
	}
}
