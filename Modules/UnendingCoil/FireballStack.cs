using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UnendingCoil;

public class FireballStack : ISpecialAction
{
	public override string Name => "Fireball (stack)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 117)
		{
			SimpleLockon.Share6S(target);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bpxf",
				radiusX = 4f,
				radiusZ = 4f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 9900u }
				}
			}, target);
		}
	}
}
