using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP;

public class LoopProgram : ISpecialAction
{
	public override string Name => "Loop Program";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31491u };

	public override uint WeatherID => 77u;

	public override void OnAbilityCast(ActorAbilityInfo info)
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
					ActionID = new HashSet<uint> { 31498u },
					TargetHitCount = 8
				}
			}, allPlayer);
		}
	}
}
