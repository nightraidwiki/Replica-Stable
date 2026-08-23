using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.JeunoArc1;

public class RakshasaSevenStarFist : ISpecialAction
{
	public override string Name => "Rakshasa Seven Star Fist";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40950u, 40951u, 40952u };

	public override void OnActionCast(ActorCastInfo info)
	{
		float radiusZ = info.ActionId switch
		{
			40950 => 12f, 
			40951 => 25f, 
			40952 => 38f, 
			_ => 0f, 
		};
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = radiusZ,
			drawOnObject = true,
			KnockBackCheck = new KnockBackCheck
			{
				OriginPos = new Vector3(800f, 0f, 400f),
				Antiable = false
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40953u }
			}
		}, Svc.Objects.LocalPlayer);
	}
}
