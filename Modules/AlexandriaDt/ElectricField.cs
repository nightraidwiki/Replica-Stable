using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.AlexandriaDt;

public class ElectricField : ISpecialAction
{
	public override string Name => "Electric Field";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43261u };

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 586 && TargetID != Svc.Objects.LocalPlayer?.GameObjectId)
		{
			IGameObject target2 = TargetID.GameObject();
			HitCounter hitCounter = new HitCounter
			{
				ActionID = ActionID
			};
			SimpleElement.FanToTarget(target, target2, 26f, 50, Follow: true, default(Angle), 0f, 3000f, hitCounter);
		}
	}
}
