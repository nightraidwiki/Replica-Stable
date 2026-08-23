using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuVc;

public class MermaidDariaCrossingCurrent : ISpecialAction
{
	public override string Name => "Mermaid Daria Crossing Current";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon == 20 && TargetID != Svc.Objects.LocalPlayer?.GameObjectId)
		{
			DrawElement obj = new DrawElement
			{
				drawAvfx = "general_x02f",
				radiusX = 4f,
				radiusZ = 36f,
				drawOnObject = false,
				fixRotation = true,
				PositionCustomAction = delegate
				{
					Utils.GridSnapper gridSnapper = new Utils.GridSnapper
					{
						Center = new Vector3(375f, -29.5f, 530f),
						Size = 40f,
						GridCount = 5
					};
					return gridSnapper.Snap(Source.Position);
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 45860u }
				}
			};
			DrawManager.Draw(obj);
			obj.refRotation += 90.Degrees();
			DrawManager.Draw(obj);
		}
	}
}
