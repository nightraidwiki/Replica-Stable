using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class EmptyDimension : ISpecialAction
{
	public override string Name => "Empty Dimension";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25306u };

	public override void OnActionCast(ActorCastInfo info)
	{
		IGameObject gameObject = Svc.Objects.SearchById(info.SourceId);
		if (gameObject != null)
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customDonut",
				refRadian = 3f / 35f,
				radiusX = 70f,
				radiusZ = 70f,
				drawOnObject = true,
				refColor = Vector4.One,
				refTargetColor = Vector4.One,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25306u }
				}
			}, gameObject);
		}
	}
}
