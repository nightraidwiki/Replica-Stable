using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.UnendingCoil;

public class GrandEarthquake : ISpecialAction
{
	public override string Name => "Grand Earthquake";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 9946u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(4);

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 40)
		{
			IGameObject target2 = Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 8168);
			DrawElement element = new DrawElement
			{
				Enable = false,
				drawAvfx = "gl_fan090_1bf",
				radiusX = 60f,
				radiusZ = 60f,
				drawOnObject = true,
				target = target,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 9946u },
					TargetHitCount = 8
				}
			};
			aoes.Add(DrawManager.Draw(element, target2));
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
}
