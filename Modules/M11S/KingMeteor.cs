using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M11S;

public class KingMeteor : ISpecialAction
{
	private readonly List<ulong> actors = new List<ulong>();

	public override string Name => "King Meteor";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46144u, 46147u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 46144u:
			base.CanDraw = true;
			break;
		case 46147u:
			actors.Clear();
			break;
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if ((Id == 57 || Id == 249) && base.CanDraw && !actors.Contains(targetId))
		{
			actors.Add(targetId);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 5f,
				radiusZ = 60f,
				drawOnObject = true,
				target = targetId.GameObject(),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46147u }
				}
			}, actorId.GameObject());
		}
	}

	public override void Reset()
	{
		actors.Clear();
		base.Reset();
	}
}
