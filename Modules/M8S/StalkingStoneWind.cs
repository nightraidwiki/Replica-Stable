using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M8S;

public class StalkingStoneWind : ISpecialAction
{
	public override string Name => "Stalking Stone/Wind";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint WeatherID => 2u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 23)
		{
			IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18225);
			IGameObject gameObject2 = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18219);
			IGameObject gameObject3 = (target.HasStatus(4389u) ? gameObject : gameObject2);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02pxf",
				Position = gameObject3.Position,
				drawOnObject = false,
				radiusX = 3f,
				radiusZ = 40f,
				target = target,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41935u, 41956u }
				}
			});
			SimpleLockon.ShareRect5s(target, gameObject3);
		}
	}
}
