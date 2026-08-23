using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.JeunoArc1;

public class UnboundRage : ISpecialAction
{
	public override string Name => "Unbound Rage";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 4u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 471)
		{
			IGameObject target2 = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18007);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 4f,
				radiusZ = 100f,
				target = target,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40808u }
				}
			}, target2);
		}
	}
}
