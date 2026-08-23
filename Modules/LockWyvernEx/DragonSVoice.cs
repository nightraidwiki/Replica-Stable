using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.LockWyvernEx;

public class DragonSVoice : ISpecialAction
{
	private Vector3 Origin;

	public override string Name => "Dragon's Voice";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43891u, 45088u, 43892u, 45085u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if ((uint)(actionId - 43891) <= 1u || actionId == 45085 || actionId == 45088)
		{
			SimpleElement.Rectangle(info);
			Origin = info.Pos;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		uint actionId = info.ActionId;
		if (actionId - 43891 <= 1 || actionId == 45085 || actionId == 45088)
		{
			WDir wDir = -14f * info.Rotation.ToDirection();
			uint actionId2 = info.ActionId;
			WDir wDir2 = ((actionId2 == 43892 || actionId2 == 45088) ? wDir.OrthoR() : wDir.OrthoL());
			SimpleElement.Rectangle(Origin + wDir2.ToVec3(), 80f, 14f, 0f, info.Rotation, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 43898u, 45093u, 43897u, 45096u }
			});
		}
	}
}
