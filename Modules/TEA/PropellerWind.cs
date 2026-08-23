using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class PropellerWind : ISpecialAction
{
	public override string Name => "Propeller Wind";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18482u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 11393);
		if (gameObject != null)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan060_1bpf",
				radiusX = 50f,
				radiusZ = 50f,
				target = gameObject,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 18482u }
				}
			}, info.SourceId.GameObject());
		}
	}
}
