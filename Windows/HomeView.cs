using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Utility;

namespace Replica.Windows;

public sealed class HomeView
{
	private readonly Plugin _plugin;

	public HomeView(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		float x = ImGui.GetContentRegionAvail().X;
		ImGui.Dummy(new Vector2(0f, 10f * ImGuiHelpers.GlobalScale));
		IDalamudTextureWrap logo = Assets.Logo;
		if (logo != null)
		{
			float num = (float)logo.Height / (float)logo.Width;
			float num2 = MathF.Min(168f * ImGuiHelpers.GlobalScale, x * 0.42f);
			float num3 = num2 * num;
			Center(num2, x);
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			windowDrawList.AddRectFilled(cursorScreenPos - new Vector2(6f, 6f), cursorScreenPos + new Vector2(num2 + 6f, num3 + 6f), ImGui.ColorConvertFloat4ToU32(new Vector4(0.13f, 0.13f, 0.13f, 0.55f)), 14f);
			Vector2 pMin = cursorScreenPos - new Vector2(6f, 6f);
			Vector2 pMax = cursorScreenPos + new Vector2(num2 + 6f, num3 + 6f);
			Vector4 accent = Ui.Accent;
			accent.W = 0.35f;
			windowDrawList.AddRect(pMin, pMax, ImGui.ColorConvertFloat4ToU32(accent), 14f, ImDrawFlags.None, 1.5f);
			ImGui.Image(logo.Handle, new Vector2(num2, num3));
		}
		else
		{
			ImGui.Dummy(new Vector2(0f, 24f * ImGuiHelpers.GlobalScale));
		}
		ImGui.Dummy(new Vector2(0f, 6f * ImGuiHelpers.GlobalScale));
		CenterText("Replica", 1.9f, Ui.Blue, x);
		CenterText("Replicate few things", 1f, Ui.Dimmed, x);
		CenterText("v" + Changelog.Version, 0.95f, Ui.Dimmed, x);
		ImGui.Dummy(new Vector2(0f, 10f * ImGuiHelpers.GlobalScale));
		DrawChangelog(x);
		ImGui.Dummy(new Vector2(0f, 10f * ImGuiHelpers.GlobalScale));
		AccentRule(x);
		ImGui.Dummy(new Vector2(0f, 16f * ImGuiHelpers.GlobalScale));
		DrawFeatureToggles(x);
	}

	private void DrawFeatureToggles(float w)
	{
		float num = MathF.Min(560f * ImGuiHelpers.GlobalScale, w);
		Center(num, w);
		ImGui.BeginGroup();
		
		bool alwaysTrue = true;
		FeatureToggleCard(num, FontAwesomeIcon.Users, "Party Finder", "Browse ads, manage resumes & apply in 1-click. (Always active)", ref alwaysTrue, isLocked: true);
		ImGui.Dummy(new Vector2(0f, 6f * ImGuiHelpers.GlobalScale));
		
		bool modulesVal = _plugin.Configuration.ModulesEnabled;
		if (FeatureToggleCard(num, FontAwesomeIcon.LayerGroup, "Module Draw", "Draw BossMod reborn mechanic overlays on the arena floor.", ref modulesVal))
		{
			_plugin.Configuration.ModulesEnabled = modulesVal;
			_plugin.Configuration.Save();
		}
		ImGui.Dummy(new Vector2(0f, 6f * ImGuiHelpers.GlobalScale));
		
		bool bossmodVal = _plugin.Configuration.BossModMirrorEnabled;
		if (FeatureToggleCard(num, FontAwesomeIcon.Clone, "BossMod Mirror", "Project radar telegraphs directly into the 3D game world.", ref bossmodVal))
		{
			_plugin.Configuration.BossModMirrorEnabled = bossmodVal;
			_plugin.Configuration.Save();
		}
		ImGui.Dummy(new Vector2(0f, 6f * ImGuiHelpers.GlobalScale));
		
		bool hacksVal = _plugin.Configuration.HacksEnabled;
		if (FeatureToggleCard(num, FontAwesomeIcon.Bolt, "Hacks", "Custom battle and movement enhancements (restricted access).", ref hacksVal))
		{
			_plugin.Configuration.HacksEnabled = hacksVal;
			if (!hacksVal)
			{
				_plugin.Configuration.HacksUnlocked = false;
			}
			_plugin.Configuration.Save();
			_plugin.UpdateAllHackHookStates();
		}
		ImGui.Dummy(new Vector2(0f, 6f * ImGuiHelpers.GlobalScale));
		
		bool logsVal = _plugin.Configuration.LogsDataEnabled;
		if (FeatureToggleCard(num, FontAwesomeIcon.ListUl, "Logs Data", "Stream casts, tethers, view live map, and access duty library.", ref logsVal))
		{
			_plugin.Configuration.LogsDataEnabled = logsVal;
			_plugin.Configuration.Save();
		}
		
		ImGui.EndGroup();
	}

