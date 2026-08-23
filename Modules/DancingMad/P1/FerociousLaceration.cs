using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P1;

public class FerociousLaceration : ISpecialAction
{
	private ulong _sourceId;

	private IGameObject? _target;

	private bool _secondTB;

	public override string Name => "Ferocious Laceration";

	public override HashSet<uint> ActionID => new HashSet<uint> { 50179u, 50401u };

	public override void Update()
	{
		if (aoes.Count == 0 || aoes[0] == null)
		{
			return;
		}
		IGameObject gameObject;
		if (_secondTB)
		{
			if (_sourceId == 0L)
			{
				return;
			}
			gameObject = PlayerHelper.RaidByEnmity(_sourceId).Skip(1).FirstOrDefault();
		}
		else
		{
			gameObject = _target;
		}
		if (gameObject != null)
		{
			aoes[0].Target = gameObject;
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		_sourceId = info.SourceId;
		_target = info.TargetId.GameObject();
		DrawElement element = new DrawElement
		{
			drawAvfx = "gl_fan120_1bf",
			radiusX = 100f,
			radiusZ = 100f,
			refColor = GroundOmen.Red,
			refTargetColor = GroundOmen.Red,
			target = info.TargetId.GameObject(),
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 50179u, 50401u },
				TargetHitCount = 2
			}
		};
		aoes.Add(DrawManager.Draw(element, info.SourceId.GameObject()));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 50179)
		{
			_secondTB = true;
		}
		if (info.ActionId == 50401)
		{
			_sourceId = 0uL;
			_secondTB = false;
		}
	}

	public override void Reset()
	{
		_sourceId = 0uL;
		_secondTB = false;
		base.Reset();
	}
}
