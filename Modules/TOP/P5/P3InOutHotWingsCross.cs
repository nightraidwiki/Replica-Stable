using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP.P5;

public class P3InOutHotWingsCross : ISpecialAction
{
	public override string Name => "P3 In/Out + Hot Wings Cross";

	public override uint Phase => 5u;

	public override uint WeatherID => 174u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 32789u, 32374u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 32789)
		{
			base.NumCasts = 0;
			base.CanDraw = true;
		}
		if (info.ActionId == 32374)
		{
			base.CanDraw = false;
		}
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (id != 7747 || !base.CanDraw || base.NumCasts == 5)
		{
			return;
		}
		base.NumCasts++;
		switch (source.BaseId)
		{
		case 15721u:
		{
			ICharacter character2 = (ICharacter)((source is ICharacter) ? source : null);
			if (character2 != null && character2.GetTransformationID() == 4)
			{
				DrawElement drawElement5 = new DrawElement
				{
					drawAvfx = "customDonut",
					refRadian = 0.25f,
					radiusX = 40f,
					radiusZ = 40f,
					drawOnObject = true,
					refColor = GroundOmen.enemyColor,
					refTargetColor = GroundOmen.enemyColor,
					destroyTime = 13100f
				};
				if (base.NumCasts > 3)
				{
					drawElement5.delayDrawTime = 9100f;
					drawElement5.destroyTime = 4100f;
				}
				DrawManager.Draw(drawElement5, source);
			}
			else
			{
				DrawElement drawElement6 = new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 10f,
					radiusZ = 10f,
					drawOnObject = true,
					destroyTime = 13100f
				};
				if (base.NumCasts > 3)
				{
					drawElement6.delayDrawTime = 9100f;
					drawElement6.destroyTime = 4100f;
				}
				DrawManager.Draw(drawElement6, source);
			}
			break;
		}
		case 15722u:
		{
			ICharacter character = (ICharacter)((source is ICharacter) ? source : null);
			if (character != null && character.GetTransformationID() == 4)
			{
				DrawElement drawElement = new DrawElement
				{
					drawAvfx = "general_x02f",
					radiusX = 18f,
					radiusZ = 80f,
					refOffsetX = 22f,
					drawOnObject = true,
					destroyTime = 13100f
				};
				DrawElement drawElement2 = new DrawElement
				{
					drawAvfx = "general_x02f",
					radiusX = 18f,
					radiusZ = 80f,
					refOffsetX = -22f,
					drawOnObject = true,
					destroyTime = 13100f
				};
				if (base.NumCasts > 3)
				{
					drawElement.delayDrawTime = 9100f;
					drawElement.destroyTime = 4100f;
					drawElement2.delayDrawTime = 9100f;
					drawElement2.destroyTime = 4100f;
				}
				DrawManager.Draw(drawElement, source);
				DrawManager.Draw(drawElement2, source);
			}
			else
			{
				DrawElement drawElement3 = new DrawElement
				{
					drawAvfx = "general_x02f",
					radiusX = 5f,
					radiusZ = 100f,
					drawOnObject = true,
					destroyTime = 13100f
				};
				DrawElement drawElement4 = new DrawElement
				{
					drawAvfx = "general_x02f",
					radiusX = 5f,
					radiusZ = 100f,
					drawOnObject = true,
					refRotation = 90.Degrees(),
					destroyTime = 13100f
				};
				if (base.NumCasts > 3)
				{
					drawElement3.delayDrawTime = 9100f;
					drawElement3.destroyTime = 4100f;
					drawElement4.delayDrawTime = 9100f;
					drawElement4.destroyTime = 4100f;
				}
				DrawManager.Draw(drawElement3, source);
				DrawManager.Draw(drawElement4, source);
			}
			break;
		}
		}
	}
}
