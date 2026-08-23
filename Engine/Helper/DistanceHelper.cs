using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace Replica.Engine.Helper;

public static class DistanceHelper
{
	public static float DistanceToTarget(this IGameObject obj, IGameObject target)
	{
		return Vector3.Distance(target.Position, obj.Position);
	}

	public static float DistanceToPos(this IGameObject obj, Vector3 target)
	{
		return Vector3.Distance(target, obj.Position);
	}

	public static float DistanceSquaredToTarget(this IGameObject obj, IGameObject target)
	{
		return Vector3.DistanceSquared(target.Position, obj.Position);
	}

	public static float DistanceSquaredToPos(this IGameObject obj, Vector3 target)
	{
		return Vector3.DistanceSquared(target, obj.Position);
	}

	public static IEnumerable<IGameObject> SortedByRange(this IGameObject target)
	{
		List<(IGameObject obj, float distanceSq)> players = new List<(IGameObject, float)>();
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			if (allPlayer != target && allPlayer != null && !allPlayer.IsDead)
			{
				float item = (allPlayer.Position - target.Position).LengthSq();
				players.Add((allPlayer, item));
			}
		}
		players.Sort(((IGameObject obj, float distanceSq) a, (IGameObject obj, float distanceSq) b) => a.distanceSq.CompareTo(b.distanceSq));
		for (int i = 0; i < players.Count; i++)
		{
			yield return players[i].obj;
		}
	}

	public static IEnumerable<IGameObject> SortedByRange(this Vector3 target)
	{
		List<(IGameObject obj, float distanceSq)> players = new List<(IGameObject, float)>();
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			if (allPlayer != null && !allPlayer.IsDead)
			{
				float item = (allPlayer.Position - target).LengthSq();
				players.Add((allPlayer, item));
			}
		}
		players.Sort(((IGameObject obj, float distanceSq) a, (IGameObject obj, float distanceSq) b) => a.distanceSq.CompareTo(b.distanceSq));
		for (int i = 0; i < players.Count; i++)
		{
			yield return players[i].obj;
		}
	}

	public static float LengthSq(this Vector3 pos)
	{
		return pos.X * pos.X + pos.Z * pos.Z;
	}

	public static Vector2 OrthoL(this Vector2 pos)
	{
		return new Vector2(pos.Y, 0f - pos.X);
	}

	public static Vector2 OrthoR(this Vector2 pos)
	{
		return new Vector2(0f - pos.Y, pos.X);
	}
}
