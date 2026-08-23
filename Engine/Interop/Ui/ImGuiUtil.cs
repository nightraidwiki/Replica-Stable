using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace Replica.Engine.Interop.Ui;

internal static class ImGuiUtil
{
	public static bool IconButton(FontAwesomeIcon icon, string tooltip)
	{
		ImU8String label = new ImU8String(3, 2);
		label.AppendFormatted((char)icon);
		label.AppendLiteral("###");
		label.AppendFormatted(tooltip);
		bool result = ImGui.Button(label);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(tooltip);
		}
		return result;
	}
}
