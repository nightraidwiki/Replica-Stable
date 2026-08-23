using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace Replica.Windows;

public static class Ui
{
	public static readonly Vector4 Accent = new Vector4(0.843f, 0.247f, 0.29f, 1f);

	public static readonly Vector4 Blue = Accent;

	public static readonly Vector4 Gold = new Vector4(1f, 0.76f, 0.24f, 1f);

	public static readonly Vector4 Dimmed = new Vector4(0.58f, 0.56f, 0.55f, 1f);

	public static readonly Vector4 Green = new Vector4(0.36f, 0.82f, 0.45f, 1f);

	public static readonly Vector4 Red = new Vector4(0.96f, 0.42f, 0.42f, 1f);

	public static readonly Vector4 White = new Vector4(0.95f, 0.95f, 0.96f, 1f);

	public static readonly Vector4 DiscordColor = new Vector4(0.447f, 0.537f, 0.855f, 1f);

	public static readonly Vector4 AetherphoneColor = new Vector4(0.898f, 0.322f, 0.596f, 1f);

	private const int ThemeColors = 29;

	private const int ThemeVars = 9;

	public static void SectionHeader(FontAwesomeIcon icon, string text)
	{
		ImGui.AlignTextToFramePadding();
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.TextColored(in Accent, icon.ToIconString());
		ImGui.PopFont();
		ImGui.SameLine(0f, 7f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in White, text);
	}

