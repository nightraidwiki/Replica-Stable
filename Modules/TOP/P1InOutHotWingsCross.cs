using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class P1InOutHotWingsCross : ISpecialAction
{
	public override string Name => "P1 In/Out + Hot Wings Cross";

	public override uint Phase => 2u;

	public override uint WeatherID => 78u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31550u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
		base.NumCasts = 0;
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (!base.CanDraw)
		{
			return;
		}
		IBattleChara battleChara = (IBattleChara)((source is IBattleChara) ? source : null);
		bool flag = battleChara != null;
		if (flag)
		{
			flag = battleChara.NameId - 7633 <= 1;
		}
		if (!flag || id != 7747 || base.NumCasts >= 2)
		{
			return;
		}
		base.NumCasts++;
		switch (source.BaseId)
		{
		case 15714u:
		{
			ICharacter character2 = (ICharacter)((source is ICharacter) ? source : null);
			if (character2 != null && character2.GetTransformationID() == 4)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_sicle_4010r1",
					radiusX = 40f,
					radiusZ = 40f,
					drawOnObject = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 31525u }
					}
				}, source);
			}
			else
			{
				SimpleElement.Circle(source, 10f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 31526u }
				});
			}
			break;
		}
		case 15715u:
		{
			ICharacter character = (ICharacter)((source is ICharacter) ? source : null);
			if (character != null && character.GetTransformationID() == 4)
			{
				SimpleElement.HotWing(source, 40f, 18f, 22f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 31531u, 31532u }
				});
			}
			else
			{
				SimpleElement.Cross(source, 100f, 5f, source.Rotation.Radians(), 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 31533u }
				});
			}
			break;
		}
		}
	}
}
