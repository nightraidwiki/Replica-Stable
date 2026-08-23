using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class SwordQuiver : ISpecialAction
{
	public override string Name => "Sword Quiver";

	public override uint Phase => 9u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38393u, 38394u, 38395u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Rectangle(new Vector3(100f, 0f, 165f), 30f, 6f, 30f, 0.Degrees(), 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 38400u }
		});
		SimpleElement.Rectangle(new Vector3(100f, 0f, 165f), 30f, 6f, 30f, 118.Degrees(), 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 38400u }
		});
		SimpleElement.Rectangle(new Vector3(100f, 0f, 165f), 30f, 6f, 30f, -118.Degrees(), 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 38400u }
		});
		SimpleElement.Rectangle(new Vector3(100f, 0f, 165 + info.ActionId switch
		{
			38393 => -10, 
			38395 => 10, 
			_ => 0, 
		}), 30f, 6f, 30f, 90.Degrees(), 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 38400u }
		});
	}
}
