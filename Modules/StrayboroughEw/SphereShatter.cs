using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.StrayboroughEw;

public class SphereShatter : ISpecialAction
{
	public override string Name => "Raging Crystal";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 2u;

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 13296)
		{
			SimpleElement.Circle(GameObject, 15f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 25252u }
			});
		}
	}
}
