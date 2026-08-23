using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.E11;

public class FellruinTrailLine : ISpecialAction
{
	public override string Name => "Fellruin Trail (line)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 1u;

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 1678)
		{
			IGameObject gameObject = info.TargetID.GameObject();
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 8f,
				radiusZ = 50f,
				drawOnObject = true,
				refRotation = gameObject.Rotation.Radians(),
				fixRotation = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 22078u }
				}
			}, gameObject);
		}
	}
}
