using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingGreen;

public class LetsDance : ISpecialAction
{
	public override string Name => "Let's Dance";

	public override HashSet<uint> ActionID => new HashSet<uint> { 39900u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public unsafe override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 334)
		{
			Character* address = (Character*)actorId.GameObject().Address;
			Angle angle = ((address->Timeline.ModelState != 5) ? (-90.Degrees()) : 90.Degrees());
			Angle rotation = angle;
			aoes.Add(SimpleElement.Rectangle(new Vector3(100f, 0f, 100f), 20f, 20f, 0f, rotation, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 39900u },
				TargetHitCount = 8
			}));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}
