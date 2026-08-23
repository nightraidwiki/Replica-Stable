using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M2S;

public class OuterstageCombo : ISpecialAction
{
	public override string Name => "Outerstage Combo";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37293u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject();
		if (gameObject != null)
		{
			SimpleElement.Circle(info.SourceId, 7f, 3000f, 0f, 37297u);
			SimpleElement.Fan(info.SourceId, 30f, 45, info.Facing + 45.Degrees(), 3000f, 0f, 37299u);
			SimpleElement.Fan(info.SourceId, 30f, 45, info.Facing + 135.Degrees(), 3000f, 0f, 37299u);
			SimpleElement.Fan(info.SourceId, 30f, 45, info.Facing + 225.Degrees(), 3000f, 0f, 37299u);
			SimpleElement.Fan(info.SourceId, 30f, 45, info.Facing + 315.Degrees(), 3000f, 0f, 37299u);
			SimpleElement.Cross(info.SourceId, 30f, 7f, default(Angle), 3000f, 6000f, 37298u);
			SimpleElement.Donut(info.SourceId, 7f, 30f, 3000f, 9000f, 37300u);
			SimpleElement.Fan(gameObject, 30f, 45, info.Facing + 0.Degrees(), 3000f, 9000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 37299u },
				TargetHitCount = 8
			});
			SimpleElement.Fan(gameObject, 30f, 45, info.Facing + 90.Degrees(), 3000f, 9000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 37299u },
				TargetHitCount = 8
			});
			SimpleElement.Fan(gameObject, 30f, 45, info.Facing + 180.Degrees(), 3000f, 9000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 37299u },
				TargetHitCount = 8
			});
			SimpleElement.Fan(gameObject, 30f, 45, info.Facing + 270.Degrees(), 3000f, 9000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 37299u },
				TargetHitCount = 8
			});
		}
	}
}
