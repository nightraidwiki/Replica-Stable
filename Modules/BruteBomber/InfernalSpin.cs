using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.BruteBomber;

public class InfernalSpin : ISpecialAction
{
	private bool clockwise = true;

	public override string Name => "Infernal Spin";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39770u };

	public override void OnActionCast(ActorCastInfo info)
	{
		for (int i = 1; i <= 7; i++)
		{
			SimpleElement.Fan(info.SourceId, 40f, 60, info.Facing + (clockwise ? (-(45.Degrees() * i)) : (45.Degrees() * i)), 5000f, 1000 * i);
		}
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (target.BaseId == 17090)
		{
			if (icon == 167)
			{
				clockwise = true;
			}
			else
			{
				clockwise = false;
			}
		}
	}
}
