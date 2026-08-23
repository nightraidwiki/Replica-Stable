using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.P12S.P12S;

public class PalladianRay : ISpecialAction
{
	public override string Name => "Palladian Ray";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33571u, 33572u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId != 33571)
		{
			return;
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan030_1bf",
				Position = new Vector3(92f, 0f, 92f),
				drawOnObject = false,
				radiusX = 100f,
				radiusZ = 100f,
				target = allPlayer,
				distanceCheck = new DistanceCheck
				{
					CheckType = 4,
					Count = 4,
					Position = new Vector3(92f, 0f, 92f)
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 33572u }
				}
			}, Svc.Objects.LocalPlayer);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan030_1bf",
				Position = new Vector3(108f, 0f, 92f),
				drawOnObject = false,
				radiusX = 100f,
				radiusZ = 100f,
				target = allPlayer,
				distanceCheck = new DistanceCheck
				{
					CheckType = 4,
					Count = 4,
					Position = new Vector3(108f, 0f, 92f)
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 33572u }
				}
			}, Svc.Objects.LocalPlayer);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 33572)
		{
			SimpleElement.Fan(info.Source, 100f, 30, info.Source.Rotation.Radians(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 33573u },
				TargetHitCount = 40
			});
		}
	}
}
