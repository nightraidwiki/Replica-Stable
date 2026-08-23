using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P12S.P12S;

public class EkpyrosisExaflare : ISpecialAction
{
	public override string Name => "Ground AoE";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33567u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject();
		Vector3 position = gameObject.Position;
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bxf",
			Position = position,
			drawOnObject = false,
			radiusX = 6f,
			radiusZ = 6f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 33567u }
			}
		}, gameObject);
		for (int i = 0; i < 4; i++)
		{
			position = new Vector3((float)((double)position.X + Math.Sin(info.Facing.Rad) * 8.0), 0f, (float)((double)position.Z + Math.Cos(info.Facing.Rad) * 8.0));
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = position,
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 6f,
				destroyTime = 2100f,
				delayDrawTime = info.CastTime * 1000f + (float)(2100 * i)
			}, gameObject);
		}
	}
}
