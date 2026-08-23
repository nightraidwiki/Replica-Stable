using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CloudOfDarkness;

public class DarkEnergyParticleBeam : ISpecialAction
{
	public override string Name => "Dark Energy Particle Beam (laser)";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 3u;

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID != 2387)
		{
			return;
		}
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "gl_fan015_0x",
			radiusX = 25f,
			radiusZ = 25f,
			delayDrawTime = (info.Time - 4f) * 1000f,
			destroyTime = 4000f
		}, info.TargetID.GameObject());
		if (info.TargetID == Svc.Objects.LocalPlayer.GameObjectId)
		{
			new TimeHelper((long)((info.Time - 5f) * 1000f), delegate
			{
				SimpleElement.ShowText("Cone laser incoming");
			});
		}
	}
}
