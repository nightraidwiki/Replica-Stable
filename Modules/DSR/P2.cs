using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.DSR;

public class P2 : ISpecialAction
{
	public override string Name => "Skyward (marker)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 0u };

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 330)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customCircle",
				radiusX = 24f,
				radiusZ = 24f,
				drawOnObject = true,
				destroyTime = 8000f,
				refColor = new Vector4(1f, 1f, 1f, 0.5f),
				refTargetColor = new Vector4(1f, 0f, 0f, 0.5f)
			}, target);
		}
	}
}
