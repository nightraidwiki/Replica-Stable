using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.SkydeepCenote;

public class Scatter : ISpecialAction
{
	public override string Name => "Scatter";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36736u, 36737u };

	public override void OnActionCast(ActorCastInfo info)
	{
		int num = ((info.ActionId == 36736) ? 8 : (-8));
		foreach (IGameObject item in Svc.Objects.Where(delegate(IGameObject o)
		{
			if (o.BaseId == 16852)
			{
				ICharacter character = (ICharacter)((o is ICharacter) ? o : null);
				if (character != null)
				{
					return character.IsCharacterVisible();
				}
			}
			return false;
		}))
		{
			SimpleElement.Circle(new Vector3(item.Position.X + (float)num, item.Position.Y, item.Position.Z), 6f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 36738u }
			});
		}
	}
}
