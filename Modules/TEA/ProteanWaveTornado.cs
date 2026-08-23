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

public class ProteanWaveTornado : ISpecialAction
{
	public override string Name => "Protean Wave 2";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18869u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => o.BaseId == 11337))
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan030_1bf",
					radiusX = 40f,
					radiusZ = 40f,
					drawOnObject = true,
					target = allPlayer,
					distanceCheck = new DistanceCheck
					{
						CheckObject = item,
						CheckType = 0
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 18870u }
					},
					OnlyVisible = true
				}, item);
			}
		}
	}
}
