using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CloudOfDarkness;

public class BreakIV : ISpecialAction
{
	public override string Name => "Break IV (look away)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40527u, 40530u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.Channeling,
			drawAvfx = "chn_chainlightning_3t1",
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40530u }
			}
		}, Svc.Objects.LocalPlayer, info.SourceId.GameObject());
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.Channeling,
			drawAvfx = "chn_miruna1v",
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40530u }
			}
		}, Svc.Objects.LocalPlayer, info.SourceId.GameObject());
	}
}
