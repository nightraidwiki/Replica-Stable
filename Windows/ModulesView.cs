using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Replica.Engine;
using Replica.Engine.ModuleSetup;

namespace Replica.Windows;

public sealed class ModulesView
{
	private readonly Plugin _plugin;

	private Category _category = Category.Savage;

	private string _search = string.Empty;

	private readonly HashSet<string> _expanded = new HashSet<string>();

	private static readonly (Category Cat, string Label, FontAwesomeIcon Icon)[] KnownCategories = new(Category, string, FontAwesomeIcon)[13]
	{
		(Category.Dungeon, "Dungeons", FontAwesomeIcon.Dungeon),
		(Category.Trial, "Trials", FontAwesomeIcon.Dragon),
		(Category.Extreme, "Extreme", FontAwesomeIcon.Fire),
		(Category.Unreal, "Unreal", FontAwesomeIcon.Ghost),
		(Category.Raid, "Raids", FontAwesomeIcon.Users),
		(Category.Savage, "Savage", FontAwesomeIcon.Skull),
		(Category.Ultimate, "Ultimate", FontAwesomeIcon.Crown),
		(Category.Alliance, "Alliance", FontAwesomeIcon.UserFriends),
		(Category.Chaotic, "Chaotic", FontAwesomeIcon.Bolt),
		(Category.Foray, "Field Operations", FontAwesomeIcon.MapMarkedAlt),
		(Category.DeepDungeon, "Deep Dungeon", FontAwesomeIcon.LayerGroup),
		(Category.TreasureHunt, "Treasure Hunt", FontAwesomeIcon.Gem),
		(Category.VariantCriterion, "Variant", FontAwesomeIcon.Random)
	};

	private static readonly Dictionary<string, (string Move, string After)[]> MechMoves = new Dictionary<string, (string, string)[]> { ["Lindblum"] = new(string, string)[2]
	{
		("Replication 2 (Clones + Bait)", "Double Kick"),
		("Idyllic Dream (Uptime)", "Replication 2 (Clones + Bait)")
	} };

	private static IEnumerable<FightModuleHost.MechView> OrderMechs(string fightKey, IEnumerable<FightModuleHost.MechView> mechs)
	{
		List<FightModuleHost.MechView> list = mechs.ToList();
		if (!MechMoves.TryGetValue(fightKey, out (string, string)[] value))
		{
			return list;
		}
		(string, string)[] array = value;
		for (int i = 0; i < array.Length; i++)
		{
			(string, string) tuple = array[i];
			string move = tuple.Item1;
			string after = tuple.Item2;
			int num = list.FindIndex((FightModuleHost.MechView m) => m.Display == move);
			if (num >= 0)
			{
				FightModuleHost.MechView item = list[num];
				list.RemoveAt(num);
				int num2 = list.FindIndex((FightModuleHost.MechView m) => m.Display == after);
				list.Insert((num2 < 0) ? list.Count : (num2 + 1), item);
			}
		}
		return list;
	}

	public ModulesView(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		DrawCategoryPane();
		ImGui.SameLine();
		DrawFightPane();
	}

	private (string Label, FontAwesomeIcon Icon) Meta(Category cat)
	{
		(Category, string, FontAwesomeIcon)[] knownCategories = KnownCategories;
		for (int i = 0; i < knownCategories.Length; i++)
		{
			(Category, string, FontAwesomeIcon) tuple = knownCategories[i];
			if (tuple.Item1 == cat)
			{
				return (Label: tuple.Item2, Icon: tuple.Item3);
			}
		}
		return (Label: cat.ToString(), Icon: FontAwesomeIcon.FolderOpen);
	}

