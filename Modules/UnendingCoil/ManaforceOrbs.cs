using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UnendingCoil;

public class ManaforceOrbs : ISpecialAction
{
	public override string Name => "Manaforce (orbs)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 8160)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 8f,
				radiusZ = 8f,
				drawOnObject = true,
				OnlyVisible = true,
				destroyTime = 15000f
			}, GameObject);
		}
	}
}
