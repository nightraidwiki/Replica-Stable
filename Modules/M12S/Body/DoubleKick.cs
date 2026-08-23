using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M12S.Body;

public class DoubleKick : ISpecialAction
{
	public override string Name => "Double Kick";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46368u, 46373u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 46368)
		{
			IGameObject gameObject = info.SourceId.GameObject();
			if (gameObject != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "customFan",
					refRadian = 180f.Degrees().Rad,
					radiusX = 60f,
					radiusZ = 60f,
					drawOnObject = true,
					alwaysFaceCurrentTarget = true,
					refColor = GroundOmen.Red,
					refTargetColor = GroundOmen.Red,
					destroyTime = info.CastTime * 1000f
				}, gameObject);
			}
			return;
		}
		SimpleElement.Fan(info, 180);
		DrawElement element = new DrawElement
		{
			drawAvfx = "tank_lockonae_10m_7s_01w",
			drawType = ElementType.LockOn,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 46374u }
			}
		};
		foreach (IGameObject item in PlayerHelper.Tank)
		{
			DrawManager.Draw(element, item);
		}
	}
}
