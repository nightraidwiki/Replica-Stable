using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.DSR;

public class SpiralPierce : ISpecialAction
{
	public override string Name => "Spiral Pierce";

	public override uint Phase => 5u;

	public override uint WeatherID => 46u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 0u };

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 5)
		{
			IGameObject gameObject = actorId.GameObject();
			IGameObject gameObject2 = targetId.GameObject();
			if (gameObject != null && gameObject2 != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					radiusX = 8f,
					radiusZ = 60f,
					drawOnObject = true,
					endToTarget = true,
					target = gameObject2,
					TetherCheck = new TetherCheck
					{
						CheckType = 1,
						TetherID = new HashSet<int> { 5 }
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 27530u }
					}
				}, gameObject);
			}
		}
	}
}
