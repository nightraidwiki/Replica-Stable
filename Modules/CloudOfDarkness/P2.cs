using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CloudOfDarkness;

public class P2 : ISpecialAction
{
	private enum Mechanic
	{
		None,
		Pull,
		Knockback
	}

	private Mechanic currentMechanic;

	public override string Name => "Pull / Knockback";

	public override uint WeatherID => 140u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40531u, 40532u, 40444u, 40446u, 40448u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 40531:
			SimpleElement.ShowText("Note pull-in");
			currentMechanic = Mechanic.Pull;
			break;
		case 40532:
			SimpleElement.ShowText("Note knockback");
			currentMechanic = Mechanic.Knockback;
			break;
		case 40444:
		case 40446:
		case 40448:
			switch (currentMechanic)
			{
			case Mechanic.Pull:
			{
				SimpleElement.ShowText("Pull-in soon");
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 15f,
					drawOnObject = true,
					KnockBackCheck = new KnockBackCheck
					{
						OriginPos = new Vector3(100f, 0f, 76.28425f),
						Reverse = true
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 40519u }
					}
				}, Svc.Objects.LocalPlayer);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = new Vector3(100f, 0f, 76.28425f),
					drawOnObject = false,
					radiusX = 6f,
					radiusZ = 6f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 40520u }
					}
				}, Svc.Objects.LocalPlayer);
				DrawElement drawElement = new DrawElement
				{
					drawAvfx = "customDonut",
					Position = new Vector3(100f, 0f, 76.28425f),
					drawOnObject = false,
					refRadian = 0.15f,
					radiusX = 40f,
					radiusZ = 40f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 40521u }
					},
					refColor = GroundOmen.enemyColor,
					refTargetColor = GroundOmen.enemyColor
				};
				DrawQueue.Enqueue((new HashSet<uint> { 40520u }, new(IGameObject, DrawElement[])[1] { (Svc.Objects.LocalPlayer, new DrawElement[1] { drawElement }) }));
				currentMechanic = Mechanic.None;
				break;
			}
			case Mechanic.Knockback:
				SimpleElement.ShowText("Knockback soon");
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 15f,
					drawOnObject = true,
					KnockBackCheck = new KnockBackCheck
					{
						OriginPos = new Vector3(100f, 0f, 76.28425f)
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 40525u }
					}
				}, Svc.Objects.LocalPlayer);
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = new Vector3(100f, 0f, 76.28425f),
					drawOnObject = false,
					radiusX = 8f,
					radiusZ = 8f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 40526u }
					}
				}, Svc.Objects.LocalPlayer);
				currentMechanic = Mechanic.None;
				break;
			case Mechanic.None:
				break;
			}
			break;
		}
	}

	public override void Reset()
	{
		currentMechanic = Mechanic.None;
		base.Reset();
	}
}
