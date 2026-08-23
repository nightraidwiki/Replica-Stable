using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.SpheneDarkEx;

public class Stack2Stack4 : ISpecialAction
{
	public override string Name => "Stack (2) / Stack (4)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 44557u, 44558u, 45167u, 45168u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if (actionId == 44557 || actionId == 45167)
		{
			foreach (IGameObject item in PlayerHelper.Healer)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan020_0pt",
					Position = info.Pos,
					drawOnObject = false,
					radiusX = 100f,
					radiusZ = 100f,
					target = item,
					delayDrawTime = ((info.ActionId == 45167) ? 15000 : 0),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 44559u }
					}
				});
			}
		}
		else
		{
			foreach (IGameObject item2 in PlayerHelper.Healer.Union(PlayerHelper.Tank))
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan020_0pt",
					Position = info.Pos,
					drawOnObject = false,
					radiusX = 100f,
					radiusZ = 100f,
					target = item2,
					delayDrawTime = ((info.ActionId == 45168) ? 15000 : 0),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 44560u }
					}
				});
			}
		}
		actionId = info.ActionId;
		if ((uint)(actionId - 44557) <= 1u)
		{
			AzureSoul specialAction = ModuleUtil.GetSpecialAction<AzureSoul>();
			Vector3 position = info.SourceId.GameObject().Position;
			switch (specialAction.Hints[0])
			{
			case "Sides":
			{
				Vector3 pos = new Vector3(88f, 0f, 85f);
				HitCounter hitCounter2 = new HitCounter
				{
					ActionID = new HashSet<uint> { 44608u }
				};
				SimpleElement.Rectangle(pos, 100f, 6f, 0f, default(Angle), 3000f, 0f, hitCounter2);
				Vector3 pos2 = new Vector3(112f, 0f, 85f);
				hitCounter2 = new HitCounter
				{
					ActionID = new HashSet<uint> { 44608u }
				};
				SimpleElement.Rectangle(pos2, 100f, 6f, 0f, default(Angle), 3000f, 0f, hitCounter2);
				break;
			}
			case "Circle":
				SimpleElement.Circle(position, 20f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 45183u }
				});
				break;
			case "Donut":
				SimpleElement.Donut(position, 16f, 60f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 45184u }
				});
				break;
			case "Middle":
			{
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 45185u }
				};
				SimpleElement.Rectangle(position, 100f, 6f, 0f, default(Angle), 3000f, 0f, hitCounter);
				break;
			}
			}
			specialAction.Hints.Clear();
		}
	}
}
