using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class MonitorCleave : ISpecialAction
{
	public override string Name => "Monitor Cleave";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31638u, 31639u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject();
		SimpleElement.Fan(gameObject, 100f, 180, gameObject.Rotation.Radians() + ((info.ActionId == 31638) ? (-90.Degrees()) : 90.Degrees()), 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { info.ActionId }
		});
	}
}
