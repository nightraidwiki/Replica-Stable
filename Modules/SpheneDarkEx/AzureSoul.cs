using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.SpheneDarkEx;

public class AzureSoul : ISpecialAction
{
	public List<string> Hints = new List<string>(4);

	public override string Name => "Azure Soul";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		switch (icon)
		{
		case 604u:
			Hints.Add("Circle");
			break;
		case 605u:
			Hints.Add("Donut");
			break;
		case 606u:
			Hints.Add("Sides");
			break;
		case 607u:
			Hints.Add("Middle");
			break;
		}
	}

	public override void OnActorModelStateChange(IGameObject obj, byte modelState, byte animState1, byte animState2)
	{
		if (Hints.Count != 4)
		{
			return;
		}
		int num = modelState switch
		{
			21 => 0, 
			147 => 1, 
			65 => 2, 
			22 => 3, 
			_ => -1, 
		};
		if (num == -1)
		{
			return;
		}
		Utils.RotateList(Hints, num);
		for (int i = 0; i < 4; i++)
		{
			Vector3 position = obj.Position;
			switch (Hints[i])
			{
			case "Sides":
			{
				Vector3 pos = new Vector3(88f, 0f, 85f);
				float castTime2 = ((i == 0) ? 12400 : 2800);
				float delay2 = ((i != 0) ? (12400 + (i - 1) * 2800) : 0);
				SimpleElement.Rectangle(pos, 100f, 6f, 0f, default(Angle), castTime2, delay2);
				Vector3 pos2 = new Vector3(112f, 0f, 85f);
				delay2 = ((i == 0) ? 12400 : 2800);
				castTime2 = ((i != 0) ? (12400 + (i - 1) * 2800) : 0);
				SimpleElement.Rectangle(pos2, 100f, 6f, 0f, default(Angle), delay2, castTime2);
				break;
			}
			case "Circle":
				SimpleElement.Circle(position, 20f, (i == 0) ? 12400 : 2800, (i != 0) ? (12400 + (i - 1) * 2800) : 0);
				break;
			case "Donut":
				SimpleElement.Donut(position, 16f, 60f, (i == 0) ? 12400 : 2800, (i != 0) ? (12400 + (i - 1) * 2800) : 0);
				break;
			case "Middle":
			{
				float castTime = ((i == 0) ? 12400 : 2800);
				float delay = ((i != 0) ? (12400 + (i - 1) * 2800) : 0);
				SimpleElement.Rectangle(position, 100f, 6f, 0f, default(Angle), castTime, delay);
				break;
			}
			}
		}
		Hints.Clear();
	}

	public override void Reset()
	{
		Hints.Clear();
		base.Reset();
	}
}
