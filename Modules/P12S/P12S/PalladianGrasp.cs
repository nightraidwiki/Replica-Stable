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

namespace Replica.Modules.P12S.P12S;

public class PalladianGrasp : ISpecialAction
{
	public override string Name => "Palladian Grasp";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 33562u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 16181).TargetObject;
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_x02f",
			radiusX = 10f,
			radiusZ = 30f,
			drawOnObject = false,
			PositionCustomAction = () => (target.Position.X < 100f) ? new Vector3(90f, 0f, 95f) : new Vector3(110f, 0f, 95f),
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 33562u }
			}
		}, Svc.Objects.LocalPlayer);
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 16181).TargetObject;
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_x02f",
			radiusX = 10f,
			radiusZ = 30f,
			drawOnObject = false,
			PositionCustomAction = () => (target.Position.X < 100f) ? new Vector3(90f, 0f, 95f) : new Vector3(110f, 0f, 95f),
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 33563u }
			}
		}, Svc.Objects.LocalPlayer);
	}
}
