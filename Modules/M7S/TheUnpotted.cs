using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M7S;

public class TheUnpotted : ISpecialAction
{
	public override string Name => "The Unpotted";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42362u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan030_1bf",
				radiusX = 60f,
				radiusZ = 60f,
				drawOnObject = true,
				target = allPlayer,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42363u }
				}
			}, info.SourceId.GameObject());
		}
	}
}
