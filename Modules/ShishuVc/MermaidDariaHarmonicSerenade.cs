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

namespace Replica.Modules.ShishuVc;

public class MermaidDariaHarmonicSerenade : ISpecialAction
{
	private List<(uint BaseId, DrawElement Element)> timeline = new List<(uint, DrawElement)>();

	private List<(uint BaseId, DrawElement Element)> timelineHistory = new List<(uint, DrawElement)>();

	private int index;

	private uint lastAbilityId;

	public override string Name => "Mermaid Daria Harmonic Serenade";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45771u, 45773u, 45844u, 45839u, 45840u, 45841u, 45842u, 45843u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 45771)
		{
			Reset();
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		uint actionId = info.ActionId;
		if ((actionId != 45773 && actionId - 45839 > 5) || info.ActionId == lastAbilityId)
		{
			return;
		}
		lastAbilityId = info.ActionId;
		switch (info.ActionId)
		{
		case 45773u:
			timelineHistory = timeline.ToList();
			break;
		case 45844u:
			timeline = timelineHistory.ToList();
			break;
		}
		Plugin.DebugLog($"timeline:{timeline.Count}, index:{index}, timelineHistory:{timelineHistory.Count}");
		if (index >= timeline.Count)
		{
			return;
		}
		(uint, DrawElement) tuple = timeline[index++];
		foreach (IGameObject @object in Svc.Objects)
		{
			if (@object.BaseId == tuple.Item1)
			{
				DrawManager.Draw(tuple.Item2, @object);
			}
		}
	}

	public override void OnActorTargetVfx(uint actorId, uint targetVfxId)
	{
		IGameObject gameObject = actorId.GameObject();
		if (gameObject != null && gameObject.BaseId == 19097)
		{
			switch (targetVfxId)
			{
			case 2746u:
				timeline.Add((19102u, CreateFan(45843u)));
				break;
			case 2744u:
				timeline.Add((19100u, CreateRect(45841u)));
				break;
			case 2743u:
				timeline.Add((19099u, CreateRect(45840u)));
				break;
			case 2741u:
				timeline.Add((19098u, CreateRect(45839u)));
				break;
			case 2745u:
				timeline.Add((19101u, CreateCircle(45842u)));
				break;
			case 2742u:
				break;
			}
		}
	}

	private static DrawElement CreateFan(uint actionId)
	{
		return new DrawElement
		{
			drawAvfx = "gl_fan060_1bf",
			radiusX = 45f,
			radiusZ = 45f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { actionId }
			}
		};
	}

	private static DrawElement CreateRect(uint actionId)
	{
		return new DrawElement
		{
			drawAvfx = "general02xf",
			radiusX = 4f,
			radiusZ = 40f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { actionId }
			}
		};
	}

	private static DrawElement CreateCircle(uint actionId)
	{
		return new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 20f,
			radiusZ = 20f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { actionId }
			}
		};
	}

	public override void Reset()
	{
		timeline.Clear();
		index = 0;
		lastAbilityId = 0u;
		base.Reset();
	}
}
