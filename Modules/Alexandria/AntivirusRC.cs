using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.Alexandria;

public class AntivirusRC : ISpecialAction
{
	public override string Name => "Antivirus RC";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 36382u, 36383u };

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId - 16756 > 1)
		{
			return;
		}
		base.NumCasts++;
		if (base.NumCasts == 1)
		{
			switch (GameObject.BaseId)
			{
			case 16756u:
				SimpleElement.Donut(GameObject, 4f, 40f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 36382u }
				});
				break;
			case 16757u:
				SimpleElement.Cross(GameObject, 40f, 3f, GameObject.Rotation.Radians(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 36383u }
				});
				break;
			}
			return;
		}
		switch (GameObject.BaseId)
		{
		case 16756u:
			ActionQueue.Enqueue((new HashSet<uint> { 36382u, 36383u }, delegate
			{
				SimpleElement.Donut(GameObject, 4f, 40f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 36382u }
				});
			}));
			break;
		case 16757u:
			ActionQueue.Enqueue((new HashSet<uint> { 36382u, 36383u }, delegate
			{
				SimpleElement.Cross(GameObject, 40f, 3f, GameObject.Rotation.Radians(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 36383u }
				});
			}));
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts = 0;
	}
}
