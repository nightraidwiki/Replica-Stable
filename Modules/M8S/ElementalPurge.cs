using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M8S;

public class ElementalPurge : ISpecialAction
{
	private IGameObject? fanTank;

	public override string Name => "Elemental Purge";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42093u };

	public override void Update()
	{
		if (fanTank == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			WPos origin = new WPos(HowlingBlade.EndArenaPlatforms[i]);
			if (new WPos(fanTank.Position).InCircle(origin, 8f) && aoes.Count > 0)
			{
				aoes[0].TargetPosition = origin.ToVec3(-150f);
				break;
			}
		}
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon != 23)
		{
			return;
		}
		ICharacter character = (ICharacter)((target is ICharacter) ? target : null);
		if (character == null || character.GetRole() != CombatRole.Tank)
		{
			return;
		}
		DrawElement element = new DrawElement
		{
			drawAvfx = "m0347_sircle_01m1",
			radiusX = 16f,
			radiusZ = 16f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 42088u }
			}
		};
		DrawManager.Draw(element, target);
		foreach (IGameObject item in PlayerHelper.Tank)
		{
			if (item != target)
			{
				fanTank = item;
				element = new DrawElement
				{
					drawAvfx = "gl_fan210_1bf",
					Position = new Vector3(100f, -150f, 100f),
					drawOnObject = false,
					targetPosition = item.Position,
					radiusX = 40f,
					radiusZ = 40f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 42093u }
					}
				};
				aoes.Add(DrawManager.Draw(element));
			}
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4395)
		{
			SimpleLockon.ShareLockon(info.TargetID.GameObject());
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		fanTank = null;
		aoes.Clear();
	}

	public override void Reset()
	{
		fanTank = null;
		base.Reset();
	}
}
