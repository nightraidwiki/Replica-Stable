using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P5;

public class Calamity : ISpecialAction
{
	public override string Name => "Calamity";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 49742u, 49743u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 49742)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 10f,
				radiusZ = 10f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47946u, 47947u }
				}
			});
		}
		if (info.ActionId == 49743)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customDonut",
				refRadian = 0.25f,
				refColor = GroundOmen.enemyColor,
				refTargetColor = GroundOmen.enemyColor,
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 40f,
				radiusZ = 40f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47946u, 47947u }
				}
			});
		}
	}
}
