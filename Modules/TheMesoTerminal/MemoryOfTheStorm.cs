using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.TheMesoTerminal;

public class MemoryOfTheStorm : ISpecialAction
{
	public override string Name => "Memory Of The Storm";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43822u };

	public override uint Phase => 3u;

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon == 525)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02pxf",
				Position = Source.Position,
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 60f,
				target = TargetID.GameObject(),
				hitCounter = new HitCounter
				{
					ActionID = ActionID
				}
			});
		}
	}
}
