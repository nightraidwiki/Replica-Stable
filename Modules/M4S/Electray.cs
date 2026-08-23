using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class Electray : ISpecialAction
{
	public override string Name => "Electray";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 17324)
		{
			Angle rotation = GameObject.Rotation.Radians();
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38379u }
			};
			SimpleElement.Rectangle(GameObject, 40f, 2.5f, 0f, null, rotation, 3000f, 0f, hitCounter);
		}
	}
}
