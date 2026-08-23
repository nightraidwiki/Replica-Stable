using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class MistralLine : ISpecialAction
{
	public override string Name => "Mistral (line)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40158u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId + 1);
		if (gameObject != null)
		{
			Angle rotation = gameObject.Rotation.Radians();
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40157u }
			};
			SimpleElement.Rectangle(gameObject, 50f, 8f, 0f, null, rotation, 3000f, 0f, hitCounter);
		}
	}
}
