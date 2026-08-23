using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Replica.QuickDraws;

namespace Replica.Windows;

public sealed class QuickDrawsView
{
	private readonly Plugin _plugin;

	private string _category = "Quick Draws";

	private string _status = "";

	private readonly HashSet<string> _expanded = new HashSet<string>();

	private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new Dictionary<uint, ISharedImmediateTexture>();

	private static readonly (string Cat, FontAwesomeIcon Icon)[] KnownCategories = new(string, FontAwesomeIcon)[10]
	{
		("Quick Draws", FontAwesomeIcon.Bolt),
		("General", FontAwesomeIcon.Star),
		("Personal", FontAwesomeIcon.User),
		("Dungeons", FontAwesomeIcon.Dungeon),
		("Trials", FontAwesomeIcon.Dragon),
		("Extreme", FontAwesomeIcon.Fire),
		("Savage", FontAwesomeIcon.Skull),
		("Ultimate", FontAwesomeIcon.Crown),
		("Alliance", FontAwesomeIcon.UserFriends),
		("Field Operations", FontAwesomeIcon.MapMarkedAlt)
	};

	public QuickDrawsView(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		Configuration configuration = _plugin.Configuration;
		bool value = configuration.QuickDrawsEnabled;
		if (Ui.ToggleSwitch("##qdmaster", ref value))
		{
			configuration.QuickDrawsEnabled = value;
			configuration.Save();
		}
		ImGui.SameLine(0f, 8f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(value ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, "Quick draws enabled");
		ImGui.SameLine(0f, 14f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Right-click a Fight Log line to make one. Shapes show on the floor when the event fires.");
		ImGui.SameLine();
		float x = ImGui.GetContentRegionAvail().X;
		float x2 = ImGui.GetStyle().ItemSpacing.X;
		float num = 110f * ImGuiHelpers.GlobalScale;
		float num2 = 110f * ImGuiHelpers.GlobalScale;
		float num3 = num + x2 + num2;
		if (x > num3)
		{
			ImGui.SameLine(ImGui.GetCursorPosX() + (x - num3));
		}
		if (ImGui.Button("Clear shapes"))
		{
			_plugin.Host.CleanVfx();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Wipe every shape currently drawn on the floor (same as /yyd cleanvfx).");
		}
		ImGui.SameLine();
		if (ImGui.Button("Import pack"))
		{
			ImportFromClipboard();
		}
		if (!string.IsNullOrEmpty(_status))
		{
			ImGui.TextDisabled(_status);
		}
		ImGui.Separator();
		DrawCategoryPane();
		ImGui.SameLine();
		DrawPackPane();
	}

	private IEnumerable<(string Cat, FontAwesomeIcon Icon)> AllCategories()
	{
		HashSet<string> known = KnownCategories.Select(((string Cat, FontAwesomeIcon Icon) k) => k.Cat).ToHashSet();
		IEnumerable<(string, FontAwesomeIcon)> second = from c in (from m in _plugin.Configuration.QuickDrawModules
				select m.Category into c
				where !string.IsNullOrEmpty(c) && !known.Contains(c)
				select c).Distinct()
			select (c: c, FolderOpen: FontAwesomeIcon.FolderOpen);
		return KnownCategories.Concat(second);
	}

	private void DrawCategoryPane()
	{
		Configuration configuration = _plugin.Configuration;
		float x = 196f * ImGuiHelpers.GlobalScale;
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8f, 7f));
		if (!ImGui.BeginChild("##qdcats", new Vector2(x, 0f), border: true))
		{
			ImGui.EndChild();
			ImGui.PopStyleVar();
			return;
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		foreach (var item2 in AllCategories())
		{
			string cat = item2.Cat;
			FontAwesomeIcon item = item2.Icon;
			int num = configuration.QuickDrawModules.Count((QuickDrawModule m) => m.Category == cat);
			bool flag = _category == cat;
			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			float frameHeight = ImGui.GetFrameHeight();
			float x2 = ImGui.GetContentRegionAvail().X;
			ImU8String label = new ImU8String(7, 1);
			label.AppendLiteral("##qdcat");
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
			windowDrawList.AddText(new Vector2(cursorScreenPos.X + 10f, y), ImGui.ColorConvertFloat4ToU32(input), item.ToIconString());
			ImGui.PopFont();
			Vector4 input2 = (flag ? new Vector4(1f, 1f, 1f, 1f) : ((num > 0) ? new Vector4(0.85f, 0.82f, 0.83f, 1f) : Ui.Dimmed));
			windowDrawList.AddText(new Vector2(cursorScreenPos.X + 34f, y), ImGui.ColorConvertFloat4ToU32(input2), cat);
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

	private void DrawPackPane()
	{
		Configuration configuration = _plugin.Configuration;
		if (!ImGui.BeginChild("##qdpacks", new Vector2(0f, 0f)))
		{
			ImGui.EndChild();
			return;
		}
		FontAwesomeIcon fontAwesomeIcon = AllCategories().FirstOrDefault<(string, FontAwesomeIcon)>(((string Cat, FontAwesomeIcon Icon) c) => c.Cat == _category).Item2;
		if (fontAwesomeIcon == FontAwesomeIcon.None)
		{
			fontAwesomeIcon = FontAwesomeIcon.FolderOpen;
		}
		ImGui.SetWindowFontScale(1.25f);
		ImGui.PushFont(UiBuilder.IconFont);
		ImGui.TextColored(in Ui.Accent, fontAwesomeIcon.ToIconString());
		ImGui.PopFont();
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), _category);
		ImGui.SetWindowFontScale(1f);
		List<QuickDrawModule> list = configuration.QuickDrawModules.Where((QuickDrawModule m) => m.Category == _category).ToList();
		int value = list.Sum((QuickDrawModule p) => p.Draws.Count);
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImU8String text = new ImU8String(18, 2);
		text.AppendLiteral("   ");
		text.AppendFormatted(list.Count);
		text.AppendLiteral(" packs · ");
		text.AppendFormatted(value);
		text.AppendLiteral(" draws");
		ImGui.TextColored(in Ui.Dimmed, text);
		float x = ImGui.GetContentRegionAvail().X;
		float num = 96f * ImGuiHelpers.GlobalScale;
		if (x > num + 40f)
		{
			ImGui.SameLine(ImGui.GetCursorPosX() + (x - num));
			if (ImGui.Button("+ New pack", new Vector2(num, 0f)))
			{
				QuickDrawModule quickDrawModule = new QuickDrawModule
				{
					Name = "New pack",
					Category = _category
				};
				configuration.QuickDrawModules.Add(quickDrawModule);
				_expanded.Add(quickDrawModule.Id);
				configuration.Save();
			}
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (list.Count == 0)
		{
			ImGui.Spacing();
			ImGui.TextColored(in Ui.Dimmed, "  No packs here yet. Add one, or right-click a Fight Log line to make a quick draw.");
			ImGui.EndChild();
			return;
		}
		QuickDrawModule removeModule = null;
		foreach (QuickDrawModule item in list)
		{
			DrawPackCard(configuration, item, ref removeModule);
		}
		if (removeModule != null)
		{
			configuration.QuickDrawModules.Remove(removeModule);
			configuration.Save();
		}
		ImGui.EndChild();
	}

	private void DrawPackCard(Configuration cfg, QuickDrawModule m, ref QuickDrawModule? removeModule)
	{
		ImGui.PushID(m.Id);
		bool flag = m.Enabled;
		bool flag2 = _expanded.Contains(m.Id);
		int num = m.Draws.Count((QuickDrawDef d) => d.Enabled);
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
		float y = cursorScreenPos.Y + (num2 - frameHeight) * 0.5f;
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X + 12f, y));
		bool value = flag;
		if (Ui.ToggleSwitch("##pen", ref value))
		{
			m.Enabled = value;
			cfg.Save();
			flag = value;
		}
		ImGui.SameLine(0f, 8f);
		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (frameHeight - ImGui.GetFrameHeight()) * 0.5f);
		if (ImGui.ArrowButton("##exp", (!flag2) ? ImGuiDir.Right : ImGuiDir.Down))
		{
			if (flag2)
			{
				_expanded.Remove(m.Id);
			}
			else
			{
				_expanded.Add(m.Id);
			}
			flag2 = !flag2;
		}
		ImGui.SameLine(0f, 10f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(flag ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, m.Name);
		ImGui.SameLine(0f, 8f);
		ImU8String text = new ImU8String(2, 1);
		text.AppendLiteral("(");
		text.AppendFormatted(m.Draws.Count);
		text.AppendLiteral(")");
		ImGui.TextColored(in Ui.Dimmed, text);
		float y2 = cursorScreenPos.Y + (num2 - ImGui.GetTextLineHeight()) * 0.5f;
		float num3 = vector.X - 12f;
		if (m.Draws.Count > 0)
		{
			string text2 = $"{num}/{m.Draws.Count}";
			float x2 = ImGui.CalcTextSize(text2).X;
			Vector4 vector2 = ((num == m.Draws.Count) ? Ui.Dimmed : ((num == 0) ? Ui.Red : Ui.Gold));
			windowDrawList.AddText(new Vector2(num3 - x2, y2), ImGui.ColorConvertFloat4ToU32(flag ? vector2 : Ui.Dimmed), text2);
		}
		ImGui.SetCursorScreenPos(new Vector2(cursorScreenPos.X, vector.Y + 3f));
		if (flag2)
		{
			ImGui.Indent(34f);
			DrawPackToolbar(m, ref removeModule);
			DrawPackDraws(m);
			ImGui.Unindent(34f);
			ImGui.Spacing();
		}
		ImGui.Dummy(new Vector2(0f, 5f));
		ImGui.PopID();
	}

