using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CloudOfDarkness;

public class ChaosCondensedParticleBeam : ISpecialAction
{
	public override string Name => "Chaos-condensed Particle Beam (line stack)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40461u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in PlayerHelper.Tank)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02pxf",
				radiusX = 3f,
				radiusZ = 50f,
				drawOnObject = true,
				target = item,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40463u }
				}
			}, info.SourceId.GameObject());
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.LockOn,
				drawAvfx = "share_laser_8sec_0t"
			}, item, info.SourceId.GameObject());
		}
	}
}
