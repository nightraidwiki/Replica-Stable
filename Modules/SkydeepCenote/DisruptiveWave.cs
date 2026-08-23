using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.SkydeepCenote;

public class DisruptiveWave : ISpecialAction
{
	public override string Name => "Disruptive Wave";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 514)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan030_1bf",
				radiusX = 24f,
				radiusZ = 24f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 36729u }
				},
				StatusCheck = new StatusCheck
				{
					CheckObject = target,
					Status = 3950u
				}
			}, target);
		}
	}
}
