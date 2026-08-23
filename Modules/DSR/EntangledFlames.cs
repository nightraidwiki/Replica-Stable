using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class EntangledFlames : ISpecialAction
{
	public override string Name => "Entangled Flames (buff)";

	public override uint Phase => 6u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2759)
		{
			IGameObject gameObject = Svc.Objects.SearchById(info.TargetID);
			if (gameObject != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "2tagup_3m_6s_x",
					drawType = ElementType.LockOn,
					drawOnObject = true,
					delayDrawTime = (int)(info.Time - 8f) * 1000
				}, gameObject);
			}
		}
	}
}
