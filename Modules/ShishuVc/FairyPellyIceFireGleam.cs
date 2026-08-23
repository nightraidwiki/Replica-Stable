using System;
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
using Replica.Engine.Vfx;

namespace Replica.Modules.ShishuVc;

public class FairyPellyIceFireGleam : ISpecialAction
{
	private IGameObject? ice;

	public override string Name => "Fairy Pelly Ice/Fire Gleam";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 45548u, 45504u, 45506u };

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			int count = aoes.Count;
			if (count == 0)
			{
				return Array.Empty<StaticVfx>();
			}
			long initTime = aoes[0].initTime;
			int i;
			for (i = 0; i < count && aoes[i].initTime - initTime < 1000; i++)
			{
			}
			return aoes.Slice(0, i);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 45548:
		{
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "general_x02f",
				Position = info.Pos,
				radiusX = 5f,
				radiusZ = 40f,
				drawOnObject = false,
				destroyTime = info.CastTime * 1000f
			};
			aoes.Add(DrawManager.Draw(drawElement));
			drawElement.refRotation += 90.Degrees();
			aoes.Add(DrawManager.Draw(drawElement));
			break;
		}
		case 45504:
		{
			IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 19059);
			if (info.Pos.AlmostEqual(gameObject.Position, 1f))
			{
				ice = info.SourceId.GameObject();
			}
			break;
		}
		case 45506:
			if (ice != null && info.SourceId == ice.GameObjectId)
			{
				DrawElement obj = new DrawElement
				{
					drawAvfx = "m0973_lzr_ice_o0e1",
					Position = ice.Position,
					radiusX = 5f,
					radiusZ = 40f,
					drawOnObject = false,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 45501u }
					}
				};
				DrawManager.Draw(obj);
				obj.refRotation += 90.Degrees();
				DrawManager.Draw(obj);
			}
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 45548 && aoes.Count > 1)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}

	public override void Reset()
	{
		ice = null;
		base.Reset();
	}
}
