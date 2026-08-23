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

public class Brightwing : ISpecialAction
{
	public override string Name => "Brightwing";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25316u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject == null)
		{
			return;
		}
		foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => obj.ObjectKind == ObjectKind.Pc).ToList())
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan030_1bf",
				radiusX = 18f,
				radiusZ = 18f,
				drawOnObject = true,
				target = item,
				distanceCheck = new DistanceCheck
				{
					CheckObject = gameObject,
					CheckType = 0,
					Count = 2
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25369u },
					TargetHitCount = 8
				}
			}, gameObject);
		}
	}
}
