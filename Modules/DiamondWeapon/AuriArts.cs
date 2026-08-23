using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DiamondWeapon;

public class AuriArts : ISpecialAction
{
	private int tether154;

	public override string Name => "Auri Arts";

	public override HashSet<uint> ActionID => new HashSet<uint> { 24495u, 24568u };

	public override void OnActionCast(ActorCastInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (base.CanDraw)
		{
			if (Id == 157)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					radiusX = 5f,
					drawOnObject = true,
					target = targetId.GameObject(),
					endToTarget = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 24922u }
					}
				}, actorId.GameObject());
			}
			if (Id == 154)
			{
				tether154++;
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					radiusX = 5f,
					drawOnObject = true,
					target = targetId.GameObject(),
					endToTarget = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 24547u },
						TargetHitCount = tether154
					}
				}, actorId.GameObject());
			}
			if (Id == 156)
			{
				tether154 = 0;
				base.CanDraw = false;
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					radiusX = 5f,
					radiusZ = 50f,
					drawOnObject = true,
					delayDrawTime = 6000f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 24548u }
					}
				}, targetId.GameObject());
			}
		}
	}

	public override void Reset()
	{
		tether154 = 0;
		base.Reset();
	}
}
