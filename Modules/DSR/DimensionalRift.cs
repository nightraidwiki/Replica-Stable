using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class DimensionalRift : ISpecialAction
{
	public override string Name => "Dimensional Rift";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25308u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => obj.BaseId == 13071))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 9f,
				radiusZ = 9f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25301u }
				}
			}, item);
		}
	}
}
