using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Valigarmanda;

public class ThunderousBreath : ISpecialAction
{
	public override string Name => "Thunderous Breath";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36835u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.ShowText("Float up", RaptureAtkModule.TextGimmickHintStyle.Info, 8);
	}
}
