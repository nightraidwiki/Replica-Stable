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

public class LandslideCone : ISpecialAction
{
	public override string Name => "Landslide (cone)";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnChatMessage(uint chatType, string content)
	{
		IGameObject target = Svc.Objects.Where((IGameObject o) => o.BaseId == 8727 && o.IsTargetable).FirstOrDefault();
		if (chatType == 68)
		{
			ReadOnlySeString text = Svc.Data.GetExcelSheet<InstanceContentTextData>().GetRow(3800u).Text;
			ReadOnlySeString readOnlySeString = text;
			if (content == readOnlySeString.ExtractText())
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan120_1bf",
					radiusX = 15.55f,
					radiusZ = 15.55f,
					drawOnObject = true,
					alwaysFaceCurrentTarget = true,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 11107u }
					}
				}, target);
			}
		}
	}
}
