using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TEA;

public class HeartPath : ISpecialAction
{
	public override string Name => "Heart Path";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject gameObject)
	{
		if (gameObject.BaseId == 11345)
		{
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 11347);
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customRect",
				radiusX = 1f,
				radiusZ = 1f,
				drawOnObject = true,
				endToTarget = true,
				target = target,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 18524u }
				}
			}, gameObject);
		}
	}
}
