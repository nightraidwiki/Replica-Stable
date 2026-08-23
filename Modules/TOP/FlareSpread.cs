using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TOP;

public class FlareSpread : ISpecialAction
{
	public override string Name => "Flare (spread)";

	public override uint Phase => 6u;

	public override uint WeatherID => 175u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject source, uint icon, ulong TargetID)
	{
		if (icon == 346)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 20f,
				radiusZ = 20f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31668u },
					HitTarget = source
				}
			}, source);
		}
	}
}
