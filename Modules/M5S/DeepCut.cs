using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.M5S;

public class DeepCut : ISpecialAction
{
	public override string Name => "DeepCut";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 471)
		{
			IGameObject? source = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18361);
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 42786u }
			};
			SimpleElement.FanToTarget(source, target, 60f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter);
		}
	}
}
