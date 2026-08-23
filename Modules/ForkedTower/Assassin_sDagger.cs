using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ForkedTower;

public class Assassin_sDagger : ISpecialAction
{
	public override string Name => "Assassin's Dagger";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41569u, 41570u };

	public override uint Phase => 4u;

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 41569)
		{
			base.NumCasts = 0;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 3f,
				targetPosition = new Vector3(699.97705f, -476f, -674.0063f),
				endToTarget = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41570u }
				}
			});
		}
		if (info.ActionId == 41570)
		{
			base.NumCasts++;
			if ((base.NumCasts - 1) / 3 % 2 == 0)
			{
				WPos wPos = WPos.RotateAroundOrigin(50f, new WPos(699.97705f, -674.0063f), new WPos(info.Source.Position));
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					Position = new Vector3(wPos.X, -476f, wPos.Z),
					drawOnObject = false,
					radiusX = 3f,
					targetPosition = new Vector3(699.97705f, -476f, -674.0063f),
					endToTarget = true,
					destroyTime = 4000f
				});
			}
		}
	}
}
