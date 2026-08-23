using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CloudOfDarkness;

public class EvilSeed : ISpecialAction
{
	public override string Name => "Evil Seed (marker)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 3u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 551)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 5f,
				radiusZ = 5f,
				drawOnObject = true,
				destroyTime = 8000f
			}, target);
		}
	}
}
