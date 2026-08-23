using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DSR;

public class HeavenlyHeel : ISpecialAction
{
	public override string Name => "Heavenly Heel";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25543u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject? gameObject = info.SourceId.GameObject();
		IGameObject gameObject2 = info.TargetId.GameObject();
		if (gameObject != null && gameObject2 != null)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "tank_lockon01i",
				drawOnObject = true
			}, gameObject2);
		}
	}
}