	private static bool FeatureToggleCard(float width, FontAwesomeIcon icon, string title, string desc, ref bool value, bool isLocked = false)
	{
		float num = ImGui.GetTextLineHeightWithSpacing() * 2.6f;
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		Vector2 vector = new Vector2(cursorScreenPos.X + width, cursorScreenPos.Y + num);
		
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		windowDrawList.AddRectFilled(
			cursorScreenPos, 
			vector, 
			ImGui.ColorConvertFloat4ToU32(isLocked ? new Vector4(0.11f, 0.11f, 0.11f, 1f) : new Vector4(0.13f, 0.13f, 0.13f, 1f)), 
			8f
		);
		
		Vector4 borderCol = new Vector4(0.3f, 0.3f, 0.3f, 0.45f);
		if (isLocked)
		{
			borderCol = new Vector4(0.2f, 0.2f, 0.2f, 0.3f);
		}
		else if (value)
		{
			Vector4 accent = Ui.Accent;
			accent.W = 0.5f;
			borderCol = accent;
		}
		windowDrawList.AddRect(cursorScreenPos, vector, ImGui.ColorConvertFloat4ToU32(borderCol), 8f, ImDrawFlags.None, 1f);
		
		float margin = 14f * ImGuiHelpers.GlobalScale;
		
		ImGui.PushFont(UiBuilder.IconFont);
		string iconStr = icon.ToIconString();
		Vector4 iconColor = isLocked ? Ui.Dimmed : (value ? Ui.Accent : Ui.Dimmed);
		windowDrawList.AddText(
			new Vector2(cursorScreenPos.X + margin, cursorScreenPos.Y + (num - ImGui.GetTextLineHeight()) * 0.5f), 
			ImGui.ColorConvertFloat4ToU32(iconColor), 
			iconStr
		);
		float iconWidth = ImGui.CalcTextSize(iconStr).X;
		ImGui.PopFont();
		
		float textX = cursorScreenPos.X + margin + iconWidth + 12f;
		windowDrawList.AddText(
			new Vector2(textX, cursorScreenPos.Y + 7f), 
			ImGui.ColorConvertFloat4ToU32(isLocked ? Ui.Dimmed : Ui.White), 
			title
		);
		windowDrawList.AddText(
			new Vector2(textX, cursorScreenPos.Y + 7f + ImGui.GetTextLineHeight() + 2f), 
			ImGui.ColorConvertFloat4ToU32(Ui.Dimmed), 
			desc
		);
		
		float frameHeight = ImGui.GetFrameHeight();
		float switchH = frameHeight * 0.82f;
		float switchW = switchH * 1.8f;
		
		float switchX = vector.X - margin - switchW;
		float switchY = cursorScreenPos.Y + (num - frameHeight) * 0.5f;
		
		ImGui.SetCursorScreenPos(new Vector2(switchX, switchY));
		
		bool dummy = value;
		bool clicked = false;
		if (isLocked)
		{
			ImGui.BeginDisabled();
			Ui.ToggleSwitch($"##switch_{title}", ref dummy);
			ImGui.EndDisabled();
		}
		else
		{
			if (Ui.ToggleSwitch($"##switch_{title}", ref value))
			{
				clicked = true;
			}
		}
		
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector.Y));
		
		return clicked;
	}

	private static void AccentRule(float avail)
	{
		ImGui.Dummy(new Vector2(0f, 8f * ImGuiHelpers.GlobalScale));
		float num = MathF.Min(220f * ImGuiHelpers.GlobalScale, avail * 0.5f);
		Center(num, avail);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float y = cursorScreenPos.Y;
		float num2 = 2f;
		uint num3 = ImGui.ColorConvertFloat4ToU32(Ui.Blue);
		Vector4 blue = Ui.Blue;
		blue.W = 0f;
		uint num4 = ImGui.ColorConvertFloat4ToU32(blue);
		float num5 = num * 0.5f;
		windowDrawList.AddRectFilledMultiColor(new Vector2(cursorScreenPos.X, y), new Vector2(cursorScreenPos.X + num5, y + num2), num4, num3, num3, num4);
		windowDrawList.AddRectFilledMultiColor(new Vector2(cursorScreenPos.X + num5, y), new Vector2(cursorScreenPos.X + num, y + num2), num3, num4, num4, num3);
		ImGui.Dummy(new Vector2(num, num2));
	}

	private static void DrawChangelog(float avail)
	{
		string[] notes = Changelog.Notes;
		if (notes == null || notes.Length == 0)
		{
			return;
		}

		float scale = ImGuiHelpers.GlobalScale;
		float width = MathF.Min(560f * scale, avail);
		Center(width, avail);

		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		float ySpacing = 6f * scale;

		ImGui.BeginGroup();

		ImGui.PushFont(UiBuilder.IconFont);
		string iconStr = FontAwesomeIcon.Bullhorn.ToIconString();
		float iconWidth = ImGui.CalcTextSize(iconStr).X;
		ImGui.PopFont();

		float titleWidth = ImGui.CalcTextSize("What's new").X;
		float totalHeaderWidth = iconWidth + 8f * scale + titleWidth;
		float headerOffset = (width - totalHeaderWidth) * 0.5f;
		if (headerOffset > 0f)
		{
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + headerOffset);
		}

		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.TextColored(in Ui.Accent, iconStr);
		ImGui.PopFont();
		ImGui.SameLine(0f, 8f * scale);
		ImGui.TextColored(in Ui.Accent, "What's new");
		ImGui.Dummy(new Vector2(0f, 6f * scale));

		float leftIndent = 24f * scale;
		foreach (string note in notes)
		{
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			float radius = 3.5f * scale;
			Vector2 center = new Vector2(cursorScreenPos.X + leftIndent, cursorScreenPos.Y + ImGui.GetTextLineHeight() * 0.5f);
			windowDrawList.AddCircleFilled(center, radius, ImGui.ColorConvertFloat4ToU32(Ui.Gold));

			ImGui.Indent(leftIndent + 12f * scale);
			ImGui.PushTextWrapPos(cursorScreenPos.X + width - 12f * scale);
			ImGui.TextColored(in Ui.White, note);
			ImGui.PopTextWrapPos();
			ImGui.Unindent(leftIndent + 12f * scale);
			ImGui.Dummy(new Vector2(0f, ySpacing));
		}

		ImGui.EndGroup();
	}

	private static void Center(float itemWidth, float avail)
	{
		float num = (avail - itemWidth) * 0.5f;
		if (num > 0f)
		{
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + num);
		}
	}

	private static void CenterText(string text, float scale, Vector4 color, float avail)
	{
		ImGui.SetWindowFontScale(scale);
		Center(ImGui.CalcTextSize(text).X, avail);
		ImGui.TextColored(in color, text);
		ImGui.SetWindowFontScale(1f);
	}
}
