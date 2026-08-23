using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.LockWyvernEx;

public class LockbladeDiveDragonSVoiceSides : ISpecialAction
{
	public override string Name => "Lockblade Dive (Dragon's Voice, sides)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43905u, 43904u, 45102u, 45103u };

	public override void Update()
	{
		if (aoes.Count == 0)
		{
			return;
		}
		LockbladeDiveDragonSVoice specialAction = ModuleUtil.GetSpecialAction<LockbladeDiveDragonSVoice>();
		foreach (StaticVfx aoe in aoes)
		{
			aoe.Color = new Vector4(1f, 1f, 1f, (specialAction.aoes.Count > 0) ? 0.3f : Plugin.Config.CustomAlpha);
			aoe.TargetColor = new Vector4(1f, 1f, 1f, (specialAction.aoes.Count > 0) ? 0.3f : Plugin.Config.CustomAlpha);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		aoes.Add(SimpleElement.Rectangle(info));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}
