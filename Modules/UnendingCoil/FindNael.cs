using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.UnendingCoil;

public class FindNael : ISpecialAction
{
	public override string Name => "Blackfire Trio (find Nael)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9955u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.CanDraw = true;
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (base.CanDraw && source.BaseId == 8161 && id == 7747)
		{
			base.CanDraw = false;
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "e5d1_b1_kblaser_t1",
				Position = new Vector3(0f, 0f, 0f),
				drawOnObject = false,
				radiusX = 2f,
				radiusZ = 25f,
				target = source,
				endToTarget = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 9901u },
					TargetHitCount = 5
				}
			}, Svc.Objects.LocalPlayer);
		}
	}
}
