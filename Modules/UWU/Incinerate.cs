using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.UWU;

public class Incinerate : ISpecialAction
{
	public override string Name => "Incinerate (cone)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnChatMessage(uint chatType, string content)
	{
		IGameObject target = Svc.Objects.Where((IGameObject o) => o.BaseId == 8730 && o.IsTargetable).FirstOrDefault();
		if (chatType == 68)
		{
			ReadOnlySeString text = Svc.Data.GetExcelSheet<InstanceContentTextData>().GetRow(2600u).Text;
			ReadOnlySeString readOnlySeString = text;
			if (content == readOnlySeString.ExtractText())
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan120_1bf",
					radiusX = 15f,
					radiusZ = 15f,
					drawOnObject = true,
					alwaysFaceCurrentTarget = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 11094u },
						TargetHitCount = 3
					}
				}, target);
			}
		}
	}
}
