using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TOP;

public class WaveCannon : ISpecialAction
{
	public override string Name => "Wave Cannon";

	public override uint Phase => 6u;

	public override uint WeatherID => 175u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 31657u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject o) => o.ObjectKind == ObjectKind.Pc && o.GameObjectId != Player.Object.GameObjectId))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 4f,
				radiusZ = 100f,
				drawOnObject = true,
				target = item,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31659u },
					TargetHitCount = 8
				}
			}, info.SourceId.GameObject());
		}
	}
}
