using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingGreen;

public class TwoFourSnapTwist : ISpecialAction
{
	private static readonly HashSet<uint> TwoSnapTwistFirst = new HashSet<uint> { 42704u, 42701u, 42699u, 42702u, 42197u, 42700u, 42703u, 42198u };

	private static readonly HashSet<uint> FourSnapTwistFirst = new HashSet<uint> { 42719u, 42720u, 42716u, 42718u, 42201u, 42717u, 42721u, 42202u };

	public override string Name => "Two/Four Snap Twist";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (uint item in TwoSnapTwistFirst)
			{
				hashSet.Add(item);
			}
			hashSet.Add(42705u);
			hashSet.Add(42724u);
			foreach (uint item2 in FourSnapTwistFirst)
			{
				hashSet.Add(item2);
			}
			hashSet.Add(42706u);
			hashSet.Add(42725u);
			return hashSet;
		}
	}

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (TwoSnapTwistFirst.Contains(info.ActionId) || FourSnapTwistFirst.Contains(info.ActionId))
		{
			aoes.Add(SimpleElement.Rectangle(info.Pos, 20f, 20f, 0f, info.Facing, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42705u, 42724u }
			}));
			aoes.Add(SimpleElement.Rectangle(info.Pos, 20f, 20f, 0f, info.Facing + 180.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42706u, 42725u }
			}));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		uint actionId = info.ActionId;
		if ((actionId - 42705 <= 1 || actionId - 42724 <= 1) && aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}
