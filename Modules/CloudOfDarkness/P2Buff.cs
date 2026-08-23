using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CloudOfDarkness;

public class P2Buff : ISpecialAction
{
	private enum Mechanic
	{
		None,
		Dorsal,
		Palmar
	}

	private Mechanic currentMechanic;

	public override string Name => "Release (buff)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40505u, 40506u };

	public override uint Phase => 2u;

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 40505u:
			currentMechanic = Mechanic.Dorsal;
			break;
		case 40506u:
			currentMechanic = Mechanic.Palmar;
			break;
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 4181 && currentMechanic != Mechanic.None)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 4f,
				radiusZ = 8f,
				refRotation = ((currentMechanic == Mechanic.Dorsal) ? 0.Degrees() : 180.Degrees()),
				drawOnObject = true,
				delayDrawTime = (info.Time - 7f) * 1000f,
				destroyTime = 7000f,
				StatusCheck = new StatusCheck
				{
					Status = 4181u,
					CheckObject = info.TargetID.GameObject()
				}
			}, info.TargetID.GameObject());
		}
	}

	public override void Reset()
	{
		currentMechanic = Mechanic.None;
		base.Reset();
	}
}
