using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace Replica.Windows;

public static class StratUI
{
	public static readonly (string Name, Vector4 Col)[] Swatches = new(string, Vector4)[8]
	{
		("White", new Vector4(1f, 1f, 1f, 1f)),
		("Orange", new Vector4(0.949f, 0.31f, 0.075f, 1f)),
		("Blue", new Vector4(0.27f, 0.55f, 1f, 1f)),
		("Red", new Vector4(0.95f, 0.25f, 0.25f, 1f)),
		("Green", new Vector4(0.2f, 0.9f, 0.35f, 1f)),
		("Yellow", new Vector4(1f, 0.85f, 0.15f, 1f)),
		("Cyan", new Vector4(0.2f, 0.85f, 0.9f, 1f)),
		("Purple", new Vector4(0.7f, 0.3f, 1f, 1f))
	};

	public static Vector4 SwatchColor(int index)
	{
		if (index < 0 || index >= Swatches.Length)
		{
			return Swatches[0].Col;
		}
		return Swatches[index].Col;
	}

	public static string SwatchName(int index)
	{
		if (index < 0 || index >= Swatches.Length)
		{
			return Swatches[0].Name;
		}
		return Swatches[index].Name;
	}

	public static float OptionColumn(params string[] labels)
	{
		float num = 0f;
		foreach (string text in labels)
		{
			num = MathF.Max(num, ImGui.CalcTextSize(text).X);
		}
		return ImGui.GetCursorPosX() + num + ImGui.GetStyle().ItemSpacing.X + 14f;
	}

