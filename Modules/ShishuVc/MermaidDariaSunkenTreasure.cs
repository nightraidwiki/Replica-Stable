using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuVc;

public class MermaidDariaSunkenTreasure : ISpecialAction
{
	public override string Name => "Mermaid Daria Sunken Treasure";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45849u };

	public override void OnActionCast(ActorCastInfo info)
	{
		Reset();
	}

	public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
	{
		IGameObject gameObject = actorID.GameObject();
		switch (gameObject.BaseId)
		{
		case 2015004u:
			if (p1 == 16 && p2 == 32)
			{
				SimpleElement.Circle(gameObject.Position, 18f, (base.NumCasts > 0) ? 3500 : 10000, (base.NumCasts > 0) ? 6500 : 0);
				base.NumCasts++;
			}
			break;
		case 2015005u:
			if (p1 == 16 && p2 == 32)
			{
				SimpleElement.Donut(gameObject.Position, 5f, 20f, 10000f);
			}
			break;
		}
	}
}
