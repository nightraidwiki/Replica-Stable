using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class AscalonsMercyRevealed : ISpecialAction
{
	public override string Name => "Ascalon's Mercy Revealed";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25546u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IEnumerable<IGameObject> enumerable = Svc.Objects.Where((IGameObject obj) => obj.ObjectKind == ObjectKind.Pc);
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject == null)
		{
			return;
		}
		foreach (IGameObject item in enumerable)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan030_1bf",
				radiusX = 50f,
				radiusZ = 50f,
				drawOnObject = true,
				target = item,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25547u }
				}
			}, gameObject);
		}
	}
}
