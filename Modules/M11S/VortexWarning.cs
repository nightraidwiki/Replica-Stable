using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M11S;

public class VortexWarning : ISpecialAction
{
	public override string Name => "Vortex Warning";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 19183)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "er_general_1f",
				radiusX = 5f,
				radiusZ = 5f,
				drawOnObject = true,
				OnlyVisible = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 46119u }
				}
			}, GameObject);
		}
	}
}
