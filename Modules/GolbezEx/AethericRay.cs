using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.GolbezEx;

public class AethericRay : ISpecialAction
{
	public override string Name => "Aetheric Ray";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon == 412)
		{
			IGameObject? source = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 19329);
			IGameObject target = TargetID.GameObject();
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 45693u }
			};
			SimpleElement.FanToTarget(source, target, 50f, 30, Follow: true, default(Angle), 0f, 3000f, hitCounter);
		}
	}
}
