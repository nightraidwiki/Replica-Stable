using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.ForkedTower;

public class ElementalImpact : ISpecialAction
{
	public override string Name => "Elemental Impact";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42433u };

	public override uint Phase => 2u;

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 2014637)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0834cir_b_o0c",
				Position = GameObject.Position,
				drawOnObject = false,
				radiusX = 5f,
				radiusY = 7f,
				radiusZ = 5f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42432u }
				}
			});
		}
	}
}
