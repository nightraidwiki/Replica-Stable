using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.QueenEternalEx;

public class IcicleTether : ISpecialAction
{
	public override string Name => "Icicle (tether)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if ((Id == 1 || Id == 57) && targetId == Svc.Objects.LocalPlayer.GameObjectId)
		{
			SimpleElement.RectangleToTarget(actorId.GameObject(), targetId.GameObject(), 80f, 2f, 3000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 41015u, 41016u }
			});
			Vector2 vector = SafeSpot(actorId.GameObject());
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "share_trap01k1",
				Position = new Vector3(vector.X, 0f, vector.Y),
				drawOnObject = false,
				radiusX = 2f,
				radiusY = 5f,
				radiusZ = 2f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41015u, 41016u }
				}
			}, Svc.Objects.LocalPlayer);
		}
	}

	private static Vector2 SafeSpot(IGameObject source)
	{
		Vector2 vector = new Vector2(100f, 100f);
		int num = ((!(source.Position.X > vector.X)) ? 1 : (-1));
		float num2 = Math.Abs(source.Position.X - vector.X);
		if (source.Position.Z > 110f)
		{
			bool flag = num2 < 6f;
			return vector + new Vector2(num * (flag ? 15 : 10), -19f);
		}
		int num3 = ((source.Position.Z < 96f) ? 9 : (-9));
		return vector + new Vector2(num * 15, num3);
	}
}
