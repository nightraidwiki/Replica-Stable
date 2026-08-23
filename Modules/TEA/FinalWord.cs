using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class FinalWord : ISpecialAction
{
	public override string Name => "Final Word";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18557u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		new TimeHelper(1000L, delegate
		{
			if (Svc.Objects.LocalPlayer.StatusList.Any((IStatus status) => status.StatusId == 2153))
			{
				DrawElement element = new DrawElement
				{
					drawAvfx = "share_trap01k1",
					Position = new Vector3(100f, 0f, 85f),
					drawOnObject = false,
					radiusX = 1f,
					radiusY = 5f,
					radiusZ = 1f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 19039u, 19040u, 18560u, 18561u }
					}
				};
				DrawElement element2 = new DrawElement
				{
					drawAvfx = "customRect",
					Position = new Vector3(100f, 0f, 85f),
					drawOnObject = false,
					radiusX = 1f,
					target = Svc.Objects.LocalPlayer,
					endToTarget = true,
					refColor = new Vector4(1f, 1f, 0f, 1f),
					refTargetColor = new Vector4(1f, 1f, 0f, 1f),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 19039u, 19040u, 18560u, 18561u }
					}
				};
				DrawManager.Draw(element, Svc.Objects.LocalPlayer);
				DrawManager.Draw(element2, Svc.Objects.LocalPlayer);
			}
			else if (Svc.Objects.LocalPlayer.StatusList.Any((IStatus status) => status.StatusId == 2155))
			{
				DrawElement element3 = new DrawElement
				{
					drawAvfx = "share_trap01k1",
					Position = new Vector3(100f, 0f, 113f),
					drawOnObject = false,
					radiusX = 1f,
					radiusY = 5f,
					radiusZ = 1f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 19039u, 19040u, 18560u, 18561u }
					}
				};
				DrawElement element4 = new DrawElement
				{
					drawAvfx = "customRect",
					Position = new Vector3(100f, 0f, 113f),
					drawOnObject = false,
					radiusX = 1f,
					target = Svc.Objects.LocalPlayer,
					endToTarget = true,
					refColor = new Vector4(1f, 1f, 0f, 1f),
					refTargetColor = new Vector4(1f, 1f, 0f, 1f),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 19039u, 19040u, 18560u, 18561u }
					}
				};
				DrawManager.Draw(element3, Svc.Objects.LocalPlayer);
				DrawManager.Draw(element4, Svc.Objects.LocalPlayer);
			}
			else
			{
				DrawElement element5 = new DrawElement
				{
					drawAvfx = "share_trap01k1",
					Position = new Vector3(100f, 0f, 110f),
					drawOnObject = false,
					radiusX = 1f,
					radiusY = 5f,
					radiusZ = 1f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 19039u, 19040u, 18560u, 18561u }
					}
				};
				DrawElement element6 = new DrawElement
				{
					drawAvfx = "customRect",
					Position = new Vector3(100f, 0f, 110f),
					drawOnObject = false,
					radiusX = 1f,
					target = Svc.Objects.LocalPlayer,
					endToTarget = true,
					refColor = new Vector4(1f, 1f, 0f, 1f),
					refTargetColor = new Vector4(1f, 1f, 0f, 1f),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 19039u, 19040u, 18560u, 18561u }
					}
				};
				DrawManager.Draw(element5, Svc.Objects.LocalPlayer);
				DrawManager.Draw(element6, Svc.Objects.LocalPlayer);
			}
		});
	}
}
