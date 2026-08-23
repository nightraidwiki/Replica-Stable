using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.CloudOfDarkness;

public class DiffusiveForceParticleBeam : ISpecialAction
{
	public override string Name => "Diffusive Force Particle Beam (spread)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40464u };

	public override uint Phase => 3u;

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				radiusX = 7f,
				radiusZ = 7f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40465u, 40466u }
				}
			}, allPlayer);
		}
	}
}
