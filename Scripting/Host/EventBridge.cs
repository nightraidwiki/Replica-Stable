using System.Globalization;
using System.Numerics;
using Replica.Logging;
using Replica.Scripting.Api;

namespace Replica.Scripting.Host;

internal static class EventBridge
{
	public static Event? Translate(LogEvent e)
	{
		EventTypeEnum? eventTypeEnum = MapKind(e.Kind);
		if (!eventTypeEnum.HasValue)
		{
			return null;
		}
		Event obj = new Event
		{
			Type = eventTypeEnum.Value,
			Info = e.Name
		};
		Put(obj, "SourceId", Hex(e.SourceId));
		Put(obj, "SourceName", e.SourceName);
		Put(obj, "TargetId", Hex(e.TargetId));
		Put(obj, "TargetName", e.TargetName);
		Vector3 v = new Vector3(e.X, 0f, e.Y);
		Put(obj, "SourcePosition", Event.Format(v));
		Put(obj, "SourceRotation", Dec(e.Heading));
		switch (e.Kind)
		{
		case LogKind.CastStart:
			Put(obj, "ActionId", Dec(e.DataId));
			Put(obj, "ActionName", e.Name);
			Put(obj, "CastTime", Dec(e.Value));
			Put(obj, "CastTimeMs", Dec((uint)(e.Value * 1000f)));
			break;
		case LogKind.Ability:
		case LogKind.AbilityExtra:
			Put(obj, "ActionId", Dec(e.DataId));
			Put(obj, "ActionName", e.Name);
			Put(obj, "TargetIndex", Dec(e.Count));
			Put(obj, "EffectPosition", Event.Format(v));
			break;
		case LogKind.StatusGain:
		case LogKind.StatusLose:
			Put(obj, "StatusID", Dec(e.DataId));
			Put(obj, "StatusName", e.Name);
			Put(obj, "StatusParam", Dec(e.Count));
			Put(obj, "StatusStackCount", Dec(e.Count));
			Put(obj, "Duration", Dec(e.Value));
			break;
		case LogKind.Headmarker:
			Put(obj, "Id", Hex4(e.DataId));
			Put(obj, "IconId", Dec(e.DataId));
			Put(obj, "SourceDataId", Dec(e.Param1));
			break;
		case LogKind.Tether:
			Put(obj, "Id", Hex4(e.DataId));
			Put(obj, "TetherId", Dec(e.DataId));
			break;
		case LogKind.TimelineEvent:
			Put(obj, "Id", Dec(e.DataId));
			Put(obj, "SourceDataId", Dec(e.Param1));
			break;
		case LogKind.Added:
		case LogKind.EventObject:
			Put(obj, "DataId", Dec(e.DataId));
			Put(obj, "SourceDataId", Dec(e.DataId));
			Put(obj, "Operate", "Add");
			break;
		case LogKind.MapEffect:
			Put(obj, "Index", Dec(e.Param1));
			Put(obj, "State", Hex(e.Category));
			Put(obj, "Param", Dec(e.Category));
			break;
		case LogKind.Chat:
			Put(obj, "Message", e.Name);
			Put(obj, "Sender", e.SourceName);
			Put(obj, "LogKind", Dec(e.Category));
			break;
		case LogKind.Vfx:
		case LogKind.ActorTargetVfx:
			Put(obj, "Path", e.Name);
			Put(obj, "Id", Hex4(e.DataId));
			Put(obj, "OmenId", Dec(e.DataId));
			break;
		case LogKind.Death:
			Put(obj, "DataId", Dec(e.DataId));
			break;
		}
		return obj;
	}

	private static EventTypeEnum? MapKind(LogKind kind)
	{
		return kind switch
		{
			LogKind.CastStart => EventTypeEnum.StartCasting, 
			LogKind.Ability => EventTypeEnum.ActionEffect, 
			LogKind.AbilityExtra => EventTypeEnum.ActionEffect, 
			LogKind.StatusGain => EventTypeEnum.StatusAdd, 
			LogKind.StatusLose => EventTypeEnum.StatusRemove, 
			LogKind.Death => EventTypeEnum.Death, 
			LogKind.Headmarker => EventTypeEnum.TargetIcon, 
			LogKind.Tether => EventTypeEnum.Tether, 
			LogKind.Added => EventTypeEnum.AddCombatant, 
			LogKind.EventObject => EventTypeEnum.ObjectChanged, 
			LogKind.MapEffect => EventTypeEnum.EnvControl, 
			LogKind.Chat => EventTypeEnum.Chat, 
			LogKind.TimelineEvent => EventTypeEnum.PlayActionTimeline, 
			LogKind.Vfx => EventTypeEnum.VfxEvent, 
			LogKind.ActorTargetVfx => EventTypeEnum.ObjectVfx, 
			_ => null, 
		};
	}

	private static void Put(Event ev, string key, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			ev[key] = value;
		}
	}

	private static string Hex(uint v)
	{
		return v.ToString("X", CultureInfo.InvariantCulture);
	}

	private static string Hex4(uint v)
	{
		return v.ToString("X4", CultureInfo.InvariantCulture);
	}

	private static string Dec(uint v)
	{
		return v.ToString(CultureInfo.InvariantCulture);
	}

	private static string Dec(float v)
	{
		return v.ToString(CultureInfo.InvariantCulture);
	}
}
