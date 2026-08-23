using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.DancingMad.P3;

public class ResoundingSlap : ISpecialAction
{
	private static readonly Vector3 Center = new Vector3(100f, 0f, 100f);

	public override string Name => "Resounding Slap";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47846u, 47847u, 47848u, 47849u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 47846 || info.ActionId == 47847)
		{
			bool num = info.ActionId == 47846;
			WDir wDir = (num ? (info.Facing.ToDirection().OrthoR() * 10f) : (info.Facing.ToDirection().OrthoL() * 10f));
			WDir wDir2 = (num ? wDir.OrthoR() : wDir.OrthoL());
			WDir wDir3 = (num ? wDir.OrthoL() : wDir.OrthoR());
			IGameObject target = info.SourceId.GameObject();
			DrawElement drawElement = new DrawElement
			{
				drawAvfx = "m0347_sircle_01m1",
				drawOnObject = false,
				radiusX = 13f,
				radiusZ = 13f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47848u },
					TargetHitCount = 3
				}
			};
			drawElement.Position = Center + (wDir + wDir2).ToVec3();
			aoes.Add(DrawManager.Draw(drawElement, target));
			drawElement.Position = Center + wDir.ToVec3();
			aoes.Add(DrawManager.Draw(drawElement, target));
			drawElement.Position = Center + (wDir + wDir3).ToVec3();
			aoes.Add(DrawManager.Draw(drawElement, target));
			aoes.Add(DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = Center,
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 6f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 47849u }
				}
			}));
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if ((info.ActionId == 47848 || info.ActionId == 47849) && aoes.Count > 0)
		{
			aoes[0]?.Remove();
			aoes.RemoveAt(0);
		}
	}
}
