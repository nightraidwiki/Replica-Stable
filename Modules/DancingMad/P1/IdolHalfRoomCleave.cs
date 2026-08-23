using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P1;

public class IdolHalfRoomCleave : ISpecialAction
{
	public override string Name => "Idol Half-Room Cleave";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint WeatherID => 77u;

	public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
	{
		if (p1 == 64 && p2 == 128)
		{
			Vector3 position = actorID.GameObject().Position;
			if (position == new Vector3(116f, 6.5f, 43f))
			{
				SimpleElement.Fan(new Vector3(100f, 0f, 100f), 100f, 180, 90.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 47794u }
				});
			}
			if (position == new Vector3(92f, 15f, 27f))
			{
				SimpleElement.Fan(new Vector3(100f, 0f, 100f), 100f, 180, -90.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 47793u }
				});
			}
		}
	}
}
