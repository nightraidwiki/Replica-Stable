using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M9S;

public class AetherSlamShatter : ISpecialAction
{
	private class Vampette
	{
		public IGameObject Actor;

		public WPos StartPos;

		public WPos EndPos;
	}

	private readonly List<Vampette> vamps = new List<Vampette>();

	public override string Name => "Aether Slam / Shatter";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45992u, 45993u, 45994u, 45995u };

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 19503 && id == 7750)
		{
			WPos wPos = new WPos(source.Position);
			WPos endPos = new WPos(100f, 100f) - (wPos - new WPos(100f, 100f));
			vamps.Add(new Vampette
			{
				Actor = source,
				StartPos = wPos,
				EndPos = endPos
			});
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 2056)
		{
			return;
		}
		for (int i = 0; i < vamps.Count; i++)
		{
			if (vamps[i].Actor.GameObjectId == info.TargetID)
			{
				switch (info.Stack)
				{
				case 1062u:
					SimpleElement.Circle((base.NumCasts == 0) ? vamps[i].EndPos.ToVec3() : vamps[i].StartPos.ToVec3(), 7f, 3000f, 2000f, new HitCounter
					{
						ActionID = ActionID
					});
					break;
				case 1063u:
					SimpleElement.Donut((base.NumCasts == 0) ? vamps[i].EndPos.ToVec3() : vamps[i].StartPos.ToVec3(), 4f, 15f, 3000f, 2000f, new HitCounter
					{
						ActionID = ActionID
					});
					break;
				}
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts++;
	}

	public override void Reset()
	{
		vamps.Clear();
		base.Reset();
	}
}
