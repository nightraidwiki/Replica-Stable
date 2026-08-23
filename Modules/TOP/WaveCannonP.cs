using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TOP;

public class WaveCannonP : ISpecialAction
{
	public override string Name => "Wave Cannon P";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint WeatherID => 77u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 23)
		{
			SimpleElement.RectangleToTarget(Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 15708), target, 50f, 3f, 3000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 31505u }
			});
		}
	}
}
