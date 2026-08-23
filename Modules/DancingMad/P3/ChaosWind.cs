using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P3;

public class ChaosWind : ISpecialAction
{
	public override string Name => "Chaos Wind";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47862u };

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 1602 && info.StatusID != 1603)
		{
			return;
		}
		IGameObject localPlayer = Svc.Objects.LocalPlayer;
		IGameObject gameObject = info.TargetID.GameObject();
		if (localPlayer != null && gameObject != null && info.TargetID == localPlayer.GameObjectId)
		{
			if (info.StatusID == 1602)
			{
				DrawElement obj = new DrawElement
				{
					drawAvfx = "gl_fan090_1bpxf",
					radiusX = 15f,
					radiusZ = 15f,
					refRotation = 180.Degrees(),
					destroyTime = 10000f,
					delayDrawTime = (info.Time - 10f) * 1000f,
					StatusCheck = new StatusCheck
					{
						CheckObject = gameObject,
						Status = 1602u
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47891u }
					}
				};
				DrawManager.Draw(obj, localPlayer);
				obj.drawAvfx = "gl_fan270_0100af";
				obj.refRotation = 0.Degrees();
				DrawManager.Draw(obj, localPlayer);
			}
			else
			{
				DrawElement obj2 = new DrawElement
				{
					drawAvfx = "gl_fan090_1bpxf",
					radiusX = 15f,
					radiusZ = 15f,
					destroyTime = 10000f,
					delayDrawTime = (info.Time - 10f) * 1000f,
					StatusCheck = new StatusCheck
					{
						CheckObject = gameObject,
						Status = 1603u
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47891u }
					}
				};
				DrawManager.Draw(obj2, localPlayer);
				obj2.drawAvfx = "gl_fan270_0100af";
				obj2.refRotation = 180.Degrees();
				DrawManager.Draw(obj2, localPlayer);
			}
		}
	}
}
