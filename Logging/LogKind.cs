namespace Replica.Logging;

public enum LogKind : byte
{
	CastStart,
	CastFinish,
	StatusGain,
	StatusLose,
	Death,
	Ability,
	Headmarker,
	Tether,
	TetherCancel,
	Added,
	ActorControl,
	AbilityExtra,
	MapEffect,
	Note,
	Chat,
	TimelineEvent,
	TimelineSync,
	EventObject,
	Vfx,
	ActorTargetVfx
}
