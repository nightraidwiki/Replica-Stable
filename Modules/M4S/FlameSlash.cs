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

namespace Replica.Modules.M4S;

public class FlameSlash : ISpecialAction
{
	public override string Name => "Flame Slash";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38342u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => o.BaseId == 17325))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 2.5f,
				radiusZ = 60f,
				drawOnObject = true,
				target = item,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38343u },
					TargetHitCount = 6
				}
			}, info.SourceId.GameObject());
		}
	}
}
