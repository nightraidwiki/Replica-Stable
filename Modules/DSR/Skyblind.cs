using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class Skyblind : ISpecialAction
{
	public override string Name => "Skyblind";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnRemoveStatus(ActorStatusChangeInfo info)
	{
		if (info.StatusID == 2661)
		{
			IGameObject gameObject = Svc.Objects.SearchById(info.TargetID);
			if (gameObject != null)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bxf",
					Position = gameObject.Position,
					drawOnObject = false,
					radiusX = 3f,
					radiusZ = 3f,
					destroyTime = 2500f
				}, gameObject);
			}
		}
	}
}
