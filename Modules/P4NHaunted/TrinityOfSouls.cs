using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.P4NHaunted;

public class TrinityOfSouls : ISpecialAction
{
	public override string Name => "Trinity of Souls";

	public override HashSet<uint> ActionID => new HashSet<uint> { 33473u, 33474u, 33475u, 33476u, 33477u, 33478u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 16163);
		switch (icon)
		{
		case 421u:
			aoes.Add(SimpleElement.Fan(gameObject, 60f, 180, gameObject.Rotation.Radians() + 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 33474u }
			}));
			break;
		case 422u:
			aoes.Add(SimpleElement.Fan(gameObject, 60f, 180, gameObject.Rotation.Radians() - 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 33473u }
			}));
			break;
		case 423u:
			aoes.Add(SimpleElement.Fan(gameObject, 60f, 180, gameObject.Rotation.Radians() + 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 33476u }
			}));
			break;
		case 424u:
			aoes.Add(SimpleElement.Fan(gameObject, 60f, 180, gameObject.Rotation.Radians() - 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 33475u }
			}));
			break;
		case 433u:
			aoes.Add(SimpleElement.Fan(gameObject, 60f, 180, gameObject.Rotation.Radians() + 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 33478u }
			}));
			break;
		case 434u:
			aoes.Add(SimpleElement.Fan(gameObject, 60f, 180, gameObject.Rotation.Radians() - 90.Degrees(), 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 33477u }
			}));
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes.RemoveAt(0);
		}
	}
}
