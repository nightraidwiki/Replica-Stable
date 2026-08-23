using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P1;

public class Overdrive : ISpecialAction
{
	private ulong _sourceId;

	public override string Name => "Overdrive";

	public override HashSet<uint> ActionID => new HashSet<uint> { 50722u };

	public override void Update()
	{
		if (aoes.Count != 0 && _sourceId != 0L)
		{
			IGameObject gameObject = _sourceId.GameObject()?.TargetObject;
			if (gameObject != null)
			{
				aoes[0].Enable = true;
				aoes[0].Owner = gameObject;
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		IGameObject gameObject = info.Source ?? Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 19504);
		if (gameObject != null)
		{
			IGameObject gameObject2 = gameObject.TargetObject ?? info.Target;
			if (gameObject2 != null)
			{
				_sourceId = gameObject.GameObjectId;
				aoes.Add(DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					radiusX = 5f,
					radiusZ = 5f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 49739u },
						TargetHitCount = 3
					}
				}, gameObject2));
			}
		}
	}

	public override void Reset()
	{
		_sourceId = 0uL;
		base.Reset();
	}
}
