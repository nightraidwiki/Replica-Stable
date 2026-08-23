using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.M4S;

public class FulminousField : ISpecialAction
{
	private Angle rotation;

	public override string Name => "Fulminous Field";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 39117u, 37118u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 37118)
		{
			base.NumCasts = 0;
			rotation = info.Facing;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts++;
		if (base.NumCasts % 8 != 0 || base.NumCasts > 48)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			Angle angle = rotation + ((float)(45 * i) + (float)(base.NumCasts / 8) * 22.5f).Degrees();
			SimpleElement.Fan(info.Source, 30f, 30, angle, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 39117u, 37118u }
			});
		}
		if (base.NumCasts == 32)
		{
			SimpleElement.Circle(Svc.Objects.LocalPlayer, 6f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 39118u }
			});
		}
		if (base.NumCasts != 40)
		{
			return;
		}
		new TimeHelper(100L, delegate
		{
			IGameObject target = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 17322);
			IEnumerable<IGameObject> enumerable = PlayerHelper.AllPlayers.Where((IGameObject o) => o.HasStatus(2941u));
			foreach (IGameObject item in enumerable)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02pxf",
					radiusX = 5f,
					radiusZ = 40f,
					drawOnObject = true,
					target = item,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 39119u }
					}
				}, target);
			}
			if (!enumerable.Contains(Svc.Objects.LocalPlayer))
			{
				SimpleElement.ShowText("Step up to block!", RaptureAtkModule.TextGimmickHintStyle.Warning, 3);
			}
		});
	}
}
