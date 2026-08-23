using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M7S;

public class QuarrySwamp : ISpecialAction
{
	public override string Name => "Quarry Swamp";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42357u };

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.Channeling,
			drawAvfx = "chn_miruna1v",
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 42357u }
			}
		}, Svc.Objects.LocalPlayer, info.SourceId.GameObject());
	}
}
