using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class Heavensflame : ISpecialAction
{
	public override string Name => "Heavensflame";

	public override uint Phase => 1u;

	public override uint WeatherID => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25310u };

	public override void OnActionCast(ActorCastInfo info)
	{
		List<IGameObject> target = Svc.Objects.Where((IGameObject obj) => obj.ObjectKind == ObjectKind.Pc).ToList();
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 10f,
			radiusY = 10f,
			radiusZ = 10f,
			drawOnObject = true,
			delayDrawTime = 4000f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 25311u }
			}
		}, target);
	}
}
