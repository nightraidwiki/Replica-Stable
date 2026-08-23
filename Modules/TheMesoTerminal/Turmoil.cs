using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TheMesoTerminal;

public class Turmoil : ISpecialAction
{
	public override string Name => "Turmoil";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43814u, 43815u };

	public override uint Phase => 3u;

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		uint actionId = info.ActionId;
		Vector3 position = info.Source.Position;
		SimpleElement.Rectangle(position + new Vector3(actionId switch
		{
			43814u => -1, 
			43815u => 1, 
			_ => 0, 
		} * 10, 0f, 0f), 40f, 10f, 0f, info.Source.Rotation.Radians(), 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 43816u }
		});
	}
}
