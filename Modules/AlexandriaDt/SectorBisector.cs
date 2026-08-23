using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop.Game;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.AlexandriaDt;

public class SectorBisector : ISpecialAction
{
	public int cloneCount;

	public override string Name => "Sector Bisector";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42566u, 42567u };

	public unsafe override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id == 313)
		{
			cloneCount++;
		}
		if (Id == 327)
		{
			base.NumCasts++;
			if (base.NumCasts == cloneCount)
			{
				IGameObject gameObject = actorId.GameObject();
				Character* ptr = gameObject.Struct();
				SimpleElement.Fan(gameObject.Position, 45f, 180, gameObject.Rotation.Radians() + ((ptr->Timeline.ModelState != 6) ? 1 : (-1)) * 90.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = ActionID
				});
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		cloneCount = 0;
		base.NumCasts = 0;
	}

	public override void Reset()
	{
		cloneCount = 0;
		base.Reset();
	}
}
