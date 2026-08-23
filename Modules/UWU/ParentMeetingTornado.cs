using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.UWU;

public class ParentMeetingTornado : ISpecialAction
{
	public override string Name => "Parent Meeting Tornado";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 11086u };

	public override uint WeatherID => 95u;

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.SourceId.GameObject().Position == new Vector3(100f, 0f, 100f))
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "customDonut",
				refRadian = 0.35f,
				radiusX = 20f,
				radiusZ = 20f,
				drawOnObject = true,
				refColor = GroundOmen.enemyColor,
				refTargetColor = GroundOmen.enemyColor,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 11087u }
				}
			};
			DrawQueue.Enqueue((new HashSet<uint> { 11086u }, new(IGameObject, DrawElement[])[1] { (info.SourceId.GameObject(), new DrawElement[1] { drawElement }) }));
		}
	}
}
