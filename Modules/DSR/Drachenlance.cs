using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class Drachenlance : ISpecialAction
{
	public override string Name => "Drachenlance";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 26379u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject != null)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan090_1bf",
				radiusX = 13f,
				radiusZ = 13f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 26380u }
				}
			}, gameObject);
		}
	}
}
