using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CloudOfDarkness;

public class Rapid_sequenceParticleBeam : ISpecialAction
{
	public override string Name => "Rapid-sequence Particle Beam (line stack)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40512u };

	public override uint Phase => 2u;

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general02pxf",
			radiusX = 3f,
			radiusZ = 50f,
			drawOnObject = true,
			target = Svc.Objects.LocalPlayer,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40514u },
				TargetHitCount = 12
			}
		}, info.SourceId.GameObject());
		DrawManager.Draw(new DrawElement
		{
			drawType = ElementType.LockOn,
			drawAvfx = "share_laser_8sec_0t"
		}, Svc.Objects.LocalPlayer, info.SourceId.GameObject());
	}
}
