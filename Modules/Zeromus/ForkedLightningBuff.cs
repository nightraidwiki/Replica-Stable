using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Zeromus;

public class ForkedLightningBuff : ISpecialAction
{
	public override string Name => "Forked Lightning (buff)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 3799)
		{
			IGameObject gameObject = Svc.Objects.SearchById(info.TargetID);
			if (gameObject != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawType = ElementType.Omen,
					drawAvfx = "general_1bxf",
					radiusX = 5f,
					radiusZ = 5f,
					drawOnObject = true,
					delayDrawTime = 71000f,
					destroyTime = 5000f
				}, gameObject);
			}
		}
	}
}
