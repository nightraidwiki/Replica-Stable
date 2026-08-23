using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UWU;

public class RigidFeather : ISpecialAction
{
	public override string Name => "Rigid Feather";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject gameObject)
	{
		if (gameObject.BaseId == 8724)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 1.5f,
				radiusZ = 1.5f,
				drawOnObject = true,
				OnlyVisible = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 11143u }
				}
			}, gameObject);
		}
	}
}
