using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class IceFireTower : ISpecialAction
{
	public override string Name => "Ice / Fire (tower)";

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 27957u, 27956u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (base.NumCasts <= 0)
		{
			base.NumCasts++;
			DrawElement obj = new DrawElement
			{
				drawAvfx = "share_trap01k1",
				Position = new Vector3(103.6f, 0f, 119f),
				drawOnObject = false,
				radiusX = 1f,
				radiusY = 3f,
				radiusZ = 1f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 27958u }
				}
			};
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
			obj.Position = new Vector3(96.3f, 0f, 119f);
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
			obj.Position = new Vector3(100f, 0f, 109.3f);
			DrawManager.Draw(obj, Svc.Objects.LocalPlayer);
		}
	}
}
