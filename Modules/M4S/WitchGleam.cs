using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Helper;
using Replica.Engine.Interop.ActionEffect;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class WitchGleam : ISpecialAction
{
	public static int[] Stacks = new int[8];

	public override string Name => "Ion Cluster Gleam";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38789u, 38790u };

	public static List<IGameObject> Players => PlayerHelper.AllPlayers;

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 38789)
		{
			Stacks = new int[8];
		}
		if (info.ActionId != 38790)
		{
			return;
		}
		int num = -1;
		TargetEffect[] targetEffects = info.TargetEffects;
		for (int i = 0; i < targetEffects.Length; i++)
		{
			TargetEffect effect = targetEffects[i];
			num = Players.FindIndex((IGameObject x) => x.EntityId == effect.TargetID);
		}
		if (num >= 0)
		{
			Stacks[num]++;
		}
	}

	public override void Reset()
	{
		Stacks = new int[8];
	}
}
