using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class HolyShieldBash : ISpecialAction
{
	public override string Name => "Holy Shield Bash";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25550u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 12632);
		IGameObject gameObject2 = Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 12601);
		if (gameObject == null || gameObject2 == null)
		{
			return;
		}
		Data.TetherPlayer.Clear();
		foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc))
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 4f,
				radiusZ = 10f,
				drawOnObject = true,
				endToTarget = true,
				target = item,
				delayDrawTime = 2000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25297u }
				},
				TetherCheck = new TetherCheck
				{
					CheckType = 1,
					TetherID = new HashSet<int> { 84 }
				}
			};
			DrawManager.Draw(element, gameObject);
			DrawManager.Draw(element, gameObject2);
		}
	}
}
