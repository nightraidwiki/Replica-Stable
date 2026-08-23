using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Ui;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.FRU;

public class GroundAoE : ISpecialAction
{
	public override string Name => "Ground AoE";

	public override uint Phase => 5u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40118u, 40307u };

	public override bool HasConfig => true;

	public override void DrawConfig()
	{
		ImGui.SetNextItemWidth(300f);
		Vector4 col = Plugin.Config.FruP5HellfireColor;
		if (ImGui.ColorEdit4("Hellfire color", ref col))
		{
			Plugin.Config.FruP5HellfireColor = col;
			Plugin.Config.Save();
		}
		ImGui.SameLine();
		if (ImGui.Button("Preview color"))
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customRect",
				drawOnObject = true,
				radiusX = 40f,
				radiusZ = 5f,
				destroyTime = 3000f,
				refColor = Plugin.Config.FruP5HellfireColor,
				refTargetColor = Plugin.Config.FruP5HellfireColor
			}, Svc.Objects.LocalPlayer);
		}
		ImGui.SameLine();
		if (ImGuiUtil.IconButton(FontAwesomeIcon.Redo, "Reset###FruP5HellfireColor"))
		{
			Plugin.Config.FruP5HellfireColor = new Vector4(1f, 1f, 1f, 2f);
			Plugin.Config.Save();
		}
		ImGui.Text("If it looks faint, double-click the 4th (A) value to type a number above 255 for noticeably brighter drawing.");
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		SimpleElement.LineRect(info, 5f, 2000f, 8, ShowAll: false, Plugin.Config.FruP5HellfireColor);
	}
}
