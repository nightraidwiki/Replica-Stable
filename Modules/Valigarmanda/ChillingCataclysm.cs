using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.Valigarmanda;

public class ChillingCataclysm : ISpecialAction
{
	public override string Name => "Chilling Cataclysm";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 16667)
		{
			for (int i = 0; i < 7; i++)
			{
				Angle rotation = (i * 45).Degrees();
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 36803u }
				};
				SimpleElement.Rectangle(GameObject, 40f, 2.5f, 0f, null, rotation, 3000f, 0f, hitCounter);
			}
		}
	}
}
