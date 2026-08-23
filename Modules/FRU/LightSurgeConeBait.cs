using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class LightSurgeConeBait : ISpecialAction
{
	public override string Name => "Light Surge (cone bait)";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40187u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (!Svc.Objects.LocalPlayer.HasStatus(2253u))
		{
			SimpleElement.ShowText("No tether — bait cone up close", RaptureAtkModule.TextGimmickHintStyle.Warning, 8);
		}
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan060_1bf",
				radiusX = 60f,
				radiusZ = 60f,
				drawOnObject = true,
				target = allPlayer,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 40190u }
				},
				distanceCheck = new DistanceCheck
				{
					CheckObject = info.SourceId.GameObject(),
					CheckType = 0,
					Count = 4
				}
			}, info.SourceId.GameObject());
		}
	}
}
