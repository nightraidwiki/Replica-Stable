using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class SearchlightWaveCannonBuff : ISpecialAction
{
	public override string Name => "Searchlight Wave Cannon (buff)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID - 3452 <= 1 && info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02pxf",
				radiusX = 50f,
				radiusZ = 50f,
				drawOnObject = true,
				refRotation = ((info.StatusID == 3452) ? (-90.Degrees()) : 90.Degrees()),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31597u }
				}
			}, info.TargetID.GameObject());
		}
	}
}
