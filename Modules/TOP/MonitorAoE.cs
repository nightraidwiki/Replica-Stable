using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP;

public class MonitorAoE : ISpecialAction
{
	public override string Name => "Monitor AoE";

	public override uint Phase => 3u;

	public override uint WeatherID => 79u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31595u, 31596u };

	public override void OnActionCast(ActorCastInfo info)
	{
		List<IGameObject> target = Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc && o.GameObjectId != Player.Object.GameObjectId).ToList();
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 7f,
			radiusZ = 7f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 31597u }
			}
		}, target);
	}
}
