using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DSR;

public class BroadSwing : ISpecialAction
{
	private static readonly DrawElement Left = new DrawElement
	{
		drawAvfx = "gl_fan120_1bf",
		radiusX = 40f,
		radiusZ = 40f,
		drawOnObject = true,
		refRotation = 60.Degrees(),
		hitCounter = new HitCounter
		{
			ActionID = new HashSet<uint> { 25538u }
		}
	};

	private static readonly DrawElement Right = new DrawElement
	{
		drawAvfx = "gl_fan120_1bf",
		radiusX = 40f,
		radiusZ = 40f,
		drawOnObject = true,
		refRotation = -60.Degrees(),
		hitCounter = new HitCounter
		{
			ActionID = new HashSet<uint> { 25538u }
		}
	};

	private static readonly DrawElement Back = new DrawElement
	{
		drawAvfx = "gl_fan120_1bf",
		radiusX = 40f,
		radiusZ = 40f,
		drawOnObject = true,
		refRotation = 180.Degrees(),
		hitCounter = new HitCounter
		{
			ActionID = new HashSet<uint> { 25538u }
		}
	};

	public override string Name => "Broad Swing";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25536u, 25537u, 25538u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = info.SourceId.GameObject();
		if (gameObject != null)
		{
			if (info.ActionId == 25536)
			{
				DrawManager.Draw(Right, gameObject);
				DrawQueue.Clear();
				DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (gameObject, new DrawElement[1] { Left }) }));
				DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (gameObject, new DrawElement[1] { Back }) }));
			}
			if (info.ActionId == 25537)
			{
				DrawManager.Draw(Left, gameObject);
				DrawQueue.Clear();
				DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (gameObject, new DrawElement[1] { Right }) }));
				DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (gameObject, new DrawElement[1] { Back }) }));
			}
		}
	}
}
