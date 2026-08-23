using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class HolyRayLineStack : ISpecialAction
{
	public override string Name => "Holy Ray (line stack)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject source, uint icon, ulong TargetID)
	{
		if (icon == 525)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02pxf",
				radiusX = 3f,
				radiusZ = 65f,
				drawOnObject = true,
				target = TargetID.GameObject(),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40211u }
				}
			}, source);
		}
	}
}
