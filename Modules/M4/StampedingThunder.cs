using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4;

public class StampedingThunder : ISpecialAction
{
	public override string Name => "Stampeding Thunder";

	public override HashSet<uint> ActionID => new HashSet<uint> { 37547u, 37548u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 37547)
		{
			SimpleElement.Rectangle(new Vector3(95f, 0f, 80f), 40f, 15f, 0f, info.Source.Rotation.Radians(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 36150u }
			});
		}
		else if (info.ActionId == 37548)
		{
			SimpleElement.Rectangle(new Vector3(105f, 0f, 80f), 40f, 15f, 0f, info.Source.Rotation.Radians(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 36150u }
			});
		}
	}
}
