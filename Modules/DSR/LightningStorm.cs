using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class LightningStorm : ISpecialAction
{
	public override string Name => "Lightning Storm";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25548u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => obj.ObjectKind == ObjectKind.Pc))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0420tar_5m0f",
				drawType = ElementType.LockOn,
				drawOnObject = true
			}, item);
		}
	}
}
