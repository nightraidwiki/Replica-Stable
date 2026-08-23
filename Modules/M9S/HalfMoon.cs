using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M9S;

public class HalfMoon : ISpecialAction
{
	public override string Name => "Half Moon";

	public override HashSet<uint> ActionID => new HashSet<uint> { 45943u, 45944u, 45945u, 45946u, 45947u, 45948u, 45949u, 45950u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			drawAvfx = "gl_fan180_1bf",
			Position = info.Pos,
			drawOnObject = false,
			radiusX = 64f,
			radiusZ = 64f,
			refRotation = info.Facing,
			destroyTime = info.CastTime * 1000f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { info.ActionId }
			}
		};
		aoes.Add(DrawManager.Draw(element));
		aoes.SortBy((StaticVfx x) => x.DrawTime);
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}
