using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CloudOfDarkness;

public class Flare : ISpecialAction
{
	public override string Name => "Flare (marker)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 2u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 346)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 25f,
				radiusZ = 25f,
				drawOnObject = true,
				delayDrawTime = 4000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40537u }
				}
			}, target);
		}
	}
}