	public static void PushTheme()
	{
		ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 9f);
		ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 7f);
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
		ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 6f);
		ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4f);
		ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 6f);
		ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 8f);
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 5f));
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12f, 10f));
		Col(ImGuiCol.Text, new Vector4(0.96f, 0.96f, 0.96f, 1f));
		Col(ImGuiCol.TextDisabled, new Vector4(0.55f, 0.55f, 0.55f, 1f));
		Col(ImGuiCol.WindowBg, new Vector4(0.082f, 0.082f, 0.082f, 0.94f));
		Col(ImGuiCol.ChildBg, new Vector4(0.12f, 0.12f, 0.12f, 0.45f));
		Col(ImGuiCol.PopupBg, new Vector4(0.1f, 0.1f, 0.1f, 0.96f));
		Col(ImGuiCol.Border, new Vector4(0.843f, 0.247f, 0.29f, 0.22f));
		Col(ImGuiCol.FrameBg, new Vector4(0.16f, 0.16f, 0.16f, 1f));
		Col(ImGuiCol.FrameBgHovered, new Vector4(0.22f, 0.205f, 0.207f, 1f));
		Col(ImGuiCol.FrameBgActive, new Vector4(0.28f, 0.255f, 0.258f, 1f));
		Col(ImGuiCol.TitleBg, new Vector4(0.1f, 0.1f, 0.1f, 1f));
		Col(ImGuiCol.TitleBgActive, new Vector4(0.22f, 0.08f, 0.1f, 1f));
		Col(ImGuiCol.TitleBgCollapsed, new Vector4(0.1f, 0.1f, 0.1f, 0.75f));
		Col(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 1f));
		Col(ImGuiCol.ButtonHovered, new Vector4(0.55f, 0.18f, 0.21f, 1f));
		Col(ImGuiCol.ButtonActive, new Vector4(0.843f, 0.247f, 0.29f, 1f));
		Col(ImGuiCol.Header, new Vector4(0.2f, 0.18f, 0.183f, 1f));
		Col(ImGuiCol.HeaderHovered, new Vector4(0.5f, 0.17f, 0.2f, 1f));
		Col(ImGuiCol.HeaderActive, new Vector4(0.78f, 0.235f, 0.275f, 1f));
		Col(ImGuiCol.CheckMark, new Vector4(0.95f, 0.35f, 0.38f, 1f));
		Col(ImGuiCol.SliderGrab, new Vector4(0.7f, 0.22f, 0.255f, 1f));
		Col(ImGuiCol.SliderGrabActive, new Vector4(0.92f, 0.3f, 0.34f, 1f));
		Col(ImGuiCol.Separator, new Vector4(0.24f, 0.24f, 0.24f, 1f));
		Col(ImGuiCol.SeparatorHovered, new Vector4(0.843f, 0.247f, 0.29f, 0.7f));
		Col(ImGuiCol.Tab, new Vector4(0.13f, 0.13f, 0.13f, 1f));
		Col(ImGuiCol.TabHovered, new Vector4(0.55f, 0.18f, 0.21f, 1f));
		Col(ImGuiCol.TabActive, new Vector4(0.32f, 0.11f, 0.13f, 1f));
		Col(ImGuiCol.ScrollbarBg, new Vector4(0.08f, 0.08f, 0.08f, 0.6f));
		Col(ImGuiCol.ScrollbarGrab, new Vector4(0.24f, 0.24f, 0.24f, 1f));
		Col(ImGuiCol.ScrollbarGrabHovered, new Vector4(0.55f, 0.18f, 0.21f, 1f));
	}

	public static void PopTheme()
	{
		ImGui.PopStyleColor(29);
		ImGui.PopStyleVar(9);
	}

	private static void Col(ImGuiCol idx, Vector4 c)
	{
		ImGui.PushStyleColor(idx, c);
	}

	public static void NavBar(Plugin plugin, string current)
	{
		if (current != "log")
		{
			if (ImGui.Button("Log"))
			{
				plugin.ToggleLog();
			}
			ImGui.SameLine();
		}
		if (current != "modules")
		{
			if (ImGui.Button("Modules"))
			{
				plugin.ShowTab("modules");
			}
			ImGui.SameLine();
		}
		if (ImGui.Button("Settings"))
		{
			plugin.OpenConfig();
		}
		ImGui.Separator();
	}

	public static bool ToggleSwitch(string id, ref bool value)
	{
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		float frameHeight = ImGui.GetFrameHeight();
		float num = frameHeight * 0.82f;
		float num2 = num * 1.8f;
		float num3 = num * 0.5f;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton(id, new Vector2(num2, frameHeight));
		bool result = false;
		if (ImGui.IsItemClicked())
		{
			value = !value;
			result = true;
		}
		bool flag = ImGui.IsItemHovered();
		float num4 = (frameHeight - num) * 0.5f;
		Vector2 pMin = new Vector2(cursorScreenPos.X, cursorScreenPos.Y + num4);
		Vector2 pMax = new Vector2(cursorScreenPos.X + num2, cursorScreenPos.Y + num4 + num);
		Vector4 accent = Accent;
		accent.W = (flag ? 1f : 0.92f);
		Vector4 vector = accent;
		Vector4 vector2 = new Vector4(0.32f, 0.32f, 0.32f, flag ? 1f : 0.9f);
		windowDrawList.AddRectFilled(pMin, pMax, ImGui.ColorConvertFloat4ToU32(value ? vector : vector2), num3);
		float x = (value ? (pMax.X - num3) : (pMin.X + num3));
		windowDrawList.AddCircleFilled(new Vector2(x, pMin.Y + num3), num3 - 2f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), 24);
		return result;
	}

	public static bool IconButton(FontAwesomeIcon icon, string label, string id, Vector2 size, float scale)
	{
		string iconStr = icon.ToIconString();
		
		ImGui.PushFont(UiBuilder.IconFont);
		Vector2 iconSize = ImGui.CalcTextSize(iconStr);
		ImGui.PopFont();
		
		Vector2 textSize = string.IsNullOrEmpty(label) ? Vector2.Zero : ImGui.CalcTextSize(label);
		float spacing = string.IsNullOrEmpty(label) ? 0f : 4f * scale;
		float totalContentWidth = iconSize.X + spacing + textSize.X;
		
		if (size.X <= 0f)
		{
			size.X = totalContentWidth + ImGui.GetStyle().FramePadding.X * 2f;
		}
		if (size.Y <= 0f)
		{
			size.Y = ImGui.GetFrameHeight();
		}
		
		Vector2 startPos = ImGui.GetCursorScreenPos();
		bool clicked = ImGui.Button($"###{id}", size);
		
		float contentX = startPos.X + (size.X - totalContentWidth) / 2f;
		float contentY = startPos.Y + (size.Y - System.MathF.Max(iconSize.Y, textSize.Y)) / 2f;
		
		uint textColor = ImGui.GetColorU32(ImGuiCol.Text);
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		
		ImGui.PushFont(UiBuilder.IconFont);
		drawList.AddText(new Vector2(contentX, contentY), textColor, iconStr);
		ImGui.PopFont();
		
		if (!string.IsNullOrEmpty(label))
		{
			drawList.AddText(new Vector2(contentX + iconSize.X + spacing, contentY), textColor, label);
		}
		
		return clicked;
	}
}
