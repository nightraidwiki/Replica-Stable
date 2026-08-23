using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class HotWingTail : ISpecialAction
{
	public override string Name => "Hot Wing / Hot Tail";

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 27947u, 27949u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject != null)
		{
			if (info.ActionId == 27947)
			{
				DrawElement obj = new DrawElement
				{
					drawAvfx = "general02xf",
					radiusX = 10.5f,
					radiusZ = 50f,
					refOffsetX = 14.5f,
					drawOnObject = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 27947u }
					}
				};
				DrawManager.Draw(obj, gameObject);
				obj.refOffsetX = -14.5f;
				DrawManager.Draw(obj, gameObject);
			}
			else
			{
				SimpleElement.Rectangle(info, 50f, 8f);
			}
		}
	}
}
