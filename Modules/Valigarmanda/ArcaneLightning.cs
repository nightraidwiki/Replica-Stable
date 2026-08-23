using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.Valigarmanda;

public class ArcaneLightning : ISpecialAction
{
	public override string Name => "Arcane Lightning";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 16770)
		{
			Angle rotation = GameObject.Rotation.Radians();
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 39002u }
			};
			SimpleElement.Rectangle(GameObject, 50f, 2.5f, 0f, null, rotation, 3000f, 0f, hitCounter);
		}
	}
}
