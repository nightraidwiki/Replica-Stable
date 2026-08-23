using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP.P5;

public class NearWorld : ISpecialAction
{
	public override string Name => "Near World";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3442)
		{
			IGameObject gameObject = info.TargetID.GameObject();
			if (gameObject != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 8f,
					radiusZ = 8f,
					drawOnObject = true,
					delayDrawTime = (int)(info.Time - 6f) * 1000,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 31625u },
						HitTarget = gameObject
					}
				}, gameObject);
			}
		}
	}
}
