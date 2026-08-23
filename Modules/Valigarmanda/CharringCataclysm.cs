using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Valigarmanda;

public class CharringCataclysm : ISpecialAction
{
	public override string Name => "Charring Cataclysm";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36808u, 36812u, 36816u };

	public override uint WeatherID => 14u;

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in PlayerHelper.Tank.Union(PlayerHelper.Healer))
		{
			SimpleLockon.ShareLockon2(item, 2300f);
		}
	}
}
