using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class SwordQuiver : ISpecialAction
{
	public override string Name => "Sword Quiver";

	public override uint Phase => 2u;

	public override uint WeatherID => 78u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31540u, 31541u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan090_1bf",
				radiusX = 100f,
				radiusZ = 100f,
				drawOnObject = true,
				target = allPlayer,
				delayDrawTime = (info.CastTime - 1.5f) * 1000f,
				TetherCheck = new TetherCheck
				{
					CheckType = 1,
					TetherID = new HashSet<int> { 84 }
				},
				hitCounter = new HitCounter
				{
					ActionID = ActionID
				}
			}, info.SourceId.GameObject());
		}
	}
}
