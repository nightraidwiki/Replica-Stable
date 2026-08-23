using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.UWU;

public class GeocrushImpact : ISpecialAction
{
	public override string Name => "Geocrush";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11110u };

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.Circle(info.SourceId.GameObject(), 24f, 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 11110u }
		});
	}
}
