using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M6S;

public class Highlightning : ISpecialAction
{
	private Vector3 lastPosition;

	public override string Name => "Highlightning";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 42651u };

	public override void Update()
	{
		if (aoes.Count == 0)
		{
			return;
		}
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18339);
		if (gameObject != null)
		{
			Vector2 vector;
			switch ((int)Angle.FromDirection(new WPos(gameObject.Position) - new WPos(lastPosition)).Deg)
			{
			case 0:
				return;
			case -150:
			case -149:
			case -90:
				vector = new Vector2(86.992f, 91.997f);
				break;
			case 90:
			case 146:
			case 147:
				vector = new Vector2(114.977f, 91.997f);
				break;
			case -35:
			case -34:
			case -33:
			case -32:
			case 28:
			case 29:
				vector = new Vector2(99.992f, 114.997f);
				break;
			default:
				vector = default(Vector2);
				break;
			}
			Vector2 vector2 = vector;
			if (vector2 != default(Vector2))
			{
				aoes[0].Position = new Vector3(vector2.X, 0f, vector2.Y);
			}
		}
	}

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 18339)
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = GameObject.Position,
				drawOnObject = false,
				radiusX = 21f,
				radiusZ = 21f,
				refColor = Vector4.One,
				refTargetColor = Vector4.One,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42651u },
					TargetHitCount = 5
				}
			};
			aoes.Add(DrawManager.Draw(element));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (base.NumCasts++ == 5)
		{
			aoes.Clear();
		}
		lastPosition = info.Source.Position;
	}
}
