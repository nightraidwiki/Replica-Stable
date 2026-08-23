using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.TOP.P5;

public class P2TailGroupLine : ISpecialAction
{
	public override string Name => "P2 Tail Group Line";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		IBattleChara battleChara = (IBattleChara)((source is IBattleChara) ? source : null);
		if (battleChara != null && battleChara.NameId == 7639 && id == 7747)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_x02f",
				radiusX = 6f,
				radiusZ = 50f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31631u }
				}
			}, source);
		}
	}

	public override void OnTargetIconEvent(IGameObject source, uint icon, ulong TargetID)
	{
		IBattleChara battleChara = (IBattleChara)((source is IBattleChara) ? source : null);
		if (battleChara == null || battleChara.NameId != 7639)
		{
			return;
		}
		for (int i = 1; i < 14; i++)
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "general_x02f",
				radiusX = 6f,
				radiusZ = 50f,
				fixRotation = true,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31632u },
					TargetHitCount = i
				}
			};
			switch (icon)
			{
			default:
				return;
			case 156u:
				drawElement.refRotation = source.Rotation.Radians() - (9 * i).Degrees();
				break;
			case 157u:
				drawElement.refRotation = source.Rotation.Radians() + (9 * i).Degrees();
				break;
			}
			DrawManager.Draw(drawElement, source);
		}
	}
}
