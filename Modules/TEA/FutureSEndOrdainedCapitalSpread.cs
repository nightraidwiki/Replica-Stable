using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.Memory;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class FutureSEndOrdainedCapitalSpread : ISpecialAction
{
	public override string Name => "Future's End α + Ordained Capital (spread)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18596u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IEnumerable<IGameObject> source = Svc.Objects.Where((IGameObject o) => o.BaseId == 11350);
		IGameObject cloestfade = source.MinBy((IGameObject o) => (o.Position - info.Source.Position).LengthSquared());
		IGameObject target = Data.TetherPlayer.FirstOrDefault((TetherInfo tether) => cloestfade.GameObjectId == tether.To).From.GameObject();
		DrawManager.Draw(new DrawElement
		{
			drawAvfx = "general_1bxf",
			radiusX = 30f,
			radiusZ = 30f,
			drawOnObject = true,
			delayDrawTime = 6000f,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18528u }
			}
		}, target);
	}
}