	private void DrawPackToolbar(QuickDrawModule m, ref QuickDrawModule? removeModule)
	{
		Configuration configuration = _plugin.Configuration;
		ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.24f, 0.24f, 0.24f, 0.9f));
		Vector4 accent = Ui.Accent;
		accent.W = 0.55f;
		ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accent);
		if (ImGui.SmallButton("+ Draw"))
		{
			QuickDrawDef quickDrawDef = new QuickDrawDef();
			m.Draws.Add(quickDrawDef);
			configuration.Save();
			_plugin.OpenQuickDraw(quickDrawDef);
		}
		ImGui.SameLine();
		if (ImGui.SmallButton("Copy (share)"))
		{
			ExportToClipboard(m);
		}
		ImGui.PopStyleColor(2);
		ImGui.SameLine();
		ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
		string buf = m.Name;
		if (ImGui.InputText("##mname", ref buf, 64))
		{
			m.Name = buf;
			configuration.Save();
		}
		ImGui.SameLine();
		if (ImGui.SmallButton("Delete pack"))
		{
			removeModule = m;
		}
		ImGui.Spacing();
	}

	private void DrawPackDraws(QuickDrawModule m)
	{
		Configuration configuration = _plugin.Configuration;
		QuickDrawDef quickDrawDef = null;
		(QuickDrawDef, QuickDrawModule)? tuple = null;
		string text = null;
		foreach (QuickDrawDef item in m.Draws.OrderBy<QuickDrawDef, string>((QuickDrawDef quickDrawDef2) => quickDrawDef2.Group, StringComparer.OrdinalIgnoreCase))
		{
			if (!string.IsNullOrEmpty(item.Group) && item.Group != text)
			{
				ImGui.Spacing();
				ImGui.TextColored(in Ui.Gold, item.Group);
				text = item.Group;
			}
			ImGui.PushID(item.Id);
			bool value = item.Enabled;
			if (Ui.ToggleSwitch("##ten", ref value))
			{
				item.Enabled = value;
				configuration.Save();
			}
			ImGui.SameLine(0f, 8f);
			DrawIcon(item.IconId, ImGui.GetFrameHeight() * 0.9f);
			ImGui.SameLine();
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(value ? new Vector4(0.92f, 0.92f, 0.94f, 1f) : Ui.Dimmed, item.Name);
			ImGui.SameLine();
			DrawShapeChip(item.Draw);
			ImGui.SameLine();
			ImGui.TextColored(in Ui.Dimmed, Summary(item));
			float x = ImGui.GetContentRegionAvail().X;
			float num = 196f * ImGuiHelpers.GlobalScale;
			if (x > num)
			{
				ImGui.SameLine(ImGui.GetCursorPosX() + (x - num));
			}
			else
			{
				ImGui.SameLine();
			}
			if (ImGui.SmallButton("Edit"))
			{
				_plugin.OpenQuickDraw(item);
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Test"))
			{
				_plugin.Engine.Preview(item);
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Move"))
			{
				ImU8String strId = new ImU8String(4, 1);
				strId.AppendLiteral("move");
				strId.AppendFormatted(item.Id);
				ImGui.OpenPopup(strId);
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("X"))
			{
				quickDrawDef = item;
			}
			ImU8String strId2 = new ImU8String(4, 1);
			strId2.AppendLiteral("move");
			strId2.AppendFormatted(item.Id);
			if (ImGui.BeginPopup(strId2))
			{
				ImGui.TextDisabled("Move to pack");
				ImGui.Separator();
				foreach (QuickDrawModule item2 in configuration.QuickDrawModules.Where((QuickDrawModule quickDrawModule2) => quickDrawModule2 != m))
				{
					ImU8String label = new ImU8String(4, 2);
					label.AppendFormatted(item2.Name);
					label.AppendLiteral("  (");
					label.AppendFormatted(item2.Category);
					label.AppendLiteral(")");
					if (ImGui.MenuItem(label))
					{
						tuple = (item, item2);
					}
				}
				ImGui.Separator();
				if (ImGui.MenuItem("+ New pack here…"))
				{
					QuickDrawModule quickDrawModule = new QuickDrawModule
					{
						Name = "New pack",
						Category = _category
					};
					configuration.QuickDrawModules.Add(quickDrawModule);
					tuple = (item, quickDrawModule);
				}
				ImGui.EndPopup();
			}
			ImGui.PopID();
		}
		if (quickDrawDef != null)
		{
			m.Draws.Remove(quickDrawDef);
			configuration.Save();
		}
		if (tuple.HasValue)
		{
			(QuickDrawDef, QuickDrawModule) valueOrDefault = tuple.GetValueOrDefault();
			m.Draws.Remove(valueOrDefault.Item1);
			valueOrDefault.Item2.Draws.Add(valueOrDefault.Item1);
			configuration.Save();
		}
	}

	private static void DrawShapeChip(DrawSpec d)
	{
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float textLineHeight = ImGui.GetTextLineHeight();
		Vector4 color = d.Color;
		color.W = 1f;
		uint col = ImGui.ColorConvertFloat4ToU32(color);
		Vector2 vector = new Vector2(cursorScreenPos.X + textLineHeight * 0.5f, cursorScreenPos.Y + textLineHeight * 0.5f);
		switch (d.Shape)
		{
		case QuickShape.Circle:
			windowDrawList.AddCircle(vector, textLineHeight * 0.45f, col, 16, 2f);
			break;
		case QuickShape.Donut:
			windowDrawList.AddCircle(vector, textLineHeight * 0.45f, col, 16, 2f);
			windowDrawList.AddCircle(vector, textLineHeight * 0.22f, col, 12, 2f);
			break;
		case QuickShape.Fan:
			windowDrawList.PathArcTo(vector, textLineHeight * 0.45f, -2.4f, -0.7f, 12);
			windowDrawList.PathLineTo(vector);
			windowDrawList.PathStroke(col, ImDrawFlags.Closed, 2f);
			break;
		case QuickShape.Rectangle:
			windowDrawList.AddRect(new Vector2(cursorScreenPos.X + 1f, cursorScreenPos.Y + 2f), new Vector2(cursorScreenPos.X + textLineHeight - 1f, cursorScreenPos.Y + textLineHeight - 2f), col, 1f, ImDrawFlags.None, 2f);
			break;
		case QuickShape.Line:
			windowDrawList.AddLine(new Vector2(cursorScreenPos.X + 2f, cursorScreenPos.Y + textLineHeight - 2f), new Vector2(cursorScreenPos.X + textLineHeight - 2f, cursorScreenPos.Y + 2f), col, 2f);
			break;
		case QuickShape.Tower:
			windowDrawList.AddCircle(vector, textLineHeight * 0.42f, col, 16, 2f);
			windowDrawList.AddCircleFilled(vector, textLineHeight * 0.16f, col);
			break;
		case QuickShape.Knockback:
			windowDrawList.AddTriangleFilled(new Vector2(vector.X, cursorScreenPos.Y + 1f), new Vector2(cursorScreenPos.X + 1f, cursorScreenPos.Y + textLineHeight - 1f), new Vector2(cursorScreenPos.X + textLineHeight - 1f, cursorScreenPos.Y + textLineHeight - 1f), col);
			break;
		case QuickShape.Laser:
			windowDrawList.AddRectFilled(new Vector2(cursorScreenPos.X + textLineHeight * 0.35f, cursorScreenPos.Y + 2f), new Vector2(cursorScreenPos.X + textLineHeight - 1f, cursorScreenPos.Y + textLineHeight - 2f), col, 1f);
			windowDrawList.AddTriangleFilled(new Vector2(cursorScreenPos.X + 1f, cursorScreenPos.Y + textLineHeight * 0.5f), new Vector2(cursorScreenPos.X + textLineHeight * 0.35f, cursorScreenPos.Y + 2f), new Vector2(cursorScreenPos.X + textLineHeight * 0.35f, cursorScreenPos.Y + textLineHeight - 2f), col);
			break;
		}
		ImGui.Dummy(new Vector2(textLineHeight, textLineHeight));
	}

	private void DrawIcon(uint iconId, float size)
	{
		if (iconId == 0)
		{
			ImGui.Dummy(new Vector2(size, size));
			return;
		}
		if (!_iconCache.TryGetValue(iconId, out ISharedImmediateTexture value))
		{
			if (_iconCache.Count > 256)
			{
				_iconCache.Clear();
			}
			value = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId));
			_iconCache[iconId] = value;
		}
		IDalamudTextureWrap dalamudTextureWrap = value?.GetWrapOrDefault();
		if (dalamudTextureWrap != null)
		{
			ImGui.Image(dalamudTextureWrap.Handle, new Vector2(size, size));
		}
		else
		{
			ImGui.Dummy(new Vector2(size, size));
		}
	}

	private static string Summary(QuickDrawDef t)
	{
		string value = (t.MatchById ? $"#{t.DataId}" : (string.IsNullOrEmpty(t.Pattern) ? "any" : t.Pattern));
		return $"[{t.On}: {value}]";
	}

	private void ExportToClipboard(QuickDrawModule m)
	{
		try
		{
			QuickDrawModule value = new QuickDrawModule
			{
				Name = m.Name,
				Category = m.Category,
				Author = m.Author,
				Draws = m.Draws.Select((QuickDrawDef x) => x.Clone()).ToList()
			};
			ImGui.SetClipboardText(ShareCodec.Encode("YAPDRAWPACK1:", value));
			_status = "Share code copied";
		}
		catch (Exception ex)
		{
			_status = "Copy failed";
			Plugin.Log.Warning("[Replica] export: " + ex.Message);
		}
	}

	private void ImportFromClipboard()
	{
		string clipboardText = ImGui.GetClipboardText();
		QuickDrawModule value;
		QuickDrawDef value2;
		if (string.IsNullOrWhiteSpace(clipboardText))
		{
			_status = "Clipboard empty";
		}
		else if (ShareCodec.TryDecode<QuickDrawModule>("YAPDRAWPACK1:", clipboardText, out value) && value != null && value.Draws != null)
		{
			value.Id = Guid.NewGuid().ToString("N");
			value.BuiltIn = false;
			foreach (QuickDrawDef draw in value.Draws)
			{
				draw.Id = Guid.NewGuid().ToString("N");
			}
			if (string.IsNullOrWhiteSpace(value.Category))
			{
				value.Category = "General";
			}
			_plugin.Configuration.QuickDrawModules.Add(value);
			_plugin.Configuration.Save();
			_category = value.Category;
			_status = "Imported \"" + value.Name + "\"";
		}
		else if (ShareCodec.TryDecode<QuickDrawDef>("YAPDRAW1:", clipboardText, out value2) && value2 != null)
		{
			value2.Id = Guid.NewGuid().ToString("N");
			(_plugin.Configuration.QuickDrawModules.Find((QuickDrawModule x) => x.Category == _category) ?? _plugin.Configuration.QuickModule()).Draws.Add(value2);
			_plugin.Configuration.Save();
			_status = "Imported draw \"" + value2.Name + "\"";
		}
		else
		{
			_status = "Not a Replica code";
		}
	}
}
