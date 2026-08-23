using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M11S;

public class FlameBreath : ISpecialAction
{
	public override string Name => "Flame Breath";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46144u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 46144)
		{
			base.CanDraw = true;
		}
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon == 244 && base.CanDraw && TargetID != Svc.Objects.LocalPlayer?.GameObjectId)
		{
			SimpleElement.RectangleToTarget(Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 19180), Source, 60f, 3f, 3000f, new HitCounter
			{
				ActionID = new HashSet<uint> { 46151u }
			});
		}
	}
}
