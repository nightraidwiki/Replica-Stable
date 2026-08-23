using System;

namespace Replica.Logging;

public sealed record LogEvent
{
	public long Seq { get; init; }

	public DateTime Time { get; init; }

	public LogKind Kind { get; init; }

	public int Pull { get; init; }

	public string SourceName { get; init; } = "";

	public uint SourceId { get; init; }

	public ActorKind SourceKind { get; init; }

	public string TargetName { get; init; } = "";

	public uint TargetId { get; init; }

	public ActorKind TargetKind { get; init; }

	public string Name { get; init; } = "";

	public uint DataId { get; init; }

	public uint IconId { get; init; }

	public float Value { get; init; }

	public uint Count { get; init; }

	public float X { get; init; }

	public float Y { get; init; }

	public float Heading { get; init; }

	public uint Category { get; init; }

	public uint Param1 { get; init; }

	public uint Param2 { get; init; }

	public uint Param3 { get; init; }

	public uint Param4 { get; init; }

	public uint[] AbilityTargetIds { get; init; } = Array.Empty<uint>();

	public bool IsCast
	{
		get
		{
			LogKind kind = Kind;
			if (kind <= LogKind.CastFinish || kind == LogKind.Ability || kind == LogKind.AbilityExtra)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsStatus
	{
		get
		{
			LogKind kind = Kind;
			if (kind - 2 <= LogKind.CastFinish)
			{
				return true;
			}
			return false;
		}
	}
}
