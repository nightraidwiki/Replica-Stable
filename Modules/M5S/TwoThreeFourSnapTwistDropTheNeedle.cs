using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M5S;

public class TwoThreeFourSnapTwistDropTheNeedle : ISpecialAction
{
	private static readonly HashSet<uint> TwoSnapTwistDropTheNeedleFirst = new HashSet<uint> { 42792u, 42793u, 42794u, 42795u, 42796u, 42797u, 42203u, 42204u };

	private static readonly HashSet<uint> TwoSnapTwistDropTheNeedleRest = new HashSet<uint> { 42798u, 42799u };

	private static readonly HashSet<uint> ThreeSnapTwistDropTheNeedleFirst = new HashSet<uint> { 42205u, 42206u, 42800u, 42801u, 42802u, 42803u, 42804u, 42805u };

	private static readonly HashSet<uint> ThreeSnapTwistDropTheNeedleRest = new HashSet<uint> { 42807u, 42808u };

	private static readonly HashSet<uint> FourSnapTwistDropTheNeedleFirst = new HashSet<uint> { 42207u, 42208u, 42809u, 42810u, 42811u, 42812u, 42813u, 42814u };

	private static readonly HashSet<uint> FourSnapTwistDropTheNeedleRest = new HashSet<uint> { 42817u, 42818u };

	public override string Name => "2/3/4-snap Twist & Drop the Needle";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (uint item in TwoSnapTwistDropTheNeedleFirst)
			{
				hashSet.Add(item);
			}
			foreach (uint item2 in TwoSnapTwistDropTheNeedleRest)
			{
				hashSet.Add(item2);
			}
			foreach (uint item3 in ThreeSnapTwistDropTheNeedleFirst)
			{
				hashSet.Add(item3);
			}
			foreach (uint item4 in ThreeSnapTwistDropTheNeedleRest)
			{
				hashSet.Add(item4);
			}
			foreach (uint item5 in FourSnapTwistDropTheNeedleFirst)
			{
				hashSet.Add(item5);
			}
			foreach (uint item6 in FourSnapTwistDropTheNeedleRest)
			{
				hashSet.Add(item6);
			}
			return hashSet;
		}
	}

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (!TwoSnapTwistDropTheNeedleFirst.Contains(info.ActionId) && !ThreeSnapTwistDropTheNeedleFirst.Contains(info.ActionId) && !FourSnapTwistDropTheNeedleFirst.Contains(info.ActionId))
		{
			return;
		}
		List<StaticVfx> list = aoes;
		IGameObject gameObject = info.SourceId.GameObject();
		Angle facing = info.Facing;
		HitCounter hitCounter = new HitCounter();
		HitCounter hitCounter2 = hitCounter;
		HashSet<uint> hashSet = new HashSet<uint>();
		foreach (uint item in TwoSnapTwistDropTheNeedleRest)
		{
			hashSet.Add(item);
		}
		foreach (uint item2 in ThreeSnapTwistDropTheNeedleRest)
		{
			hashSet.Add(item2);
		}
		foreach (uint item3 in FourSnapTwistDropTheNeedleRest)
		{
			hashSet.Add(item3);
		}
		hitCounter2.ActionID = hashSet;
		hitCounter.TargetHitCount = 2;
		HitCounter hitCounter3 = hitCounter;
		list.Add(SimpleElement.Rectangle(gameObject, 20f, 20f, 0f, null, facing, 3000f, 0f, hitCounter3));
		list = aoes;
		gameObject = info.SourceId.GameObject();
		facing = info.Facing + 180.Degrees();
		hitCounter3 = new HitCounter();
		hitCounter2 = hitCounter3;
		hashSet = new HashSet<uint>();
		foreach (uint item4 in TwoSnapTwistDropTheNeedleRest)
		{
			hashSet.Add(item4);
		}
		foreach (uint item5 in ThreeSnapTwistDropTheNeedleRest)
		{
			hashSet.Add(item5);
		}
		foreach (uint item6 in FourSnapTwistDropTheNeedleRest)
		{
			hashSet.Add(item6);
		}
		hitCounter2.ActionID = hashSet;
		hitCounter3.TargetHitCount = 2;
		hitCounter = hitCounter3;
		list.Add(SimpleElement.Rectangle(gameObject, 20f, 20f, 0f, null, facing, 3000f, 0f, hitCounter));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if ((TwoSnapTwistDropTheNeedleRest.Contains(info.ActionId) || ThreeSnapTwistDropTheNeedleRest.Contains(info.ActionId) || FourSnapTwistDropTheNeedleRest.Contains(info.ActionId)) && aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}
