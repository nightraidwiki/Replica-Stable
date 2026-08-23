using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TEA;

public class FluidSwing : ISpecialAction
{
	public override string Name => "Fluid Swing";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject gameObject)
	{
		if (gameObject.BaseId == 11335)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan090_1bf",
				radiusX = 11.5f,
				radiusZ = 11.5f,
				drawOnObject = true,
				alwaysFaceCurrentTarget = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 18864u },
					TargetHitCount = 3
				}
			}, gameObject);
		}
	}
}
