using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP.P5;

public class P3TetherSpread : ISpecialAction
{
	public override string Name => "P3 Tether Spread";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 32374u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 15f,
				radiusZ = 15f,
				drawOnObject = true,
				TetherCheck = new TetherCheck
				{
					CheckType = 0,
					TetherID = new HashSet<int> { 89 }
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 32373u }
				}
			}, allPlayer);
		}
	}
}
