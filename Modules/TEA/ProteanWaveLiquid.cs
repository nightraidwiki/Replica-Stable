using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class ProteanWaveLiquid : ISpecialAction
{
	public override string Name => "Protean Wave";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18466u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "gl_fan030_1bf",
			radiusX = 40f,
			radiusZ = 40f,
			drawOnObject = true,
			refRotation = info.Source.Rotation.Radians(),
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18467u },
				TargetHitCount = 2
			}
		}, info.Source);
		Draw(info.Source);
		new TimeHelper(2800L, delegate
		{
			Draw(info.Source);
		});
	}

	private static void Draw(IGameObject obj)
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
					CheckObject = obj,
					CheckType = 0,
					Count = 4
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 18469u },
					TargetHitCount = 4
				}
			}, obj);
		}
	}
}
