using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M6S;

public class StickyMousse : ISpecialAction
{
	public override string Name => "Sticky Mousse";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42645u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in PlayerHelper.Healer.Union(PlayerHelper.DPS))
		{
			SimpleElement.Circle(item, 4f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 42646u }
			});
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4453)
		{
			IGameObject gameObject = info.TargetID.GameObject();
			SimpleLockon.ShareLockon(gameObject);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bpxf",
				radiusX = 4f,
				radiusZ = 4f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42647u }
				}
			}, gameObject);
		}
	}
}
