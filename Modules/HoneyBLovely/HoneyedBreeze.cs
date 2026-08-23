using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.HoneyBLovely;

public class HoneyedBreeze : ISpecialAction
{
	public override string Name => "Honeyed Breeze";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 230)
		{
			IGameObject gameObject = Svc.Objects.Where((IGameObject o) => o.BaseId == 16938 && o.IsTargetable).FirstOrDefault();
			if (gameObject != null)
			{
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 37224u }
				};
				SimpleElement.FanToTarget(gameObject, target, 40f, 30, Follow: true, default(Angle), 0f, 3000f, hitCounter);
			}
		}
	}
}
