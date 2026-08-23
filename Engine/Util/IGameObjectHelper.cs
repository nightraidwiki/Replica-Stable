using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Interop;

namespace Replica.Engine.Util;

public static class IGameObjectHelper
{
	public static IGameObject? GameObject(this uint ObjectID)
	{
		return Svc.Objects.SearchById(ObjectID);
	}

	public static IGameObject? GameObject(this ulong ObjectID)
	{
		return Svc.Objects.SearchById(ObjectID);
	}

	public static Actor? Find(ulong instanceID)
	{
		if (instanceID == 0L || instanceID == 3758096384u)
		{
			return null;
		}
		return Data.Actors.GetValueOrDefault(instanceID);
	}

	public static WDir DirectionTo(this IGameObject obj, WPos other)
	{
		return (other - new WPos(obj.Position)).Normalized();
	}
}
