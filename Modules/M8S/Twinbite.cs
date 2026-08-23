using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M8S;

public class Twinbite : ISpecialAction
{
	public override string Name => "Twinbite";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42189u, 42190u };

	public override void Update()
	{
		for (int i = 0; i < PlayerHelper.Tank.Count; i++)
		{
			IGameObject gameObject = PlayerHelper.Tank[i];
			for (int j = 0; j < 5; j++)
			{
				WPos origin = new WPos(HowlingBlade.EndArenaPlatforms[j]);
				if (new WPos(gameObject.Position).InCircle(origin, 8f) && aoes.Count > i)
				{
					aoes[i].Position = origin.ToVec3(-150f);
					break;
				}
			}
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId != 42189)
		{
			return;
		}
		foreach (IGameObject item in PlayerHelper.Tank)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "tank_lockon_8s_01t"
			}, item);
			aoes.Add(SimpleElement.Circle(Vector3.Zero, 8f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42190u }
			}));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 42190)
		{
			aoes.Clear();
		}
	}
}
