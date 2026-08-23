using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class Heavensblaze : ISpecialAction
{
	public override string Name => "Heavensblaze";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25309u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.TargetId);
		if (gameObject != null)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "com_share0c",
				drawOnObject = true
			}, gameObject);
		}
	}
}
