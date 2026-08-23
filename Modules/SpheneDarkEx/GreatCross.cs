using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.SpheneDarkEx;

public class GreatCross : ISpecialAction
{
	public override string Name => "Great Cross";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public unsafe override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		IGameObject gameObject = actorId.GameObject();
		IGameObject gameObject2 = targetId.GameObject();
		uint baseId = gameObject.BaseId;
		Angle angle = baseId switch
		{
			18761u => 41f.Degrees(), 
			18708u => -153f.Degrees(), 
			_ => default(Angle), 
		};
		if (angle != default(Angle))
		{
			WPos wPos = new WPos(100f, 100f);
			SimpleElement.Rectangle(wPos.ToVec3(), 50f, 2f, 50f, Angle.FromDirection(wPos - new WPos(gameObject.Position)) + angle, (baseId == 18761) ? 7600 : 5600);
			Character* address = (Character*)gameObject.Address;
			address->GameObject.RenderFlags = (VisibilityFlags)((int)address->GameObject.RenderFlags | 2 | 0x800);
			Character* address2 = (Character*)gameObject2.Address;
			address2->GameObject.RenderFlags = (VisibilityFlags)((int)address2->GameObject.RenderFlags | 2 | 0x800);
		}
	}
}
