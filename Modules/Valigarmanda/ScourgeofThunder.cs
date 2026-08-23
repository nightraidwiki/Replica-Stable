using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Valigarmanda;

public class ScourgeofThunder : ISpecialAction
{
	public override string Name => "Scourge of Thunder";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 0u };

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon != 507)
		{
			return;
		}
		uint weatherID = Events.WeatherID;
		if (weatherID == 14 || weatherID == 15)
		{
			SimpleElement.Circle(target, 5f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 36842u }
			});
		}
		if (weatherID == 9)
		{
			SimpleElement.Circle(target, 8f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 36841u }
			});
			if (target.GameObjectId == Svc.Objects.LocalPlayer.GameObjectId)
			{
				SimpleElement.ShowText("Don't float", RaptureAtkModule.TextGimmickHintStyle.Info, 7);
			}
		}
	}
}
