using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P5;

public class ChaosApocalypse : ISpecialAction
{
	public override string Name => "Chaos Apocalypse";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47932u };

	public override void OnActionCast(ActorCastInfo info)
	{
		WDir wDir = info.Facing.ToDirection();
		WPos wPos = new WPos(info.Pos.X, info.Pos.Z);
		for (int i = 0; i < 8; i++)
		{
			WPos wPos2 = wPos + 7f * (float)i * wDir;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				Position = new Vector3(wPos2.X, 0f, wPos2.Z),
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 6f,
				refColor = GroundOmen.Yellow,
				refTargetColor = GroundOmen.Red,
				destroyTime = 4500 + i * 500
			});
		}
	}
}