	private void DrawCategoryPane()
	{
		FightModuleHost host = _plugin.Host;
		float x = 196f * ImGuiHelpers.GlobalScale;
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8f, 7f));
		if (!ImGui.BeginChild("##cats", new Vector2(x, 0f), border: true))
		{
			ImGui.EndChild();
			ImGui.PopStyleVar();
			return;
		}
		IReadOnlyList<FightModuleHost.FightView> fights = host.Fights;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		(Category, string, FontAwesomeIcon)[] knownCategories = KnownCategories;
		for (int i = 0; i < knownCategories.Length; i++)
		{
			(Category, string, FontAwesomeIcon) tuple = knownCategories[i];
			Category cat = tuple.Item1;
			string item = tuple.Item2;
			FontAwesomeIcon item2 = tuple.Item3;
			int num = fights.Count((FightModuleHost.FightView f) => f.Category == cat);
			bool flag = _category == cat;
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			float frameHeight = ImGui.GetFrameHeight();
			float x2 = ImGui.GetContentRegionAvail().X;
			ImU8String label = new ImU8String(5, 1);
			label.AppendLiteral("##cat");
			label.AppendFormatted(cat);
			if (ImGui.Selectable(label, flag, ImGuiSelectableFlags.None, new Vector2(x2, frameHeight)))
			{
				_category = cat;
			}
			if (flag)
			{
				windowDrawList.AddRectFilled(cursorScreenPos, new Vector2(cursorScreenPos.X + 3f, cursorScreenPos.Y + frameHeight), ImGui.ColorConvertFloat4ToU32(Ui.Accent), 2f);
			}
			float y = cursorScreenPos.Y + (frameHeight - ImGui.GetTextLineHeight()) * 0.5f;
			Vector4 input = (flag ? Ui.Accent : ((num > 0) ? new Vector4(0.8f, 0.7f, 0.72f, 1f) : Ui.Dimmed));
			ImGui.PushFont(UiBuilder.IconFont);
			windowDrawList.AddText(new Vector2(cursorScreenPos.X + 10f, y), ImGui.ColorConvertFloat4ToU32(input), item2.ToIconString());
			ImGui.PopFont();
			Vector4 input2 = (flag ? new Vector4(1f, 1f, 1f, 1f) : ((num > 0) ? new Vector4(0.85f, 0.82f, 0.83f, 1f) : Ui.Dimmed));
			windowDrawList.AddText(new Vector2(cursorScreenPos.X + 34f, y), ImGui.ColorConvertFloat4ToU32(input2), item);
			if (num > 0)
			{
				string text = num.ToString();
				float x3 = ImGui.CalcTextSize(text).X;
				windowDrawList.AddText(new Vector2(cursorScreenPos.X + x2 - x3 - 6f, y), ImGui.ColorConvertFloat4ToU32(flag ? Ui.Accent : Ui.Dimmed), text);
			}
		}
		ImGui.EndChild();
		ImGui.PopStyleVar();
	}

	private void DrawFightPane()
	{
		Configuration configuration = _plugin.Configuration;
		FightModuleHost host = _plugin.Host;
		if (!ImGui.BeginChild("##fights", new Vector2(0f, 0f)))
		{
			ImGui.EndChild();
			return;
		}
		var (text, icon) = Meta(_category);
		ImGui.SetWindowFontScale(1.25f);
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.TextColored(in Ui.Accent, icon.ToIconString());
		ImGui.PopFont();
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), text);
		ImGui.SetWindowFontScale(1f);
		List<FightModuleHost.FightView> list = (from f in host.Fights
			where f.Category == _category
			orderby f.Cfc descending
			select f).ThenBy<FightModuleHost.FightView, string>((FightModuleHost.FightView f) => f.Display, StringComparer.OrdinalIgnoreCase).ToList();
		int value = list.Sum((FightModuleHost.FightView f) => f.Mechanics.Count);
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImU8String text2 = new ImU8String(23, 2);
		text2.AppendLiteral("   ");
		text2.AppendFormatted(list.Count);
		text2.AppendLiteral(" fights · ");
		text2.AppendFormatted(value);
		text2.AppendLiteral(" mechanics");
		ImGui.TextColored(in Ui.Dimmed, text2);
		float num = 220f * ImGuiHelpers.GlobalScale;
		float x = ImGui.GetContentRegionAvail().X;
		if (x > num + 40f)
		{
			ImGui.SameLine(ImGui.GetCursorPosX() + (x - num));
			ImGui.SetNextItemWidth(num);
			ImGui.InputTextWithHint("##search", "Search fights…", ref _search, 64);
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (!string.IsNullOrWhiteSpace(_search))
		{
			list = list.Where((FightModuleHost.FightView f) => f.Display.Contains(_search, StringComparison.OrdinalIgnoreCase)).ToList();
		}
		if (list.Count == 0)
		{
			ImGui.Spacing();
			ImGui.TextColored(in Ui.Dimmed, "  Nothing here yet.");
			ImGui.EndChild();
			return;
		}
		foreach (FightModuleHost.FightView item in list)
		{
			DrawFightCard(configuration, item);
		}
		ImGui.EndChild();
	}

	private void DrawFightCard(Configuration cfg, FightModuleHost.FightView f)
	{
		ImGui.PushID(f.Key);
		bool flag = !cfg.DisabledFights.Contains(f.Key);
		bool flag2 = _expanded.Contains(f.Key);
		int num = f.Mechanics.Count((FightModuleHost.MechView m) => !cfg.DisabledMechanics.Contains(f.Key + "/" + m.Key));
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float x = ImGui.GetContentRegionAvail().X;
		float frameHeight = ImGui.GetFrameHeight();
		float num2 = frameHeight + 12f;
		Vector2 vector = new Vector2(cursorScreenPos.X + x, cursorScreenPos.Y + num2);
		bool flag3 = ImGui.IsMouseHoveringRect(cursorScreenPos, vector);
		Vector4 input = (flag ? new Vector4(0.15f, 0.15f, 0.15f, 1f) : new Vector4(0.105f, 0.105f, 0.105f, 1f));
		if (flag3)
		{
			input += new Vector4(0.03f, 0.03f, 0.03f, 0f);
		}
		windowDrawList.AddRectFilled(cursorScreenPos, vector, ImGui.ColorConvertFloat4ToU32(input), 7f);
		Vector2 pMin = cursorScreenPos;
		Vector2 pMax = vector;
		Vector4 input2;
		if (!flag3)
		{
			input2 = new Vector4(0.3f, 0.3f, 0.3f, 0.45f);
		}
		else
		{
			Vector4 accent = Ui.Accent;
			accent.W = 0.5f;
			input2 = accent;
		}
		windowDrawList.AddRect(pMin, pMax, ImGui.ColorConvertFloat4ToU32(input2), 7f, ImDrawFlags.None, 1f);
		if (f.IsActive)
		{
			windowDrawList.AddRectFilled(cursorScreenPos, new Vector2(cursorScreenPos.X + 3.5f, vector.Y), ImGui.ColorConvertFloat4ToU32(Ui.Green), 2f);
		}
		float y = cursorScreenPos.Y + (num2 - frameHeight) * 0.5f;
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + 12f, y));
		bool value = flag;
		if (Ui.ToggleSwitch("##fen", ref value))
		{
			if (value)
			{
				cfg.DisabledFights.Remove(f.Key);
			}
			else
			{
				cfg.DisabledFights.Add(f.Key);
			}
			cfg.Save();
			flag = value;
		}
		ImGui.SameLine(0f, 8f);
		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (frameHeight - ImGui.GetFrameHeight()) * 0.5f);
		if (ImGui.ArrowButton("##exp", (!flag2) ? ImGuiDir.Right : ImGuiDir.Down))
		{
			if (flag2)
			{
				_expanded.Remove(f.Key);
			}
			else
			{
				_expanded.Add(f.Key);
			}
			flag2 = !flag2;
		}
		ImGui.SameLine(0f, 10f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(flag ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, f.Display);
		ImGui.SameLine(0f, 8f);
		if (f.Mechanics.Count > 0)
		{
			ImU8String text = new ImU8String(2, 1);
			text.AppendLiteral("(");
			text.AppendFormatted(f.Mechanics.Count);
			text.AppendLiteral(")");
			ImGui.TextColored(in Ui.Dimmed, text);
		}
		else if (f.UseAutoDraw)
		{
			ImGui.TextColored(in Ui.Dimmed, "(auto)");
		}
		Action drawConfig = f.DrawConfig;
		if (drawConfig != null || f.Mechanics.Any((FightModuleHost.MechView m) => m.HasConfig))
		{
			ImGui.SameLine(0f, 8f);
			ImGui.PushFont(UiBuilder.IconFont);
			ImGui.TextColored(flag ? Ui.Accent : Ui.Dimmed, FontAwesomeIcon.Cog.ToIconString());
			ImGui.PopFont();
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("Has options — expand the fight to see them.");
			}
		}
		float num3 = cursorScreenPos.Y + (num2 - ImGui.GetTextLineHeight()) * 0.5f;
		float num4 = vector.X - 12f;
		if (f.IsActive)
		{
			float x2 = ImGui.CalcTextSize("ACTIVE").X;
			float num5 = 7f;
			Vector2 pMax2 = new Vector2(num4, num3 + ImGui.GetTextLineHeight() * 0.5f + 9f);
			Vector2 pMin2 = new Vector2(num4 - x2 - num5 * 2f, num3 - 3f);
			windowDrawList.AddRectFilled(pMin2, pMax2, ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.52f, 0.3f, 0.85f)), 4f);
			windowDrawList.AddText(new Vector2(pMin2.X + num5, num3), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), "ACTIVE");
			num4 = pMin2.X - 10f;
		}
		if (f.Mechanics.Count > 0)
		{
			string text2 = $"{num}/{f.Mechanics.Count}";
			float x3 = ImGui.CalcTextSize(text2).X;
			Vector4 vector2 = ((num == f.Mechanics.Count) ? Ui.Dimmed : ((num == 0) ? Ui.Red : Ui.Gold));
			windowDrawList.AddText(new Vector2(num4 - x3, num3), ImGui.ColorConvertFloat4ToU32(flag ? vector2 : Ui.Dimmed), text2);
		}
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector.Y + 3f));
		if (flag2)
		{
			float x4 = cursorScreenPos.X + 22f;
			float y2 = ImGui.GetCursorScreenPos().Y + 2f;
			if (!flag)
			{
				ImGui.Indent(34f);
				ImGui.TextColored(in Ui.Dimmed, "Module is off — nothing will draw.");
				ImGui.Unindent(34f);
			}
			else if (f.Mechanics.Count == 0 && f.UseAutoDraw)
			{
				ImGui.Indent(34f);
				ImGui.TextColored(in Ui.Dimmed, "Auto-draw — telegraphs from game action data.");
				ImGui.Unindent(34f);
			}
			ImGui.Indent(34f);
			if ((drawConfig != null) & flag)
			{
				ImGui.Dummy(new Vector2(0f, 2f));
				ImGui.PushStyleColor(ImGuiCol.Separator, new Vector4(0.3f, 0.3f, 0.3f, 0.5f));
				ImGui.Separator();
				ImGui.PopStyleColor();
				ImGui.Dummy(new Vector2(0f, 3f));
				drawConfig();
				ImGui.Dummy(new Vector2(0f, 4f));
			}
			List<uint> list = (from p in f.Mechanics.Select((FightModuleHost.MechView m) => m.Phase).Distinct()
				orderby p
				select p).ToList();
			bool flag4 = list.Count > 1;
			int num6 = 0;
			bool flag5 = true;
			foreach (uint phase in list)
			{
				if (flag4)
				{
					if (!flag5)
					{
						ImGui.Dummy(new Vector2(0f, 4f));
					}
					ImGui.PushFont(UiBuilder.IconFont);
					ImGui.TextColored(in Ui.Accent, FontAwesomeIcon.CircleNotch.ToIconString());
					ImGui.PopFont();
					ImGui.SameLine(0f, 7f);
					ImGui.AlignTextToFramePadding();
					ImU8String text3 = new ImU8String(6, 1);
					text3.AppendLiteral("Phase ");
					text3.AppendFormatted(phase);
					ImGui.TextColored(in Ui.Accent, text3);
					ImGui.PushStyleColor(ImGuiCol.Separator, new Vector4(0.35f, 0.35f, 0.35f, 0.55f));
					ImGui.Separator();
					ImGui.PopStyleColor();
					ImGui.Dummy(new Vector2(0f, 2f));
				}
				flag5 = false;
				foreach (FightModuleHost.MechView item in OrderMechs(f.Key, f.Mechanics.Where((FightModuleHost.MechView m) => m.Phase == phase)))
				{
					ImGui.PushID(num6++);
					string enableKey = f.Key + "/" + item.Key;
					bool value2 = ModuleConfig.IsEnabled(enableKey);
					if (Ui.ToggleSwitch("##men", ref value2))
					{
						ModuleConfig.SetEnabled(enableKey, value2);
					}
					ImGui.SameLine(0f, 8f);
					ImGui.AlignTextToFramePadding();
					ImGui.TextColored((value2 & flag) ? new Vector4(0.92f, 0.92f, 0.94f, 1f) : Ui.Dimmed, item.Display);
					if (item.HasConfig && item.DrawConfig != null)
					{
						ImGui.SameLine(0f, 12f);
						ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.24f, 0.24f, 0.24f, 0.9f));
						Vector4 accent = Ui.Accent;
						accent.W = 0.55f;
						ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accent);
						ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 2f));
						Action drawConfig2 = item.DrawConfig;
						if (ImGui.SmallButton("Configure"))
						{
							_plugin.OpenModuleConfig(item.Display, drawConfig2);
						}
						ImGui.PopStyleVar();
						ImGui.PopStyleColor(2);
					}
					ImGui.PopID();
				}
			}
			ImGui.Unindent(34f);
			float y3 = ImGui.GetCursorScreenPos().Y - 2f;
			windowDrawList.AddLine(new Vector2(x4, y2), new Vector2(x4, y3), ImGui.ColorConvertFloat4ToU32(new Vector4(0.32f, 0.32f, 0.32f, 0.7f)), 1.5f);
			ImGui.Spacing();
		}
		ImGui.Dummy(new Vector2(0f, 5f));
		ImGui.PopID();
	}
}
