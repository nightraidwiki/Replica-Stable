using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.DancingGreen;

public class DeepCut : ISpecialAction
{
	public override string Name => "Deep Cut";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint WeatherID => 2u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18358);
		if (icon == 471 && gameObject != null)
		{
			HitCounter hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 42694u }
			};
			SimpleElement.FanToTarget(gameObject, target, 60f, 45, Follow: true, default(Angle), 0f, 3000f, hitCounter);
		}
	}
}
