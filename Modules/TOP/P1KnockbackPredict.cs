using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP;

public class P1KnockbackPredict : ISpecialAction
{
	public override string Name => "P1 Knockback (predict)";

	public override uint Phase => 2u;

	public override uint WeatherID => 78u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31550u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 100 && base.CanDraw)
		{
			base.CanDraw = false;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				radiusX = 1f,
				radiusZ = 13f,
				drawOnObject = true,
				KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3(100f, 0f, 100f)
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31534u }
				}
			}, Svc.Objects.LocalPlayer);
		}
	}
}
