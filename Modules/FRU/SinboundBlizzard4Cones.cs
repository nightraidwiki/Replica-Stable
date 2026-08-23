using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class SinboundBlizzard4Cones : ISpecialAction
{
	public List<Angle> angles = new List<Angle>();

	public override string Name => "Sinbound Blizzard (4 cones)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40144u, 40148u, 40329u, 40330u, 40145u, 40146u };

	public override void OnActionCast(ActorCastInfo info)
	{
		Reset();
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			IGameObject? source = info.SourceId.GameObject();
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40145u }
			};
			SimpleElement.FanToTarget(source, allPlayer, 60f, 25, Follow: true, default(Angle), 0f, 3000f, hitCounter);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 40145)
		{
			angles.Add(info.Source.Rotation.Radians());
			base.NumCasts++;
		}
		if (info.ActionId == 40146)
		{
			base.NumCasts++;
		}
		if (base.NumCasts % 8 != 0)
		{
			return;
		}
		switch (base.NumCasts / 8)
		{
		case 1:
		{
			foreach (Angle angle in angles)
			{
				SimpleElement.Fan(info.Source, 50f, 25, angle, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 40146u }
				});
			}
			break;
		}
		case 2:
		{
			foreach (Angle angle2 in angles)
			{
				SimpleElement.Fan(info.Source, 50f, 25, angle2 - 22.5f.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 40146u }
				});
			}
			break;
		}
		case 3:
		{
			foreach (Angle angle3 in angles)
			{
				SimpleElement.Fan(info.Source, 50f, 25, angle3 - 45.Degrees(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 40146u }
				});
			}
			break;
		}
		}
	}

	public override void Reset()
	{
		angles.Clear();
		base.Reset();
	}
}
