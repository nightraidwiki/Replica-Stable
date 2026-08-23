using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.M11S;

public class GaleForce : ISpecialAction
{
	public override string Name => "Gale Force";

	public override HashSet<uint> ActionID => new HashSet<uint> { 46120u };

	public override void OnActionCast(ActorCastInfo info)
	{
		foreach (IGameObject item in Svc.Objects.Where((IGameObject x) => x.BaseId == 19183))
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan090_1bf",
					drawOnObject = true,
					target = allPlayer,
					radiusZ = 60f,
					radiusX = 60f,
					distanceCheck = new DistanceCheck
					{
						CheckType = 0,
						Count = 2,
						CheckObject = item
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 46119u }
					}
				}, item);
			}
		}
	}
}
