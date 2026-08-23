using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class TwistingDive2 : ISpecialAction
{
	public override string Name => "Twisting Dive";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25556u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject != null)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 8f,
				radiusZ = 52f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { info.ActionId }
				}
			}, gameObject);
		}
	}
}
