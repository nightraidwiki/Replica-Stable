using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class AlmightyJudgment : ISpecialAction
{
	public override string Name => "Almighty Judgment";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18574u, 18575u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 18575)
		{
			base.NumCasts++;
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "customCircle",
				Position = info.SourceId.GameObject().Position,
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 6f,
				refColor = new Vector4(1f, 1f, 0f, 1f),
				refTargetColor = new Vector4(1f, 1f, 0f, 1f)
			};
			if (base.NumCasts < 10)
			{
				drawElement.destroyTime = 8000f;
				DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer);
				return;
			}
			drawElement.destroyTime = 2000f;
			DrawQueue.Enqueue((new HashSet<uint> { 18576u }, new(IGameObject, DrawElement[])[1] { (Svc.Objects.LocalPlayer, new DrawElement[1] { drawElement }) }));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 18574)
		{
			base.NumCasts = 0;
		}
	}
}
