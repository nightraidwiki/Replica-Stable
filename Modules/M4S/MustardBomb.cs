using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M4S;

public class MustardBomb : ISpecialAction
{
	public override string Name => "Mustard Bomb (tether spread)";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 38430u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in PlayerHelper.DPS.Union(PlayerHelper.Healer))
		{
			SimpleLockon.TarLockOn6m5s(item, (info.CastTime - 5f) * 1000f);
		}
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 6f,
			radiusZ = 6f,
			drawOnObject = true,
			TetherCheck = new TetherCheck
			{
				CheckType = 0,
				TetherID = new HashSet<int> { 283 }
			},
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 38432u },
				TargetHitCount = 2
			}
		}, PlayerHelper.AllPlayers);
	}
}
