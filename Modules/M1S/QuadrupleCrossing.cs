using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class QuadrupleCrossing : ISpecialAction
{
	public override string Name => "Quadruple Crossing";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37948u, 37952u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 37948)
		{
			Draw(info.SourceId.GameObject());
			new TimeHelper(6800L, delegate
			{
				Draw(info.SourceId.GameObject());
			});
		}
		if (info.ActionId == 37952)
		{
			SimpleElement.Fan(info, 100f, 45);
		}
	}

	private static void Draw(IGameObject obj)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan045_1bf",
				drawOnObject = true,
				target = allPlayer,
				radiusX = 100f,
				radiusZ = 100f,
				distanceCheck = new DistanceCheck
				{
					CheckObject = obj,
					CheckType = 0,
					Count = 4
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 37951u }
				}
			}, obj);
		}
	}
}
