using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class RedMirror : ISpecialAction
{
	public override string Name => "Red Mirror";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40205u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Donut(info, 4f, 20f);
		new TimeHelper(5000L, delegate
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan030_1bf",
					drawOnObject = true,
					radiusX = 60f,
					radiusZ = 60f,
					target = allPlayer,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 40206u }
					},
					distanceCheck = new DistanceCheck
					{
						CheckObject = info.SourceId.GameObject(),
						CheckType = 0,
						Count = 4
					}
				}, info.SourceId.GameObject());
			}
		});
	}
}
