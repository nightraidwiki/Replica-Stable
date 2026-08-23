using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M8S;

public class AlphaWindStone : ISpecialAction
{
	public override string Name => "Alpha Stone/Wind";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon != 23 || ++base.NumCasts % 2 != 0)
		{
			return;
		}
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18225);
		IGameObject gameObject2 = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18219);
		foreach (IGameObject item in PlayerHelper.Tank)
		{
			IGameObject target2 = (item.HasStatus(4389u) ? gameObject : gameObject2);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan090_1bf",
				radiusX = 40f,
				radiusZ = 40f,
				drawOnObject = true,
				target = item,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41933u, 41954u }
				}
			}, target2);
		}
	}
}
