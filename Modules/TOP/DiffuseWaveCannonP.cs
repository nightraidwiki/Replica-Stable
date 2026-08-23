using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TOP;

public class DiffuseWaveCannonP : ISpecialAction
{
	public override string Name => "Diffuse Wave Cannon P";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint WeatherID => 77u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon != 23 || base.NumCasts != 0)
		{
			return;
		}
		base.NumCasts++;
		IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 15708);
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan120_1bf",
				radiusX = 60f,
				radiusZ = 60f,
				drawOnObject = true,
				target = allPlayer,
				distanceCheck = new DistanceCheck
				{
					CheckObject = gameObject,
					CheckType = 1,
					Count = 2
				},
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 31504u },
					TargetHitCount = 10
				}
			}, gameObject);
		}
	}
}
