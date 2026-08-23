using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P1;

public class WaveCannon : ISpecialAction
{
	public override string Name => "Wave Cannon";

	public override HashSet<uint> ActionID => new HashSet<uint> { 47764u };

	public override uint WeatherID => 77u;

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (base.NumCasts > 0)
		{
			return;
		}
		base.NumCasts++;
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			if (allPlayer != Svc.Objects.LocalPlayer)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					drawOnObject = false,
					Position = new Vector3(100f, 0f, 65f),
					radiusX = 3f,
					radiusZ = 100f,
					target = allPlayer,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47784u }
					}
				});
			}
		}
	}
}
