using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M1S;

public class SplinteringNails : ISpecialAction
{
	private IGameObject? iconTarget;

	public override string Name => "Splintering Nails";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38041u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (iconTarget != Svc.Objects.LocalPlayer)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan045_1bf",
				radiusX = 100f,
				radiusZ = 100f,
				target = Svc.Objects.LocalPlayer,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 38042u }
				}
			}, info.SourceId.GameObject());
		}
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 538)
		{
			iconTarget = target;
		}
	}
}
