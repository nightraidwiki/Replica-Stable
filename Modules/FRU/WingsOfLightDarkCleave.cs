using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class WingsOfLightDarkCleave : ISpecialAction
{
	public static bool LightFirst;

	private IGameObject? firstTarget;

	public override string Name => "Wings of Light/Dark (cleave)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40313u, 40233u };

	public override void OnActionCast(ActorCastInfo info)
	{
		LightFirst = info.ActionId == 40313;
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17839);
		firstTarget = gameObject.TargetObject;
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customFan",
			refRadian = 225f.Degrees().Rad,
			radiusX = 100f,
			radiusZ = 100f,
			refOffsetRotation = ((info.ActionId == 40313) ? 67.5f.Degrees() : (-67.5f.Degrees())),
			target = firstTarget,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40314u, 40315u }
			},
			refColor = ((info.ActionId == 40313) ? GroundOmen.enemyColor : new Vector4(0.94f, 0f, 1f, Plugin.Config.CustomAlpha)),
			refTargetColor = ((info.ActionId == 40313) ? GroundOmen.enemyColor : new Vector4(0.94f, 0f, 1f, Plugin.Config.CustomAlpha))
		}, gameObject);
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17839);
		IGameObject target2 = PlayerHelper.Tank.FirstOrDefault((IGameObject o) => o != firstTarget);
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customFan",
			refRadian = 225f.Degrees().Rad,
			radiusX = 100f,
			radiusZ = 100f,
			delayDrawTime = 1000f,
			refOffsetRotation = ((info.ActionId == 40313) ? (-67.5f.Degrees()) : 67.5f.Degrees()),
			target = target2,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40314u, 40315u },
				TargetHitCount = 2
			},
			refColor = ((info.ActionId == 40313) ? new Vector4(0.94f, 0f, 1f, Plugin.Config.CustomAlpha) : GroundOmen.enemyColor),
			refTargetColor = ((info.ActionId == 40313) ? new Vector4(0.94f, 0f, 1f, Plugin.Config.CustomAlpha) : GroundOmen.enemyColor)
		}, target);
	}
}
