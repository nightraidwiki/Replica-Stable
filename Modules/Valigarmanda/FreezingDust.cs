using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Valigarmanda;

public class FreezingDust : ISpecialAction
{
	public override string Name => "Freezing Dust";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36848u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.ShowText("Move now!", RaptureAtkModule.TextGimmickHintStyle.Info, 7);
	}
}
