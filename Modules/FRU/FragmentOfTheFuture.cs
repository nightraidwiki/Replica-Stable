using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.FRU;

public class FragmentOfTheFuture : ISpecialAction
{
	public override string Name => "Fragment of the Future";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 4u;

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 17841)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bzt",
				radiusX = 3.5f,
				radiusZ = 3.5f,
				drawOnObject = true,
				OnlyVisible = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 0u }
				}
			}, GameObject);
		}
	}

	public override void OnWeatherChange(uint oldWeatherID, uint newWeatherID)
	{
		if (newWeatherID - 105 <= 1)
		{
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17841);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bzt",
				radiusX = 3.5f,
				radiusZ = 3.5f,
				drawOnObject = true,
				OnlyVisible = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 0u }
				}
			}, target);
		}
	}
}
