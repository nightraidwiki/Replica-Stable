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

namespace Replica.Modules.BlackCat;

public class Mouser : ISpecialAction
{
	public override string Name => "Mouser";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37653u, 39275u, 38053u };

	public override void OnActionCast(ActorCastInfo info)
	{
		ushort actionId = info.ActionId;
		if (actionId != 37653 && actionId != 39275)
		{
			return;
		}
		base.NumCasts++;
		IGameObject gameObject = info.SourceId.GameObject();
		if (gameObject != null)
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "customRect2",
				Position = gameObject.Position,
				radiusX = 5f,
				radiusZ = 5f,
				fixRotation = true,
				drawOnObject = false,
				refColor = GroundOmen.enemyColor,
				refTargetColor = GroundOmen.enemyColor,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38053u },
					TargetHitCount = base.NumCasts
				}
			};
			if (info.ActionId == 39275)
			{
				drawElement.refColor = new Vector4(1f, 0f, 0f, 1f);
				drawElement.refTargetColor = new Vector4(1f, 0f, 0f, 1f);
			}
			DrawManager.Draw(drawElement, gameObject);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 38053)
		{
			base.NumCasts = 0;
		}
	}
}
