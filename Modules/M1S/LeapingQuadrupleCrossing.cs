using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M1S;

public class LeapingQuadrupleCrossing : ISpecialAction
{
	public override string Name => "Leaping Quadruple Crossing";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38959u, 37975u, 37980u, 38014u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if (actionId == 37975 || actionId == 38959)
		{
			base.CanDraw = true;
		}
		actionId = info.ActionId;
		if (actionId == 37980 || actionId == 38014)
		{
			SimpleElement.Fan(info, 100f, 45);
		}
	}

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 17195 && base.CanDraw)
		{
			base.CanDraw = false;
			Vector3 pos = GameObject.Position;
			Draw(pos);
			new TimeHelper(7300L, delegate
			{
				Draw(pos);
			});
		}
	}

	private static void Draw(Vector3 Pos)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan045_1bf",
				Position = Pos,
				drawOnObject = false,
				radiusX = 100f,
				radiusZ = 100f,
				target = allPlayer,
				distanceCheck = new DistanceCheck
				{
					Position = Pos,
					CheckType = 4,
					Count = 4
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 37979u }
				}
			}, Svc.Objects.LocalPlayer);
		}
	}
}
