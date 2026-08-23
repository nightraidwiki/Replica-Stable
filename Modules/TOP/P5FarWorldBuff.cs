using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class P5FarWorldBuff : ISpecialAction
{
	public override string Name => "Far World (buff)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3443)
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
						ActionID = new HashSet<uint> { 33040u },
						HitTarget = gameObject
					}
				}, gameObject);
			}
		}
	}
}
