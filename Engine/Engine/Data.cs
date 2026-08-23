using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Memory;

namespace Replica.Engine;

public static class Data
{
	public static HashSet<string> lockonList = new HashSet<string>();

	public static HashSet<string> omenList = new HashSet<string> { "customFan", "customCircle", "customDonut", "customRect", "customRect2", "share2_6m", "eye_warn", "tank_lockon_3m_5s_noc", "tank_lockon_5m_5s_noc", "ShareLazerGround5s" };

	public static HashSet<string> channelingList = new HashSet<string>();

	public static Dictionary<ulong, Vector3> LastCastPositions = new Dictionary<ulong, Vector3>();

	public static readonly Actor?[] ActorsByIndex = new Actor[819];

	public static readonly Dictionary<ulong, Actor> Actors = new Dictionary<ulong, Actor>();

	public static List<TimeHelper> DelayTasks { get; set; } = new List<TimeHelper>();

	public static List<TetherInfo> TetherPlayer { get; set; } = new List<TetherInfo>();

	public static void Clear()
	{
		TetherPlayer.Clear();
	}
}
