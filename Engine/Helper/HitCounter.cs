using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;

namespace Replica.Engine.Helper;

public class HitCounter
{
	public HashSet<uint> ActionID { get; set; } = new HashSet<uint>();

	public int TargetHitCount { get; set; } = 1;

	public IGameObject? HitTarget { get; set; }

	public IGameObject? DestoryObject { get; set; }
}
