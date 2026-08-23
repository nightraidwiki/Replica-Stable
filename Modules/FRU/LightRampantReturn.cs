using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class LightRampantReturn : ISpecialAction
{
	private readonly List<Vector3> posMap = new List<Vector3>();

	public override string Name => "Light Rampant (return)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40251u };

	public override void OnActionCast(ActorCastInfo info)
	{
		posMap.Add(info.SourceId.GameObject().Position);
		if (posMap.Count == 2)
		{
			Vector3 vector = (posMap[0] + posMap[1]) / 2f;
			DrawElement element = new DrawElement
			{
				drawAvfx = "m0119_trap_02t",
				Position = new Vector3(vector.X, 0f, vector.Y),
				drawOnObject = false,
				radiusX = 2f,
				radiusY = 5f,
				radiusZ = 2f,
				StatusCheck = new StatusCheck
				{
					CheckObject = Svc.Objects.LocalPlayer,
					Status = 4208u
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40332u }
				}
			};
			aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
			DrawElement element2 = new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				drawOnObject = true,
				targetPosition = new Vector3(vector.X, 0f, vector.Y),
				StatusCheck = new StatusCheck
				{
					CheckObject = Svc.Objects.LocalPlayer,
					Status = 4208u
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40332u }
				}
			};
			aoes.Add(DrawManager.Draw(element2, Svc.Objects.LocalPlayer));
		}
	}

	public override void Reset()
	{
		posMap.Clear();
		base.Reset();
	}
}