	public static void OptionLabel(string label, float columnX, string? tooltip = null)
	{
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Dimmed, label);
		if (!string.IsNullOrEmpty(tooltip) && ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(tooltip);
		}
		ImGui.SameLine(columnX);
	}

	public static bool Header(string title, ref bool active)
	{
		ImGui.SetWindowFontScale(1.18f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Accent, title);
		ImGui.SetWindowFontScale(1f);
		float num = ImGui.GetFrameHeight() * 1.8f;
		string text = (active ? "ACTIVE" : "OFF");
		float x = ImGui.CalcTextSize(text).X;
		float x2 = ImGui.GetStyle().ItemSpacing.X;
		ImGui.SameLine();
		ImGui.SetCursorPosX(ImGui.GetWindowWidth() - num - x - x2 - 14f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(active ? Ui.Accent : Ui.Dimmed, text);
		ImGui.SameLine();
		bool result = Ui.ToggleSwitch("##active_" + title, ref active);
		ImGui.Separator();
		ImGui.Spacing();
		return result;
	}

	public static void Section(string label)
	{
		ImGui.Spacing();
		ImGui.PushStyleColor(ImGuiCol.Text, Ui.Accent);
		ImGui.TextUnformatted(label.ToUpperInvariant());
		ImGui.PopStyleColor();
	}

	public static void Hint(string text)
	{
		ImGui.TextDisabled(text);
	}

	public static bool SegmentedBar(string[] options, ref int selected)
	{
		bool result = false;
		for (int i = 0; i < options.Length; i++)
		{
			bool num = selected == i;
			if (num)
			{
				ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
				Vector4 accent = Ui.Accent;
				accent.W = 0.9f;
				ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accent);
				ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.08f, 0.06f, 0.05f, 1f));
			}
			if (ImGui.Button(options[i]))
			{
				if (selected != i)
				{
					result = true;
				}
				selected = i;
			}
			if (num)
			{
				ImGui.PopStyleColor(3);
			}
			if (i < options.Length - 1)
			{
				ImGui.SameLine();
			}
		}
		return result;
	}

	public static bool SegmentedBarWrapped(string[] options, ref int selected)
	{
		bool result = false;
		ImGuiStylePtr style = ImGui.GetStyle();
		float num = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
		for (int i = 0; i < options.Length; i++)
		{
			bool num2 = selected == i;
			if (num2)
			{
				ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
				Vector4 accent = Ui.Accent;
				accent.W = 0.9f;
				ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accent);
				ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.08f, 0.06f, 0.05f, 1f));
			}
			if (ImGui.Button(options[i]))
			{
				if (selected != i)
				{
					result = true;
				}
				selected = i;
			}
			if (num2)
			{
				ImGui.PopStyleColor(3);
			}
			if (i < options.Length - 1)
			{
				float x = ImGui.GetItemRectMax().X;
				float num3 = ImGui.CalcTextSize(options[i + 1]).X + style.FramePadding.X * 2f;
				if (x + style.ItemSpacing.X + num3 < num)
				{
					ImGui.SameLine();
				}
			}
		}
		return result;
	}

	public static bool RoleGrid(string[] roles, ref int selected, int columns = 2)
	{
		bool result = false;
		if (!ImGui.BeginTable("##rolegrid", columns, ImGuiTableFlags.SizingStretchSame))
		{
			return false;
		}
		for (int i = 0; i < roles.Length; i++)
		{
			ImGui.TableNextColumn();
			bool num = selected == i;
			if (num)
			{
				ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
				Vector4 accent = Ui.Accent;
				accent.W = 0.9f;
				ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accent);
				ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.08f, 0.06f, 0.05f, 1f));
			}
			if (ImGui.Button(roles[i], new Vector2(-1f, 0f)))
			{
				if (selected != i)
				{
					result = true;
				}
				selected = i;
			}
			if (num)
			{
				ImGui.PopStyleColor(3);
			}
		}
		ImGui.EndTable();
		return result;
	}

	public static bool ColorSwatches(ref int index)
	{
		bool result = false;
		float frameHeight = ImGui.GetFrameHeight();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		for (int i = 0; i < Swatches.Length; i++)
		{
			ImU8String descId = new ImU8String(4, 1);
			descId.AppendLiteral("##sw");
			descId.AppendFormatted(i);
			if (ImGui.ColorButton(descId, in Swatches[i].Col, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(frameHeight, frameHeight)))
			{
				if (index != i)
				{
					result = true;
				}
				index = i;
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip(Swatches[i].Name);
			}
			if (index == i)
			{
				Vector2 itemRectMin = ImGui.GetItemRectMin();
				Vector2 itemRectMax = ImGui.GetItemRectMax();
				windowDrawList.AddRect(itemRectMin - new Vector2(2f, 2f), itemRectMax + new Vector2(2f, 2f), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), 4f, ImDrawFlags.None, 2f);
			}
			if (i < Swatches.Length - 1)
			{
				ImGui.SameLine();
			}
		}
		return result;
	}

	public static bool PriorityList(string id, List<string> items)
	{
		bool result = false;
		float frameHeight = ImGui.GetFrameHeight();
		float x = ImGui.GetStyle().ItemSpacing.X;
		float offsetFromStartX = ImGui.GetWindowWidth() - frameHeight * 2f - x - 16f;
		for (int i = 0; i < items.Count; i++)
		{
			ImU8String strId = new ImU8String(1, 2);
			strId.AppendFormatted(id);
			strId.AppendLiteral("_");
			strId.AppendFormatted(i);
			ImGui.PushID(strId);
			ImGui.AlignTextToFramePadding();
			ImU8String text = new ImU8String(1, 1);
			text.AppendFormatted(i + 1);
			text.AppendLiteral(".");
			ImGui.TextColored(in Ui.Dimmed, text);
			ImGui.SameLine();
			ImGui.TextUnformatted(items[i]);
			ImGui.SameLine(offsetFromStartX);
			ImGui.PushFont(UiBuilder.IconFont);
			bool num = ImGui.Button(FontAwesomeIcon.ChevronUp.ToIconString() + "##u", new Vector2(frameHeight, frameHeight));
			ImGui.SameLine();
			bool flag = ImGui.Button(FontAwesomeIcon.ChevronDown.ToIconString() + "##d", new Vector2(frameHeight, frameHeight));
			ImGui.PopFont();
			if (num && i > 0)
			{
				List<string> list = items;
				int index = i - 1;
				int index2 = i;
				string value = items[i];
				string value2 = items[i - 1];
				list[index] = value;
				items[index2] = value2;
				result = true;
			}
			if (flag && i < items.Count - 1)
			{
				List<string> list = items;
				int index2 = i + 1;
				int index = i;
				string value2 = items[i];
				string value = items[i + 1];
				list[index2] = value2;
				items[index] = value;
				result = true;
			}
			ImGui.PopID();
		}
		return result;
	}
}
