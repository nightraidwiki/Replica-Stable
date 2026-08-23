using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Replica.QuickDraws;

namespace Replica.Windows;

public sealed class LibraryView
{
	private readonly Plugin _plugin;

	private uint _zone;

	private string _zoneSearch = "";

	private string _entrySearch = "";

	private bool _showCasts = true;

	private bool _showStatus = true;

	private bool _showMarkers = true;

	private bool _showTethers = true;

	private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new Dictionary<uint, ISharedImmediateTexture>();

	public LibraryView(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		FightCatalog catalog = _plugin.Catalog;
		ImGui.TextDisabled("Fights fill in as you play or replay them. Pick a duty, then turn any cast or debuff into a quick draw.");
		ImGui.Separator();
		DrawZonePane(catalog);
		ImGui.SameLine();
		DrawEntryPane(catalog);
	}

	private void DrawZonePane(FightCatalog cat)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		if (!ImGui.BeginChild("##libzones", new Vector2(240f * globalScale, 0f), border: true))
		{
			ImGui.EndChild();
			return;
		}
		ImGui.SetNextItemWidth(-1f);
		ImGui.InputTextWithHint("##zq", "filter duties…", ref _zoneSearch, 64);
		List<uint> list = cat.Zones();
		if (list.Count == 0)
		{
			ImGui.TextWrapped("Nothing recorded yet. Run or replay a duty and it'll show up here.");
		}
		foreach (IGrouping<string, (uint, string, string)> item in (from t in list
			select (Terr: t, Name: ZoneLibrary.NameOf(t), Cat: ZoneLibrary.CategoryOf(t)) into z
			where string.IsNullOrWhiteSpace(_zoneSearch) || z.Name.Contains(_zoneSearch, StringComparison.OrdinalIgnoreCase)
			group z by z.Cat).OrderBy<IGrouping<string, (uint, string, string)>, string>((IGrouping<string, (uint Terr, string Name, string Cat)> g) => g.Key, StringComparer.OrdinalIgnoreCase))
		{
			ImU8String label = new ImU8String(6, 2);
			label.AppendFormatted(item.Key);
			label.AppendLiteral("###cat");
			label.AppendFormatted(item.Key);
			if (!ImGui.CollapsingHeader(label, ImGuiTreeNodeFlags.DefaultOpen))
			{
				continue;
			}
			foreach (var item2 in item.OrderBy<(uint, string, string), string>(((uint Terr, string Name, string Cat) z) => z.Name, StringComparer.OrdinalIgnoreCase))
			{
				ImU8String label2 = new ImU8String(7, 3);
				label2.AppendFormatted(item2.Item2);
				label2.AppendLiteral("  (");
				label2.AppendFormatted(cat.Count(item2.Item1));
				label2.AppendLiteral(")##z");
				label2.AppendFormatted(item2.Item1);
				if (ImGui.Selectable(label2, _zone == item2.Item1))
				{
					(_zone, _, _) = item2;
				}
			}
		}
		ImGui.EndChild();
	}

	private void DrawEntryPane(FightCatalog cat)
	{
		if (!ImGui.BeginChild("##libentries", new Vector2(0f, 0f)))
		{
			ImGui.EndChild();
			return;
		}
		if (_zone == 0)
		{
			ImGui.TextDisabled("Select a duty on the left.");
			ImGui.EndChild();
			return;
		}
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Gold, ZoneLibrary.NameOf(_zone));
		ImGui.SameLine();
		if (ImGui.SmallButton("Clear this duty"))
		{
			cat.Clear(_zone);
		}
		ImGui.SetNextItemWidth(260f * ImGuiHelpers.GlobalScale);
		ImGui.InputTextWithHint("##eq", "search name or id…", ref _entrySearch, 64);
		ImGui.SameLine();
		ImGui.Checkbox("Casts", ref _showCasts);
		ImGui.SameLine();
		ImGui.Checkbox("Statuses", ref _showStatus);
		ImGui.SameLine();
		ImGui.Checkbox("Markers", ref _showMarkers);
		ImGui.SameLine();
		ImGui.Checkbox("Tethers", ref _showTethers);
		List<FightCatalog.Entry> all = (from e in cat.Entries(_zone)
			where string.IsNullOrWhiteSpace(_entrySearch) || e.Name.Contains(_entrySearch, StringComparison.OrdinalIgnoreCase) || e.Id.ToString().Contains(_entrySearch)
			select e).ToList();
		if (_showCasts)
		{
			DrawGroup("Casts", all, FightCatalog.Kind.Cast);
		}
		if (_showStatus)
		{
			DrawGroup("Statuses", all, FightCatalog.Kind.Status);
		}
		if (_showMarkers)
		{
			DrawGroup("Headmarkers", all, FightCatalog.Kind.Headmarker);
		}
		if (_showTethers)
		{
			DrawGroup("Tethers", all, FightCatalog.Kind.Tether);
		}
		ImGui.EndChild();
	}

	private void DrawGroup(string label, List<FightCatalog.Entry> all, FightCatalog.Kind kind)
	{
		List<FightCatalog.Entry> list = all.Where((FightCatalog.Entry e) => e.Kind == kind).OrderBy<FightCatalog.Entry, string>((FightCatalog.Entry e) => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
		if (list.Count == 0)
		{
			return;
		}
		ImU8String label2 = new ImU8String(10, 3);
		label2.AppendFormatted(label);
		label2.AppendLiteral("  (");
		label2.AppendFormatted(list.Count);
		label2.AppendLiteral(")###grp");
		label2.AppendFormatted(label);
		if (!ImGui.CollapsingHeader(label2, ImGuiTreeNodeFlags.DefaultOpen))
		{
			return;
		}
		foreach (FightCatalog.Entry item in list)
		{
			ImU8String strId = new ImU8String(0, 2);
			strId.AppendFormatted(label);
			strId.AppendFormatted(item.Id);
			ImGui.PushID(strId);
			DrawIcon(item.Icon, ImGui.GetFrameHeight() * 0.9f);
			ImGui.SameLine();
			ImGui.AlignTextToFramePadding();
			ImGui.TextUnformatted(item.Name);
			ImGui.SameLine();
			ImU8String text = new ImU8String(1, 1);
			text.AppendLiteral("#");
			text.AppendFormatted(item.Id);
			ImGui.TextColored(in Ui.Dimmed, text);
			float x = ImGui.GetContentRegionAvail().X;
			float num = 190f * ImGuiHelpers.GlobalScale;
			if (x > num)
			{
				ImGui.SameLine(ImGui.GetCursorPosX() + (x - num));
			}
			else
			{
				ImGui.SameLine();
			}
			if (ImGui.SmallButton("Make quick draw"))
			{
				_plugin.OpenQuickDrawForCatalog(item, _zone);
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Copy id"))
			{
				ImGui.SetClipboardText(item.Id.ToString());
			}
			ImGui.PopID();
		}
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
}
