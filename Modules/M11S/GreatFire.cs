using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.M11S;

public class GreatFire : ISpecialAction
{
	public override string Name => "Great Fire";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46138u };

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon == 525)
		{
			SimpleElement.RectangleToTarget(Source, TargetID.GameObject(), 60f, 3f, 3000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46138u }
			});
		}
	}
}
