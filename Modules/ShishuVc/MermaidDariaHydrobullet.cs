using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.ShishuVc;

public class MermaidDariaHydrobullet : ISpecialAction
{
	public override string Name => "Mermaid Daria Hydrobullet";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon == 22 && TargetID != Svc.Objects.LocalPlayer?.GameObjectId)
		{
			SimpleElement.Circle(Source, 15f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 45848u }
			});
		}
	}
}
