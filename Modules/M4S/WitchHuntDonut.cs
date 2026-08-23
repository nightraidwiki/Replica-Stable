using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class WitchHuntDonut : ISpecialAction
{
	public override string Name => "Witch Hunt (donut)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38368u, 38369u };

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 38368:
			SimpleElement.Circle(info.SourceId, 10f, 3000f, 0f, 19729u);
			ActionQueue.Enqueue((new HashSet<uint> { 19729u, 19730u }, delegate
			{
				SimpleElement.Donut(info.SourceId, 10f, 60f, 3000f, 0f, 19730u);
			}));
			ActionQueue.Enqueue((new HashSet<uint> { 19729u, 19730u }, delegate
			{
				SimpleElement.Circle(info.SourceId, 10f, 3000f, 0f, 19729u);
			}));
			ActionQueue.Enqueue((new HashSet<uint> { 19729u, 19730u }, delegate
			{
				SimpleElement.Donut(info.SourceId, 10f, 60f, 3000f, 0f, 19730u);
			}));
			break;
		case 38369:
			SimpleElement.Donut(info.SourceId, 10f, 60f, 3000f, 0f, 19730u);
			ActionQueue.Enqueue((new HashSet<uint> { 19729u, 19730u }, delegate
			{
				SimpleElement.Circle(info.SourceId, 10f, 3000f, 0f, 19729u);
			}));
			ActionQueue.Enqueue((new HashSet<uint> { 19729u, 19730u }, delegate
			{
				SimpleElement.Donut(info.SourceId, 10f, 60f, 3000f, 0f, 19730u);
			}));
			ActionQueue.Enqueue((new HashSet<uint> { 19729u, 19730u }, delegate
			{
				SimpleElement.Circle(info.SourceId, 10f, 3000f, 0f, 19729u);
			}));
			break;
		}
	}
}
