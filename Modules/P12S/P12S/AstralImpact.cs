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

namespace Replica.Modules.P12S.P12S;

public class AstralImpact : ISpecialAction
{
	public override string Name => "Astral Impact Sigil (buff)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3582)
		{
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 16171);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customRect",
				radiusX = 3f,
				radiusZ = 100f,
				drawOnObject = true,
				target = info.TargetID.GameObject(),
				refColor = new Vector4(0f, 0f, 0f, 1f),
				refTargetColor = new Vector4(0f, 0f, 0f, 1f),
				delayDrawTime = (info.Time - 5f) * 1000f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 33545u }
				}
			}, target);
		}
	}
}
