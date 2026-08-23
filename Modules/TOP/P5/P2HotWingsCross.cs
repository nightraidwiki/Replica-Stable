using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop.Game;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP.P5;

public class P2HotWingsCross : ISpecialAction
{
	public override string Name => "P2 Hot Wings Cross";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31492u, 31493u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId - 31492 <= 1)
		{
			base.CanDraw = true;
		}
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (base.CanDraw && base.NumCasts <= 0 && source.BaseId == 15720 && id == 7747)
		{
			base.NumCasts++;
			base.CanDraw = false;
			switch (((ICharacter)source).GetTransformationID())
			{
			case 4:
				SimpleElement.HotWing(source, 80f, 18f, 22f, 3000f, 12000f, new HitCounter
				{
					ActionID = new HashSet<uint> { 31531u, 31532u }
				});
				break;
			case 11:
				SimpleElement.Cross(source, 100f, 5f, source.Rotation.Radians(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 31533u }
				});
				break;
			}
		}
	}
}
