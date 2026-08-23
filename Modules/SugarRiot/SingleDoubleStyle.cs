using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.SugarRiot;

public class SingleDoubleStyle : ISpecialAction
{
	private bool target;

	public override string Name => "Single/Double Style";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42581u, 42583u, 42585u };

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id - 319 > 1 && Id != 324)
		{
			return;
		}
		IGameObject gameObject = actorId.GameObject();
		switch (gameObject.BaseId)
		{
		case 18330u:
			SimpleElement.Circle(gameObject.Position, 15f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42581u }
			});
			break;
		case 18331u:
			SimpleElement.Circle(gameObject.Position + 16f * gameObject.Rotation.Radians().ToDirection().ToVec3(), 15f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42583u }
			});
			break;
		case 18333u:
			if (target)
			{
				SimpleElement.RectangleToTarget(gameObject.Position, new Vector3(100f, 0f, 100f), 60f, 3.5f, 3000f, new HitCounter
				{
					ActionID = new HashSet<uint> { 42585u }
				});
			}
			else
			{
				aoes.Add(SimpleElement.Rectangle(gameObject.Position, 60f, 3.5f, 0f, gameObject.Rotation.Radians(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 42585u }
				}));
			}
			break;
		case 18332u:
			target = true;
			{
				foreach (StaticVfx aoe in aoes)
				{
					aoe.TargetPosition = new Vector3(100f, 0f, 100f);
				}
				break;
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		target = false;
		aoes.Clear();
	}
}
