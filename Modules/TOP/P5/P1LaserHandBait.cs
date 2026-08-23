using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TOP.P5;

public class P1LaserHandBait : ISpecialAction
{
	public override string Name => "P1 Laser Hand (bait)";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31624u, 31528u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 31624)
		{
			base.CanDraw = true;
		}
		if (info.ActionId == 31528)
		{
			base.CanDraw = false;
		}
	}

	public override void OnTargetIconEvent(IGameObject source, uint icon, ulong TargetID)
	{
		if (source.BaseId - 15718 <= 1 && base.CanDraw)
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "m0119_trap_02t",
				radiusX = 1.5f,
				radiusY = 5f,
				radiusZ = 1.5f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31528u }
				}
			};
			switch (icon)
			{
			case 156u:
				drawElement.refOffsetX = 3.08f;
				drawElement.refOffsetZ = -1f;
				DrawManager.Draw(drawElement, source);
				break;
			case 157u:
				drawElement.refOffsetX = -2.6f;
				drawElement.refOffsetZ = -1.04f;
				DrawManager.Draw(drawElement, source);
				break;
			}
			Plugin.DebugChat("P5 P1 laser hand bait guide");
		}
	}
}
