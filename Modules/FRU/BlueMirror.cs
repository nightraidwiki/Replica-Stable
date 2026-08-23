using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class BlueMirror : ISpecialAction
{
	private static readonly WPos[] Map = new WPos[8]
	{
		new WPos(100f, 80f),
		new WPos(114f, 86f),
		new WPos(120f, 100f),
		new WPos(114f, 114f),
		new WPos(100f, 120f),
		new WPos(86f, 114f),
		new WPos(80f, 100f),
		new WPos(86f, 86f)
	};

	private WPos pos;

	public override string Name => "Blue Mirror";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40179u, 40203u };

	public override void OnEnvControl(byte index, uint state)
	{
		if (base.CanDraw && state == 131073)
		{
			base.CanDraw = false;
			pos = Map[index - 1];
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 40179)
		{
			base.CanDraw = true;
		}
		if (info.ActionId != 40203 || !(pos != default(WPos)))
		{
			return;
		}
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17825 && new WPos(o.Position).AlmostEqual(pos, 1f));
		if (gameObject == null)
		{
			return;
		}
		SimpleElement.Donut(gameObject, 4f, 20f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 40203u, 40204u }
		});
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan030_1bf",
				drawOnObject = true,
				radiusX = 60f,
				radiusZ = 60f,
				target = allPlayer,
				delayDrawTime = 3000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40206u }
				},
				distanceCheck = new DistanceCheck
				{
					CheckObject = gameObject,
					CheckType = 0,
					Count = 4
				}
			}, gameObject);
		}
	}

	public override void Reset()
	{
		pos = default(WPos);
		base.Reset();
	}
}
