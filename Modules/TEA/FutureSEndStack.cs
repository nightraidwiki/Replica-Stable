using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class FutureSEndStack : ISpecialAction
{
	public override string Name => "Future's End β (stack)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18593u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "com_share0c",
			delayDrawTime = 28000f
		}, Svc.Objects.LocalPlayer);
	}
}
