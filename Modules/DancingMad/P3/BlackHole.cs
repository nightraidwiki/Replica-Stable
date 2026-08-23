using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P3;

public class BlackHole : ISpecialAction
{
	private readonly List<IGameObject> _added = new List<IGameObject>();

	public override string Name => "Black Hole";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void Update()
	{
		if (!Svc.Objects.Any((IGameObject o) => o.BaseId == 19512))
		{
			_added.Clear();
		}
	}

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 19512)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				radiusX = 2f,
				radiusZ = 2f,
				OnlyVisible = true,
				destroyTime = 60000f
			}, GameObject);
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
	{
		if (Id != 84)
		{
			return;
		}
		IGameObject gameObject = actorId.GameObject();
		if (gameObject == null || _added.Contains(gameObject))
		{
			return;
		}
		_added.Add(gameObject);
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general02xf",
				radiusX = 3f,
				radiusZ = 125f,
				target = allPlayer,
				TetherCheck = new TetherCheck
				{
					CheckType = 1,
					TetherID = new HashSet<int> { 84 }
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47868u },
					TargetHitCount = 24
				}
			}, gameObject);
		}
	}
}
