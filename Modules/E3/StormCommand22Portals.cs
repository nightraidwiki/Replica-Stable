using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.E3;

public class StormCommand22Portals : ISpecialAction
{
	public static int DoorIndex;

	public Dictionary<int, (string first, string second)> DoorMap = new Dictionary<int, (string, string)>
	{
		{
			2,
			("Red", "Blue")
		},
		{
			7,
			("Blue", "Red")
		}
	};

	public override string Name => "Storm Command (2+2 portals)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 20067u, 20068u, 19520u };

	public override void OnEnvControl(byte index, uint state)
	{
		if (state == 131073 && (index == 2 || index == 7))
		{
			DoorIndex = index;
		}
		if (state == 524292)
		{
			DoorIndex = 0;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId - 20067 <= 1 && DoorMap.TryGetValue(DoorIndex, out (string, string) value))
		{
			DoorIndex = 0;
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "general02xf",
				drawOnObject = false,
				radiusX = 5f,
				radiusZ = 100f,
				refRotation = 90.Degrees(),
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 19520u },
					TargetHitCount = 2
				}
			};
			if (info.Source.Position.X < 100f)
			{
				drawElement.Position = new Vector3(80f, 0f, (value.Item1 == "Red") ? 105 : 115);
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
				drawElement.Position = new Vector3(80f, 0f, (value.Item1 == "Red") ? 115 : 105);
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
			}
			else
			{
				drawElement.Position = new Vector3(80f, 0f, (value.Item1 == "Blue") ? 105 : 115);
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
				drawElement.Position = new Vector3(80f, 0f, (value.Item1 == "Blue") ? 115 : 105);
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
			}
		}
		if (info.ActionId == 19520 && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}
