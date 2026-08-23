using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;

namespace Replica.Scripting.Api;

public sealed class DataAccessory
{
	public ulong Me => Plugin.ObjectTable.LocalPlayer?.EntityId ?? Plugin.PlayerState.EntityId;

	public IPlayerCharacter? MyObject => Plugin.ObjectTable.LocalPlayer;

	public ulong[] PartyList
	{
		get
		{
			IPartyList partyList = Plugin.PartyList;
			if (partyList.Length == 0)
			{
				ulong me = Me;
				if (me != 0L)
				{
					return new ulong[1] { me };
				}
				return Array.Empty<ulong>();
			}
			return ((IEnumerable<IPartyMember>)partyList).Select((Func<IPartyMember, ulong>)((IPartyMember m) => m.EntityId)).ToArray();
		}
	}

	public ulong[] Objects => ((IEnumerable<IGameObject>)Plugin.ObjectTable).Select((Func<IGameObject, ulong>)((IGameObject o) => o.EntityId)).ToArray();

	public Vector3 DefaultDangerColor => new Vector3(1f, 0.2f, 0.2f);

	public Vector3 DefaultSafeColor => new Vector3(0.2f, 1f, 0.4f);

	public DrawPropertiesEdit GetDefaultDrawProperties()
	{
		return new DrawPropertiesEdit();
	}
}
