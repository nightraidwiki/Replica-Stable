using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class LashAndGnash : ISpecialAction
{
	private static readonly DrawElement Circle = new DrawElement
	{
		drawAvfx = "general_1bxf",
		radiusX = 8f,
		radiusZ = 8f,
		drawOnObject = true,
		hitCounter = new HitCounter
		{
			ActionID = new HashSet<uint> { 26389u }
		}
	};

	private static readonly DrawElement Donut = new DrawElement
	{
		drawAvfx = "customDonut",
		radiusX = 40f,
		radiusZ = 40f,
		refRadian = 0.2f,
		drawOnObject = true,
		refColor = GroundOmen.enemyColor,
		refTargetColor = GroundOmen.enemyColor,
		hitCounter = new HitCounter
		{
			ActionID = new HashSet<uint> { 26390u }
		}
	};

	public override string Name => "Lash and Gnash";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 26387u, 26390u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject != null && info.ActionId == 26387)
		{
			DrawQueue.Clear();
			DrawQueue.Enqueue((new HashSet<uint> { 26390u }, new(IGameObject, DrawElement[])[1] { (gameObject, new DrawElement[1] { Circle }) }));
			DrawManager.Draw(Donut, gameObject);
		}
	}
}
