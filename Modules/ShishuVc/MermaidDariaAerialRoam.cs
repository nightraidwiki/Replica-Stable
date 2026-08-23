using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.ShishuVc;

public class MermaidDariaAerialRoam : ISpecialAction
{
	public override string Name => "Mermaid Daria Aerial Roam";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 2015003)
		{
			SimpleElement.Circle(GameObject.Position, 12f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 45846u }
			});
		}
	}
}
