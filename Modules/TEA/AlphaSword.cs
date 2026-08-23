using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class AlphaSword : ISpecialAction
{
	public override string Name => "Alpha Sword";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18527u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 11342);
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan090_1bf",
				radiusX = 30f,
				radiusZ = 30f,
				target = allPlayer,
				drawOnObject = true,
				distanceCheck = new DistanceCheck
				{
					CheckObject = gameObject,
					CheckType = 0,
					Count = 3
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 18539u },
					TargetHitCount = 3
				}
			}, gameObject);
		}
	}
}
