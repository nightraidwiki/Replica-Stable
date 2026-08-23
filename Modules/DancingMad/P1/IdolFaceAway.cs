using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P1;

public class IdolFaceAway : ISpecialAction
{
	public override string Name => "Idol Face/Away";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint WeatherID => 78u;

	public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
	{
		if (p1 == 64 && p2 == 128)
		{
			Vector3 position = actorID.GameObject().Position;
			if (position == new Vector3(95f, 12.5f, 25f))
			{
				DrawManager.Draw(new DrawElement
				{
					drawType = ElementType.Channeling,
					drawAvfx = "chn_miro1v",
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47795u, 47796u }
					}
				}, Svc.Objects.LocalPlayer, actorID.GameObject());
			}
			if (position == new Vector3(105.25f, 13.5f, 34f))
			{
				DrawManager.Draw(new DrawElement
				{
					drawType = ElementType.Channeling,
					drawAvfx = "chn_miruna1v",
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47795u, 47796u }
					}
				}, Svc.Objects.LocalPlayer, actorID.GameObject());
			}
		}
	}
}
