using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.Valigarmanda;

public class ScourgeOfIce : ISpecialAction
{
	public override string Name => "Scourge of Ice";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon == 509)
		{
			uint weatherID = Events.WeatherID;
			if (weatherID == 15)
			{
				SimpleElement.Circle(target, 16f, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 36844u }
				});
			}
			if (weatherID == 14 && target.GameObjectId == Svc.Objects.LocalPlayer.GameObjectId)
			{
				SimpleElement.ShowText("Move now", RaptureAtkModule.TextGimmickHintStyle.Info, 7);
			}
			if (weatherID == 9 && target.GameObjectId == Svc.Objects.LocalPlayer.GameObjectId)
			{
				SimpleElement.ShowText("Float up", RaptureAtkModule.TextGimmickHintStyle.Info, 7);
			}
		}
	}
}
