using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.CloudOfDarkness;

public class UnholyDarkness : ISpecialAction
{
	public override string Name => "Unholy Darkness (stack x4)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 2u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 100)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bpxf",
				radiusX = 6f,
				radiusZ = 6f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41262u }
				}
			}, target);
		}
	}
}
