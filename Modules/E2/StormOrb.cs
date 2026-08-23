using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.E2;

public class StormOrb : ISpecialAction
{
	public override string Name => "Storm Orb";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 11537)
		{
			SimpleElement.Circle(GameObject, 8f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 19426u }
			});
		}
	}
}
