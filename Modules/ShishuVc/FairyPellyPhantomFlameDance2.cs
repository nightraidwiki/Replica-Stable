using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuVc;

public class FairyPellyPhantomFlameDance2 : ISpecialAction
{
	private readonly Dictionary<uint, bool> phantomCasts = new Dictionary<uint, bool>();

	public override string Name => "Fairy Pelly Phantom Flame Dance 2";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 45449u, 45452u };

	public override void OnActionCast(ActorCastInfo info)
	{
		phantomCasts[info.SourceId] = info.ActionId == 45452;
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon != 631)
		{
			return;
		}
		foreach (KeyValuePair<uint, bool> phantomCast in phantomCasts)
		{
			Actor actor = IGameObjectHelper.Find(phantomCast.Key);
			if (actor != null && actor.Tether.Target == TargetID)
			{
				DrawElement element = new DrawElement
				{
					drawAvfx = "general02xf",
					Position = phantomCast.Key.GameObject().Position,
					drawOnObject = false,
					target = TargetID.GameObject(),
					radiusX = 40f,
					radiusZ = 40f,
					refOffsetZ = -40f,
					refOffsetRotation = (phantomCasts[phantomCast.Key] ? 90.Degrees() : (-90.Degrees())),
					hitCounter = new HitCounter
					{
						ActionID = ActionID,
						TargetHitCount = 4
					}
				};
				aoes.Add(DrawManager.Draw(element));
				break;
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}

	public override void Reset()
	{
		phantomCasts.Clear();
		base.Reset();
	}
}
