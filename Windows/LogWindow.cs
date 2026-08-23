using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Replica.Logging;
using Replica.QuickDraws;

namespace Replica.Windows;

public sealed class LogWindow : Window, IDisposable
{
	private enum SearchScope
	{
		Any,
		Source,
		Target,
		Ability
	}

	private readonly Plugin _plugin;

	private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new Dictionary<uint, ISharedImmediateTexture>();

	private string _search = "";

	private int _pullFilter;

	private string _exportStatus = "";

	private static readonly string[] SearchScopeNames = new string[4] { "any", "source", "target", "ability" };

	private SearchScope _searchScope;

	private uint _focusId;

	private string _focusName = "";

	private bool _paused;

	private bool _autoScroll = true;

	private bool _scrollToLatest;

	private int _prevFilteredCount = -1;

	private static readonly Vector4 ColCast = new Vector4(1f, 0.55f, 0.3f, 1f);

	private static readonly Vector4 ColUse = new Vector4(0.95f, 0.8f, 0.45f, 1f);

	private static readonly Vector4 ColGain = new Vector4(0.55f, 0.85f, 1f, 1f);

	private static readonly Vector4 ColLose = new Vector4(0.55f, 0.55f, 0.6f, 1f);

	private static readonly Vector4 ColDeath = new Vector4(1f, 0.35f, 0.35f, 1f);

	private static readonly Vector4 ColMarker = new Vector4(0.85f, 0.55f, 1f, 1f);

	private static readonly Vector4 ColDim = new Vector4(0.65f, 0.65f, 0.65f, 1f);

	private static readonly Vector4 ColEnemy = new Vector4(1f, 0.45f, 0.42f, 1f);

	private static readonly Vector4 ColYou = new Vector4(0.55f, 0.85f, 1f, 1f);

	private static readonly Vector4 ColParty = new Vector4(0.55f, 0.9f, 0.6f, 1f);

	private static readonly Vector4 ColId = new Vector4(0.6f, 0.7f, 0.85f, 1f);

	private static readonly Vector4 ColMap = new Vector4(0.45f, 0.9f, 0.8f, 1f);

	private static readonly Vector4 ColCtrl = new Vector4(0.7f, 0.7f, 0.78f, 1f);

	private static readonly Vector4 ColNote = new Vector4(1f, 0.85f, 0.4f, 1f);

	private static readonly Vector4 ColSize = new Vector4(0.55f, 0.85f, 0.75f, 1f);

	private readonly Dictionary<uint, ActionShape.Info?> _shapeCache = new Dictionary<uint, ActionShape.Info?>();

	private static readonly string[] CaptureNames = new string[4] { "Always (everything)", "Only in combat", "Only in a duty", "Disabled (no logging)" };

	private readonly List<LogEvent> _filtered = new List<LogEvent>();

	private int _lastEventCount = -1;

	private int _lastPullFilter = -1;

	private string _lastSearch = "";

	private SearchScope _lastSearchScope;

	private uint _lastFocusId;

	public LogWindow(Plugin plugin)
		: base("Replica Fight Log###ReplicaLog")
	{
		_plugin = plugin;
		base.SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(420f, 250f),
			MaximumSize = new Vector2(2000f, 2000f)
		};
	}

	public void Dispose()
	{
	}

	public override void OnClose()
	{
		if (_plugin.Configuration.LogWindowOpen)
		{
			_plugin.Configuration.LogWindowOpen = false;
			_plugin.Configuration.Save();
		}
	}

	public override void Draw()
	{
		DrawContent();
	}

	public void DrawContent()
	{
		Configuration configuration = _plugin.Configuration;
		DrawZoneBar();
		DrawCaptureRow(configuration);
		DrawKindToggles(configuration);
		DrawAuthorToggles(configuration);
		DrawSearchRow();
		DrawFocusBanner();
		ImGui.Separator();
		DrawPullSidebar();
		ImGui.SameLine();
		DrawTable();
	}

	private void DrawCaptureRow(Configuration cfg)
	{
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Capture:");
		ImGui.SameLine();
		int currentItem = (int)cfg.CaptureWhen;
		ImGui.SetNextItemWidth(170f * ImGuiHelpers.GlobalScale);
		if (ImGui.Combo("##capmode", ref currentItem, CaptureNames, CaptureNames.Length))
		{
			cfg.CaptureWhen = (CaptureMode)currentItem;
			cfg.Save();
			_plugin.Capture.UpdateHookStates();
			if (cfg.CaptureWhen == CaptureMode.Disabled)
			{
				_plugin.Capture.TrimPulls();
				_plugin.Capture.SaveToDisk();
			}
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Always = log everything you see, even out of combat (e.g. Shake It Off in town).\nOnly in combat / Only in a duty restrict when the log records.");
		}
		ImGui.SameLine();
		ImGui.TextDisabled("|");
		ImGui.SameLine();
		if (ImGui.Button(_paused ? "Resume" : "Pause"))
		{
			_paused = !_paused;
			if (!_paused)
			{
				_lastEventCount = -1;
			}
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Freeze the table so streaming events don't move the view while you read.\nCapture keeps running in the background.");
		}
		ImGui.SameLine();
		ImGui.Checkbox("Auto-scroll", ref _autoScroll);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Snap back to the newest row as events arrive.");
		}
		ImGui.SameLine();
		if (ImGui.Button("Jump to latest"))
		{
			_scrollToLatest = true;
		}
		if (_paused)
		{
			ImGui.SameLine();
			ImGui.TextColored(in Ui.Gold, "paused");
		}
	}

	private void DrawKindToggles(Configuration cfg)
	{
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Show:");
		ImGui.SameLine();
		DrawToggle("Casts", () => cfg.ShowCasts, delegate(bool v)
		{
			cfg.ShowCasts = v;
		});
		ImGui.SameLine();
		DrawToggle("Status", () => cfg.ShowStatus, delegate(bool v)
		{
			cfg.ShowStatus = v;
		});
		ImGui.SameLine();
		DrawToggle("Markers", () => cfg.ShowMarkers, delegate(bool v)
		{
			cfg.ShowMarkers = v;
		});
		ImGui.SameLine();
		DrawToggle("Deaths", () => cfg.ShowDeaths, delegate(bool v)
		{
			cfg.ShowDeaths = v;
		});
		ImGui.SameLine();
		ImGui.TextDisabled("|");
		ImGui.SameLine();
		DrawToggle("Enemy", () => cfg.ShowEnemies, delegate(bool v)
		{
			cfg.ShowEnemies = v;
		});
		ImGui.SameLine();
		DrawToggle("You", () => cfg.ShowYou, delegate(bool v)
		{
			cfg.ShowYou = v;
		});
		ImGui.SameLine();
		DrawToggle("Party", () => cfg.ShowParty, delegate(bool v)
		{
			cfg.ShowParty = v;
		});
		ImGui.SameLine();
		ImGui.TextDisabled("|");
		ImGui.SameLine();
		if (ImGui.SmallButton("Reset filters"))
		{
			ResetFilters(cfg);
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Show every event kind again and clear the search / focus / pull filters.");
		}
		ImGui.SameLine();
		ImGui.TextDisabled(FilterSummary(cfg));
	}

	private void DrawAuthorToggles(Configuration cfg)
	{
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Author:");
		ImGui.SameLine();
		DrawToggle("MapFx", () => cfg.ShowMapFx, delegate(bool v)
		{
			cfg.ShowMapFx = v;
		});
		ImGui.SameLine();
		DrawToggle("Adds", () => cfg.ShowAdds, delegate(bool v)
		{
			cfg.ShowAdds = v;
		});
		ImGui.SameLine();
		DrawToggle("Ctrl", () => cfg.ShowControl, delegate(bool v)
		{
			cfg.ShowControl = v;
		});
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Raw ActorControl opcodes (director commands, knockbacks, etc.).\nVery noisy — only enable when debugging a specific low-level mechanic.");
		}
		ImGui.SameLine();
		DrawToggle("Pos", () => cfg.ShowPositions, delegate(bool v)
		{
			cfg.ShowPositions = v;
		});
		ImGui.SameLine();
		DrawToggle("VFX", () => cfg.LogGameVfx, delegate(bool v)
		{
			cfg.LogGameVfx = v;
			cfg.ShowVfx = v;
		});
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Experimental: actor-attached effect VFX (the visual 'tells' the game plays on players/bosses).\nVery high volume — capture only runs while this is on. Paste a logged path into a draw's Custom look.");
		}
		ImGui.SameLine();
		ImGui.TextDisabled("|");
		ImGui.SameLine();
		DrawToggle("IDs", () => cfg.ShowIds, delegate(bool v)
		{
			cfg.ShowIds = v;
		});
		ImGui.SameLine();
		DrawToggle("Dec", () => cfg.ShowDecIds, delegate(bool v)
		{
			cfg.ShowDecIds = v;
		});
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Show decimal IDs too (e.g. NpcBaseId for onAdd).");
		}
	}

	private void DrawSearchRow()
	{
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Find:");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(240f * ImGuiHelpers.GlobalScale);
		ImGui.InputTextWithHint("##search", "name / ID hex+dec…", ref _search, 64);
		ImGui.SameLine();
		ImGui.TextDisabled("in");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(86f * ImGuiHelpers.GlobalScale);
		int currentItem = (int)_searchScope;
		if (ImGui.Combo("##scope", ref currentItem, SearchScopeNames, SearchScopeNames.Length))
		{
			_searchScope = (SearchScope)currentItem;
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Limit the text match to one column:\nany = name / source / target / IDs, or pick source / target / ability.");
		}
		ImGui.SameLine();
		ImGui.TextDisabled("|");
		ImGui.SameLine();
		if (ImGui.Button("Wipe log"))
		{
			_plugin.Capture.Clear();
			_lastEventCount = -1;
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Discard every captured event and reset the pull list.");
		}
		ImGui.SameLine();
		if (ImGui.Button("Export"))
		{
			ExportLog();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Write a readable text log (relative timings + all fields) and open the folder.");
		}
		ImGui.SameLine();
		if (ImGui.Button("Export JSON"))
		{
			ExportJson();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Write a structured JSON dump with everything needed to build a module\n(action ids, cast times, positions/headings, targets, statuses, tethers, headmarkers).");
		}
		if (!string.IsNullOrEmpty(_exportStatus))
		{
			ImGui.SameLine();
			ImGui.TextColored(in ColParty, _exportStatus);
		}
	}

	private void DrawFocusBanner()
	{
		if (_focusId != 0)
		{
			string value = (string.IsNullOrEmpty(_focusName) ? $"0x{_focusId:X8}" : _focusName);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(in Ui.Gold, "Focus:");
			ImGui.SameLine();
			ImU8String text = new ImU8String(6, 2);
			text.AppendFormatted(value);
			text.AppendLiteral("  (0x");
			text.AppendFormatted(_focusId, "X8");
			text.AppendLiteral(")");
			ImGui.TextColored(in Ui.Accent, text);
			ImGui.SameLine();
			if (ImGui.SmallButton("clear focus"))
			{
				ClearFocus();
			}
			ImGui.SameLine();
			ImGui.TextDisabled("only events with this actor as source or target are shown");
		}
	}

	private void SetFocus(uint id, string name)
	{
		_focusId = id;
		_focusName = name;
		_lastEventCount = -1;
	}

	private void ClearFocus()
	{
		_focusId = 0u;
		_focusName = "";
		_lastEventCount = -1;
	}

	private void ResetFilters(Configuration cfg)
	{
		bool flag = (cfg.ShowDeaths = true);
		bool flag3 = (cfg.ShowMarkers = flag);
		bool showCasts = (cfg.ShowStatus = flag3);
		cfg.ShowCasts = showCasts;
		flag3 = (cfg.ShowParty = true);
		showCasts = (cfg.ShowYou = flag3);
		cfg.ShowEnemies = showCasts;
		flag = (cfg.ShowPositions = true);
		flag3 = (cfg.ShowControl = flag);
		showCasts = (cfg.ShowAdds = flag3);
		cfg.ShowMapFx = showCasts;
		cfg.ShowVfx = true;
		cfg.ShowIds = true;
		cfg.ShowDecIds = false;
		cfg.Save();
		_search = "";
		_searchScope = SearchScope.Any;
		_pullFilter = 0;
		ClearFocus();
	}

	private static string FilterSummary(Configuration cfg)
	{
		List<string> list = new List<string>();
		if (!cfg.ShowCasts)
		{
			list.Add("Casts");
		}
		if (!cfg.ShowStatus)
		{
			list.Add("Status");
		}
		if (!cfg.ShowMarkers)
		{
			list.Add("Markers");
		}
		if (!cfg.ShowDeaths)
		{
			list.Add("Deaths");
		}
		if (!cfg.ShowEnemies)
		{
			list.Add("Enemy");
		}
		if (!cfg.ShowYou)
		{
			list.Add("You");
		}
		if (!cfg.ShowParty)
		{
			list.Add("Party");
		}
		if (!cfg.ShowMapFx)
		{
			list.Add("MapFx");
		}
		if (!cfg.ShowAdds)
		{
			list.Add("Adds");
		}
		if (!cfg.ShowControl)
		{
			list.Add("Ctrl");
		}
		if (!cfg.ShowPositions)
		{
			list.Add("Pos");
		}
		if (!cfg.ShowVfx)
		{
			list.Add("VFX");
		}
		if (list.Count == 0)
		{
			return "all kinds shown";
		}
		if (list.Count <= 4)
		{
			return $"{list.Count} hidden: {string.Join(", ", list)}";
		}
		return $"{list.Count} hidden: {string.Join(", ", list.GetRange(0, 4))}, +{list.Count - 4}";
	}

	private void DrawZoneBar()
	{
		uint territoryType = Plugin.ClientState.TerritoryType;
		string fightName = _plugin.Host.FightName;
		uint num = 0u;
		uint value = 0u;
		if (!string.IsNullOrEmpty(fightName) && fightName != "(none)")
		{
			foreach (IGameObject item in Plugin.ObjectTable)
			{
				if (item is IBattleChara battleChara && string.Equals(battleChara.Name.TextValue, fightName, StringComparison.OrdinalIgnoreCase))
				{
					num = battleChara.BaseId;
					value = battleChara.EntityId;
					break;
				}
			}
		}
		ImU8String text = new ImU8String(15, 1);
		text.AppendLiteral("Zone/Arena ID: ");
		text.AppendFormatted(territoryType);
		ImGui.TextColored(in ColId, text);
		ImGui.SameLine();
		ImU8String label = new ImU8String(10, 0);
		label.AppendLiteral("copy##zone");
		if (ImGui.SmallButton(label))
		{
			ImGui.SetClipboardText(territoryType.ToString());
		}
		ImGui.SameLine();
		ImGui.TextDisabled("  |  Boss:");
		ImGui.SameLine();
		ImGui.TextColored(in ColEnemy, string.IsNullOrEmpty(fightName) ? "(none)" : fightName);
		if (num != 0)
		{
			ImGui.SameLine();
			ImU8String text2 = new ImU8String(23, 3);
			text2.AppendLiteral("BaseId ");
			text2.AppendFormatted(num);
			text2.AppendLiteral(" (0x");
			text2.AppendFormatted(num, "X");
			text2.AppendLiteral(")  Entity 0x");
			text2.AppendFormatted(value, "X8");
			ImGui.TextColored(in ColId, text2);
			ImGui.SameLine();
			if (ImGui.SmallButton("copy##bossbase"))
			{
				ImGui.SetClipboardText(num.ToString());
			}
		}
		ImGui.Separator();
	}

	private void DrawPullSidebar()
	{
		float x = MathF.Max(200f * ImGuiHelpers.GlobalScale, ImGui.GetContentRegionAvail().X * 0.26f);
		if (!ImGui.BeginChild("##pulls", new Vector2(x, 0f), border: true))
		{
			ImGui.EndChild();
			return;
		}
		ImGui.TextDisabled("Pulls");
		ImGui.Separator();
		if (ImGui.Selectable("All", _pullFilter == 0))
		{
			_pullFilter = 0;
		}
		IReadOnlyList<CombatLogCapture.PullInfo> pulls = _plugin.Capture.Pulls;
		for (int num = pulls.Count - 1; num >= 0; num--)
		{
			CombatLogCapture.PullInfo pullInfo = pulls[num];
			bool selected = _pullFilter == pullInfo.Index;
			string value = $"{pullInfo.GetFullDisplayLabel()}\n{pullInfo.Start:HH:mm:ss} · {pullInfo.Events} events";
			ImU8String label = new ImU8String(6, 2);
			label.AppendFormatted(value);
			label.AppendLiteral("##pull");
			label.AppendFormatted(pullInfo.Index);
			if (ImGui.Selectable(label, selected, ImGuiSelectableFlags.None, new Vector2(0f, ImGui.GetTextLineHeight() * 2f)))
			{
				_pullFilter = pullInfo.Index;
			}
		}
		ImGui.EndChild();
	}

	private void RebuildFiltered()
	{
		if (_paused)
		{
			return;
		}
		IReadOnlyList<LogEvent> events = _plugin.Capture.Events;
		if (events.Count == _lastEventCount && _pullFilter == _lastPullFilter && _search == _lastSearch && _searchScope == _lastSearchScope && _focusId == _lastFocusId)
		{
			return;
		}
		_lastEventCount = events.Count;
		_lastPullFilter = _pullFilter;
		_lastSearch = _search;
		_lastSearchScope = _searchScope;
		_lastFocusId = _focusId;
		_filtered.Clear();
		for (int num = events.Count - 1; num >= 0; num--)
		{
			LogEvent logEvent = events[num];
			if (Passes(logEvent))
			{
				_filtered.Add(logEvent);
			}
		}
	}

	private void DrawTable()
	{
		RebuildFiltered();
		if (!_paused && _autoScroll && _filtered.Count != _prevFilteredCount)
		{
			_scrollToLatest = true;
		}
		_prevFilteredCount = _filtered.Count;
		if (!ImGui.BeginTable("##log", 5, ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Hideable | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
		{
			return;
		}
		ImGui.TableSetupScrollFreeze(0, 1);
		ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.WidthFixed, 58f * ImGuiHelpers.GlobalScale);
		ImGui.TableSetupColumn("Source", ImGuiTableColumnFlags.WidthFixed, 150f * ImGuiHelpers.GlobalScale);
		ImGui.TableSetupColumn("Event", ImGuiTableColumnFlags.WidthFixed, 66f * ImGuiHelpers.GlobalScale);
		ImGui.TableSetupColumn("Target", ImGuiTableColumnFlags.WidthFixed, 150f * ImGuiHelpers.GlobalScale);
		ImGui.TableSetupColumn("Detail", ImGuiTableColumnFlags.WidthStretch);
		ImGui.TableHeadersRow();
		if (_scrollToLatest)
		{
			ImGui.SetScrollY(0f);
			_scrollToLatest = false;
		}
		float textLineHeightWithSpacing = ImGui.GetTextLineHeightWithSpacing();
		ImGuiListClipper imGuiListClipper = default(ImGuiListClipper);
		imGuiListClipper.Begin(_filtered.Count, textLineHeightWithSpacing);
		while (imGuiListClipper.Step())
		{
			for (int i = imGuiListClipper.DisplayStart; i < imGuiListClipper.DisplayEnd; i++)
			{
				DrawRow(_filtered[i]);
			}
		}
		imGuiListClipper.End();
		ImGui.EndTable();
	}

	private bool Passes(LogEvent e)
	{
		Configuration configuration = _plugin.Configuration;
		if (_pullFilter != 0 && e.Pull != _pullFilter)
		{
			return false;
		}
		if (_focusId != 0 && !TouchesFocus(e))
		{
			return false;
		}
		bool flag;
		switch (e.Kind)
		{
		case LogKind.CastStart:
		case LogKind.CastFinish:
		case LogKind.Ability:
			flag = configuration.ShowCasts;
			break;
		case LogKind.StatusGain:
		case LogKind.StatusLose:
			flag = configuration.ShowStatus;
			break;
		case LogKind.Death:
			flag = configuration.ShowDeaths;
			break;
		case LogKind.Headmarker:
		case LogKind.Tether:
			flag = configuration.ShowMarkers;
			break;
		case LogKind.MapEffect:
			flag = configuration.ShowMapFx;
			break;
		case LogKind.Added:
			flag = configuration.ShowAdds;
			break;
		case LogKind.ActorControl:
			flag = configuration.ShowControl;
			break;
		case LogKind.AbilityExtra:
			flag = configuration.ShowPositions;
			break;
		case LogKind.Vfx:
			flag = configuration.ShowVfx;
			break;
		case LogKind.Note:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			return false;
		}
		LogKind kind = e.Kind;
		if ((kind != LogKind.ActorControl && kind - 12 > LogKind.CastFinish) || 1 == 0)
		{
			flag = e.IsStatus;
			if (!flag)
			{
				kind = e.Kind;
				bool flag2 = kind - 6 <= LogKind.CastFinish;
				flag = flag2;
			}
			if ((flag ? e.TargetKind : e.SourceKind) switch
			{
				ActorKind.Enemy => configuration.ShowEnemies ? 1 : 0, 
				ActorKind.You => configuration.ShowYou ? 1 : 0, 
				ActorKind.Party => configuration.ShowParty ? 1 : 0, 
				_ => TargetKindOk(e) ? 1 : 0, 
			} == 0)
			{
				return false;
			}
		}
		if (!string.IsNullOrEmpty(_search))
		{
			string search = _search;
			if (_searchScope switch
			{
				SearchScope.Source => e.SourceName.Contains(search, StringComparison.OrdinalIgnoreCase) ? 1 : 0, 
				SearchScope.Target => e.TargetName.Contains(search, StringComparison.OrdinalIgnoreCase) ? 1 : 0, 
				SearchScope.Ability => (e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || (e.DataId != 0 && IdMatches(e.DataId, search))) ? 1 : 0, 
				_ => (e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || e.SourceName.Contains(search, StringComparison.OrdinalIgnoreCase) || e.TargetName.Contains(search, StringComparison.OrdinalIgnoreCase) || (e.DataId != 0 && IdMatches(e.DataId, search)) || (e.Category != 0 && IdMatches(e.Category, search))) ? 1 : 0, 
			} == 0)
			{
				return false;
			}
		}
		return true;
	}

	private bool TouchesFocus(LogEvent e)
	{
		if (e.SourceId == _focusId || e.TargetId == _focusId)
		{
			return true;
		}
		uint[] abilityTargetIds = e.AbilityTargetIds;
		if (abilityTargetIds != null)
		{
			for (int i = 0; i < abilityTargetIds.Length; i++)
			{
				if (abilityTargetIds[i] == _focusId)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IdMatches(uint id, string search)
	{
		string text = search.Trim();
		if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
		{
			text = text.Substring(2);
		}
		if (text.Length == 0)
		{
			return false;
		}
		if (id.ToString("X").Contains(text, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (id.ToString("X4").Contains(text, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return id.ToString().Contains(text, StringComparison.Ordinal);
	}

	private bool TargetKindOk(LogEvent e)
	{
		Configuration configuration = _plugin.Configuration;
		if (e.TargetId == Plugin.PlayerState.EntityId)
		{
			return configuration.ShowYou;
		}
		foreach (IPartyMember party in Plugin.PartyList)
		{
			if (party?.EntityId == e.TargetId)
			{
				return configuration.ShowParty;
			}
		}
		return configuration.ShowEnemies;
	}

	private void DrawRow(LogEvent e)
	{
		ImGui.TableNextRow();
		ImGui.TableNextColumn();
		ImU8String label = new ImU8String(3, 2);
		label.AppendFormatted(e.Time, "HH:mm:ss");
		label.AppendLiteral("##r");
		label.AppendFormatted(e.Seq);
		ImGui.Selectable(label, selected: false, ImGuiSelectableFlags.SpanAllColumns);
		ImU8String strId = new ImU8String(3, 1);
		strId.AppendLiteral("ctx");
		strId.AppendFormatted(e.Seq);
		if (ImGui.BeginPopupContextItem(strId))
		{
			ImGui.TextDisabled(string.IsNullOrEmpty(e.Name) ? e.Kind.ToString() : e.Name);
			if (e.DataId != 0)
			{
				ImGui.SameLine();
				ImU8String text = new ImU8String(5, 2);
				text.AppendLiteral("[");
				text.AppendFormatted(e.DataId, "X4");
				text.AppendLiteral(" · ");
				text.AppendFormatted(e.DataId);
				text.AppendLiteral("]");
				ImGui.TextColored(in ColId, text);
			}
			ImGui.Separator();
			if (ImGui.MenuItem("Create quick draw from this"))
			{
				_plugin.OpenQuickDrawFor(e);
			}
			ImGui.Separator();
			if (e.SourceId != 0 && e.SourceId != _focusId && ImGui.MenuItem(string.IsNullOrEmpty(e.SourceName) ? $"Focus this source  (0x{e.SourceId:X8})" : ("Focus this source  (" + e.SourceName + ")")))
			{
				SetFocus(e.SourceId, e.SourceName);
			}
			if (e.TargetId != 0 && e.TargetId != _focusId && ImGui.MenuItem(string.IsNullOrEmpty(e.TargetName) ? $"Focus this target  (0x{e.TargetId:X8})" : ("Focus this target  (" + e.TargetName + ")")))
			{
				SetFocus(e.TargetId, e.TargetName);
			}
			if (_focusId != 0 && ImGui.MenuItem("Clear focus"))
			{
				ClearFocus();
			}
			ImGui.Separator();
			if (e.DataId != 0)
			{
				ImU8String label2 = new ImU8String(15, 1);
				label2.AppendLiteral("Copy ID hex  (");
				label2.AppendFormatted(e.DataId, "X");
				label2.AppendLiteral(")");
				if (ImGui.MenuItem(label2))
				{
					ImGui.SetClipboardText(e.DataId.ToString("X"));
				}
				ImU8String label3 = new ImU8String(15, 1);
				label3.AppendLiteral("Copy ID dec  (");
				label3.AppendFormatted(e.DataId);
				label3.AppendLiteral(")");
				if (ImGui.MenuItem(label3))
				{
					ImGui.SetClipboardText(e.DataId.ToString());
				}
			}
			if (e.Kind == LogKind.MapEffect)
			{
				ImU8String label4 = new ImU8String(14, 1);
				label4.AppendLiteral("Copy flags  (");
				label4.AppendFormatted(e.Category, "X8");
				label4.AppendLiteral(")");
				if (ImGui.MenuItem(label4))
				{
					ImGui.SetClipboardText(e.Category.ToString("X8"));
				}
				ImU8String label5 = new ImU8String(17, 1);
				label5.AppendLiteral("Copy location  (");
				label5.AppendFormatted(e.Param1, "X2");
				label5.AppendLiteral(")");
				if (ImGui.MenuItem(label5))
				{
					ImGui.SetClipboardText(e.Param1.ToString("X2"));
				}
			}
			if (!string.IsNullOrEmpty(e.SourceName))
			{
				ImU8String label6 = new ImU8String(20, 1);
				label6.AppendLiteral("Copy source name  (");
				label6.AppendFormatted(e.SourceName);
				label6.AppendLiteral(")");
				if (ImGui.MenuItem(label6))
				{
					ImGui.SetClipboardText(e.SourceName);
				}
			}
			if (!string.IsNullOrEmpty(e.TargetName))
			{
				ImU8String label7 = new ImU8String(20, 1);
				label7.AppendLiteral("Copy target name  (");
				label7.AppendFormatted(e.TargetName);
				label7.AppendLiteral(")");
				if (ImGui.MenuItem(label7))
				{
					ImGui.SetClipboardText(e.TargetName);
				}
			}
			if (e.SourceId != 0)
			{
				ImU8String label8 = new ImU8String(24, 1);
				label8.AppendLiteral("Copy source entity  (0x");
				label8.AppendFormatted(e.SourceId, "X8");
				label8.AppendLiteral(")");
				if (ImGui.MenuItem(label8))
				{
					ImU8String clipboardText = new ImU8String(0, 1);
					clipboardText.AppendFormatted(e.SourceId, "X8");
					ImGui.SetClipboardText(clipboardText);
				}
			}
			if (e.TargetId != 0)
			{
				ImU8String label9 = new ImU8String(24, 1);
				label9.AppendLiteral("Copy target entity  (0x");
				label9.AppendFormatted(e.TargetId, "X8");
				label9.AppendLiteral(")");
				if (ImGui.MenuItem(label9))
				{
					ImU8String clipboardText2 = new ImU8String(0, 1);
					clipboardText2.AppendFormatted(e.TargetId, "X8");
					ImGui.SetClipboardText(clipboardText2);
				}
			}
			ImGui.EndPopup();
		}
		ImGui.TableNextColumn();
		DrawActorCell(e.SourceName, e.SourceId, SourceColor(e));
		ImGui.TableNextColumn();
		var (text2, col) = e.Kind switch
		{
			LogKind.CastStart => ("startcast", ColCast), 
			LogKind.CastFinish => ("endcast", ColCast), 
			LogKind.Ability => ("use", ColUse), 
			LogKind.StatusGain => ("gain", ColGain), 
			LogKind.StatusLose => ("lose", ColLose), 
			LogKind.Death => ("death", ColDeath), 
			LogKind.Headmarker => ("marker", ColMarker), 
			LogKind.Tether => ("tether", ColMarker), 
			LogKind.MapEffect => ("mapfx", ColMap), 
			LogKind.Added => ("add", ColEnemy), 
			LogKind.ActorControl => ("ctrl", ColCtrl), 
			LogKind.AbilityExtra => ("pos", ColCtrl), 
			LogKind.Vfx => ("vfx", ColMap), 
			LogKind.Note => ("note", ColNote), 
			_ => ("?", ColDim), 
		};
		ImGui.TextColored(in col, text2);
		ImGui.TableNextColumn();
		DrawActorCell(e.TargetName, e.TargetId, TargetColor(e));
		ImGui.TableNextColumn();
		DrawIcon(e.IconId, ImGui.GetTextLineHeight());
		ImGui.SameLine();
		switch (e.Kind)
		{
		case LogKind.CastStart:
		case LogKind.CastFinish:
		case LogKind.Ability:
			ImGui.Text(e.Name);
			DrawId(e);
			if (e.Value > 0f)
			{
				ImGui.SameLine();
				ImU8String text9 = new ImU8String(3, 1);
				text9.AppendLiteral("(");
				text9.AppendFormatted(e.Value, "0.0");
				text9.AppendLiteral("s)");
				ImGui.TextDisabled(text9);
			}
			if (!string.IsNullOrEmpty(e.TargetName))
			{
				ImGui.SameLine();
				ImU8String text10 = new ImU8String(2, 1);
				text10.AppendLiteral("→ ");
				text10.AppendFormatted(e.TargetName);
				ImGui.TextDisabled(text10);
			}
			DrawShapeSize(e);
			break;
		case LogKind.StatusGain:
		case LogKind.StatusLose:
		{
			ImGui.Text(e.Name);
			DrawId(e);
			ImGui.SameLine();
			ImU8String text7 = new ImU8String(3, 1);
			text7.AppendLiteral("on ");
			text7.AppendFormatted(e.TargetName);
			ImGui.TextDisabled(text7);
			if (e.Kind == LogKind.StatusGain && e.Value > 0f)
			{
				ImGui.SameLine();
				ImU8String text8 = new ImU8String(3, 1);
				text8.AppendLiteral("(");
				text8.AppendFormatted(e.Value, "0.0");
				text8.AppendLiteral("s)");
				ImGui.TextDisabled(text8);
			}
			break;
		}
		case LogKind.Headmarker:
			ImGui.Text(e.Name);
			DrawId(e);
			if (!string.IsNullOrEmpty(e.TargetName))
			{
				ImGui.SameLine();
				ImU8String text20 = new ImU8String(3, 1);
				text20.AppendLiteral("on ");
				text20.AppendFormatted(e.TargetName);
				ImGui.TextDisabled(text20);
			}
			break;
		case LogKind.Tether:
		{
			ImGui.Text(e.Name);
			DrawId(e);
			ImGui.SameLine();
			ImU8String text13 = new ImU8String(3, 2);
			text13.AppendFormatted(e.SourceName);
			text13.AppendLiteral(" → ");
			text13.AppendFormatted(e.TargetName);
			ImGui.TextDisabled(text13);
			break;
		}
		case LogKind.Death:
		{
			ImU8String text12 = new ImU8String(5, 1);
			text12.AppendFormatted(e.SourceName);
			text12.AppendLiteral(" died");
			ImGui.TextColored(in ColDeath, text12);
			break;
		}
		case LogKind.MapEffect:
		{
			ImGui.TextColored(in ColMap, "MapEffect");
			ImGui.SameLine();
			ImU8String text14 = new ImU8String(4, 1);
			text14.AppendLiteral("loc ");
			text14.AppendFormatted(e.Param1, "X2");
			ImGui.TextColored(in ColId, text14);
			ImGui.SameLine();
			ImU8String text15 = new ImU8String(2, 1);
			text15.AppendLiteral("(");
			text15.AppendFormatted(e.Param1);
			text15.AppendLiteral(")");
			ImGui.TextDisabled(text15);
			ImGui.SameLine();
			ImU8String text16 = new ImU8String(6, 1);
			text16.AppendLiteral("flags ");
			text16.AppendFormatted(e.Category, "X8");
			ImGui.TextColored(in ColId, text16);
			break;
		}
		case LogKind.Added:
		{
			ImGui.Text(e.Name);
			ImGui.SameLine();
			ImU8String text5 = new ImU8String(12, 2);
			text5.AppendLiteral("[BaseId ");
			text5.AppendFormatted(e.DataId, "X");
			text5.AppendLiteral(" · ");
			text5.AppendFormatted(e.DataId);
			text5.AppendLiteral("]");
			ImGui.TextColored(in ColId, text5);
			if (e.X != 0f || e.Y != 0f)
			{
				ImGui.SameLine();
				ImU8String text6 = new ImU8String(6, 2);
				text6.AppendLiteral("@ (");
				text6.AppendFormatted(e.X, "0.0");
				text6.AppendLiteral(", ");
				text6.AppendFormatted(e.Y, "0.0");
				text6.AppendLiteral(")");
				ImGui.TextDisabled(text6);
			}
			break;
		}
		case LogKind.ActorControl:
		{
			ImU8String text17 = new ImU8String(4, 1);
			text17.AppendLiteral("cat ");
			text17.AppendFormatted(e.Category, "X4");
			ImGui.TextColored(in ColCtrl, text17);
			ImGui.SameLine();
			ImU8String text18 = new ImU8String(22, 5);
			text18.AppendLiteral("(");
			text18.AppendFormatted(e.Category);
			text18.AppendLiteral(")  p1 ");
			text18.AppendFormatted(e.Param1, "X");
			text18.AppendLiteral("  p2 ");
			text18.AppendFormatted(e.Param2, "X");
			text18.AppendLiteral("  p3 ");
			text18.AppendFormatted(e.Param3, "X");
			text18.AppendLiteral("  p4 ");
			text18.AppendFormatted(e.Param4, "X");
			ImGui.TextDisabled(text18);
			if (!string.IsNullOrEmpty(e.SourceName))
			{
				ImGui.SameLine();
				ImU8String text19 = new ImU8String(2, 1);
				text19.AppendLiteral("← ");
				text19.AppendFormatted(e.SourceName);
				ImGui.TextDisabled(text19);
			}
			break;
		}
		case LogKind.Note:
			ImGui.TextColored(in ColNote, e.Name);
			break;
		case LogKind.AbilityExtra:
		{
			ImGui.Text(string.IsNullOrEmpty(e.Name) ? "effect" : e.Name);
			DrawId(e);
			ImGui.SameLine();
			ImU8String text11 = new ImU8String(6, 2);
			text11.AppendLiteral("@ (");
			text11.AppendFormatted(e.X, "0.0");
			text11.AppendLiteral(", ");
			text11.AppendFormatted(e.Y, "0.0");
			text11.AppendLiteral(")");
			ImGui.TextDisabled(text11);
			break;
		}
		case LogKind.Vfx:
			ImGui.TextColored(in ColMap, e.Name);
			if (!string.IsNullOrEmpty(e.TargetName))
			{
				ImGui.SameLine();
				ImU8String text3 = new ImU8String(3, 1);
				text3.AppendLiteral("on ");
				text3.AppendFormatted(e.TargetName);
				ImGui.TextDisabled(text3);
			}
			else if (!string.IsNullOrEmpty(e.SourceName))
			{
				ImGui.SameLine();
				ImU8String text4 = new ImU8String(5, 1);
				text4.AppendLiteral("from ");
				text4.AppendFormatted(e.SourceName);
				ImGui.TextDisabled(text4);
			}
			break;
		case LogKind.TetherCancel:
		case LogKind.Chat:
		case LogKind.TimelineEvent:
		case LogKind.TimelineSync:
		case LogKind.EventObject:
			break;
		}
	}

	private void DrawShapeSize(LogEvent e)
	{
		if (e.DataId == 0)
		{
			return;
		}
		if (!_shapeCache.TryGetValue(e.DataId, out var value))
		{
			value = ActionShape.Describe(e.DataId);
			_shapeCache[e.DataId] = value;
		}
		if (value.HasValue)
		{
			ImGui.SameLine();
			ImGui.TextColored(in ColSize, value.Value.Label);
			if (ImGui.IsItemHovered())
			{
				ImU8String tooltip = new ImU8String(41, 1);
				tooltip.AppendLiteral("AOE shape / size (from the Action sheet)\n");
				tooltip.AppendFormatted(value.Value.Call);
				ImGui.SetTooltip(tooltip);
			}
		}
	}

	private void DrawId(LogEvent e)
	{
		Configuration configuration = _plugin.Configuration;
		if (configuration.ShowIds && e.DataId != 0)
		{
			ImGui.SameLine();
			ImGui.TextColored(in ColId, configuration.ShowDecIds ? $"[{e.DataId:X4} · {e.DataId}]" : $"[{e.DataId:X4}]");
		}
	}

	private Dictionary<int, DateTime> PullStarts()
	{
		Dictionary<int, DateTime> dictionary = new Dictionary<int, DateTime>();
		foreach (CombatLogCapture.PullInfo pull in _plugin.Capture.Pulls)
		{
			dictionary[pull.Index] = pull.Start;
		}
		return dictionary;
	}

	private void ExportLog()
	{
		try
		{
			IReadOnlyList<LogEvent> events = _plugin.Capture.Events;
			Dictionary<int, DateTime> dictionary = PullStarts();
			StringBuilder stringBuilder = new StringBuilder(events.Count * 72);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder2);
			handler.AppendLiteral("# Replica fight log  (");
			handler.AppendFormatted(DateTime.Now, "yyyy-MM-dd HH:mm:ss");
			handler.AppendLiteral(")");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(34, 2, stringBuilder2);
			handler.AppendLiteral("# Zone/Arena ID: ");
			handler.AppendFormatted(Plugin.ClientState.TerritoryType);
			handler.AppendLiteral("   Active fight: ");
			handler.AppendFormatted(_plugin.Host.FightName);
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(20, 2, stringBuilder2);
			handler.AppendLiteral("# ");
			handler.AppendFormatted(events.Count);
			handler.AppendLiteral(" events   (");
			handler.AppendFormatted(_plugin.Capture.Pulls.Count);
			handler.AppendLiteral(" pulls)");
			stringBuilder5.AppendLine(ref handler);
			stringBuilder.AppendLine("# columns: [pull +relSec] [clock] kind  source -> target : detail");
			stringBuilder.AppendLine();
			for (int i = 0; i < events.Count; i++)
			{
				LogEvent logEvent = events[i];
				if (_pullFilter == 0 || logEvent.Pull == _pullFilter)
				{
					double num = (dictionary.TryGetValue(logEvent.Pull, out var value) ? (logEvent.Time - value).TotalSeconds : 0.0);
					stringBuilder.Append("[p").Append(logEvent.Pull).Append(" +")
						.Append(num.ToString("00.000"))
						.Append("] ");
					stringBuilder.Append('[').Append(logEvent.Time.ToString("HH:mm:ss.fff")).Append("] ");
					stringBuilder.AppendLine(FormatExportLine(logEvent));
				}
			}
			string text = WriteExport("txt", stringBuilder.ToString());
			_exportStatus = "saved → " + text;
			RevealFile(text);
		}
		catch (Exception ex)
		{
			_exportStatus = "export failed: " + ex.Message;
		}
	}

	private void ExportJson()
	{
		try
		{
			CombatLogCapture capture = _plugin.Capture;
			IReadOnlyList<LogEvent> events = capture.Events;
			Dictionary<int, DateTime> dictionary = PullStarts();
			var pulls = (from p in capture.Pulls
				where _pullFilter == 0 || p.Index == _pullFilter
				select new
				{
					index = p.Index,
					label = p.Label,
					territory = p.Territory,
					mapId = p.MapId,
					start = p.Start.ToString("o"),
					durationSec = Math.Round((((p.End == DateTime.MinValue) ? DateTime.Now : p.End) - p.Start).TotalSeconds, 2),
					events = p.Events
				}).ToList();
			List<object> list = new List<object>(events.Count);
			foreach (LogEvent item in events)
			{
				if (_pullFilter == 0 || item.Pull == _pullFilter)
				{
					double value = (dictionary.TryGetValue(item.Pull, out var value2) ? (item.Time - value2).TotalSeconds : 0.0);
					list.Add(new
					{
						t = Math.Round(value, 3),
						pull = item.Pull,
						seq = item.Seq,
						kind = item.Kind.ToString(),
						source = new
						{
							name = item.SourceName,
							id = $"0x{item.SourceId:X8}",
							kind = item.SourceKind.ToString()
						},
						target = new
						{
							name = item.TargetName,
							id = $"0x{item.TargetId:X8}",
							kind = item.TargetKind.ToString()
						},
						action = new
						{
							name = item.Name,
							id = item.DataId,
							idHex = $"0x{item.DataId:X}"
						},
						iconId = item.IconId,
						castSec = Math.Round(item.Value, 3),
						count = item.Count,
						pos = new
						{
							x = Math.Round(item.X, 3),
							z = Math.Round(item.Y, 3)
						},
						headingRad = Math.Round(item.Heading, 4),
						headingDeg = Math.Round((double)item.Heading * 180.0 / Math.PI, 1),
						category = item.Category,
						@params = new uint[4] { item.Param1, item.Param2, item.Param3, item.Param4 },
						targets = item.AbilityTargetIds.Select((uint t) => $"0x{t:X8}").ToArray()
					});
				}
			}
			string contents = JsonSerializer.Serialize(new
			{
				exported = DateTime.Now.ToString("o"),
				zone = Plugin.ClientState.TerritoryType,
				fight = _plugin.Host.FightName,
				pullFilter = _pullFilter,
				legend = new
				{
					t = "seconds since the start of that event's pull",
					pos = "world coordinates: x = east/west, z = north/south",
					heading = "facing in radians (headingRad) and degrees (headingDeg)",
					action = "id = Lumina Action sheet row (cast/ability)",
					castSec = "cast duration for CastStart events",
					iconId = "headmarker / status icon id",
					targets = "entity ids hit by an Ability",
					kinds = Enum.GetNames(typeof(LogKind))
				},
				pulls = pulls,
				events = list
			}, new JsonSerializerOptions
			{
				WriteIndented = true
			});
			string text = WriteExport("json", contents);
			_exportStatus = "saved → " + text;
			RevealFile(text);
		}
		catch (Exception ex)
		{
			_exportStatus = "json export failed: " + ex.Message;
		}
	}

	private string WriteExport(string ext, string contents)
	{
		string pluginConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();
		Directory.CreateDirectory(pluginConfigDirectory);

		string fileName;
		if (_pullFilter != 0)
		{
			CombatLogCapture.PullInfo? pull = null;
			foreach (var p in _plugin.Capture.Pulls)
			{
				if (p.Index == _pullFilter)
				{
					pull = p;
					break;
				}
			}
			if (pull != null)
			{
				string slug = pull.GetFileSlug();
				fileName = $"replica-replay-{slug}-{DateTime.Now:yyyyMMdd-HHmmss}.{ext}";
			}
			else
			{
				fileName = $"replica-replay-pull{_pullFilter}-{DateTime.Now:yyyyMMdd-HHmmss}.{ext}";
			}
		}
		else
		{
			uint terr = Plugin.ClientState.TerritoryType;
			string zone = CombatLogCapture.SanitizeForFileName(ZoneLibrary.NameOf(terr));
			string boss = CombatLogCapture.SanitizeForFileName(CombatLogCapture.DetectCurrentBossName());
			string bossPart = (!string.IsNullOrWhiteSpace(boss) && boss != "Unknown" && boss != "none") ? $"_{boss}" : "";
			fileName = $"replica-log-{zone}{bossPart}-{DateTime.Now:yyyyMMdd-HHmmss}.{ext}";
		}

		string text = Path.Combine(pluginConfigDirectory, fileName);
		File.WriteAllText(text, contents);
		return text;
	}

	private static void RevealFile(string path)
	{
		try
		{
			Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"")
			{
				UseShellExecute = true
			});
		}
		catch
		{
		}
	}

	private static string FormatExportLine(LogEvent e)
	{
		string text = ((e.DataId != 0) ? $"[{e.DataId:X} / {e.DataId}]" : "");
		string text2 = ((e.SourceId != 0) ? $"{e.SourceName}(0x{e.SourceId:X8})" : e.SourceName);
		string value = ((e.TargetId != 0) ? $"{e.TargetName}(0x{e.TargetId:X8})" : e.TargetName);
		ActionShape.Info? info = ActionShape.Describe(e.DataId);
		string value2 = (info.HasValue ? ("  " + info.Value.Label) : "");
		return e.Kind switch
		{
			LogKind.CastStart => $"startcast {text2} -> {value} : {e.Name} {text} ({e.Value:0.0}s){value2}", 
			LogKind.CastFinish => $"endcast   {text2} -> {value} : {e.Name} {text}{value2}", 
			LogKind.Ability => $"use     {text2} -> {value} : {e.Name} {text}{value2}", 
			LogKind.StatusGain => $"gain    {e.Name} {text} on {value} ({e.Value:0.0}s)", 
			LogKind.StatusLose => $"lose    {e.Name} {text} on {value}", 
			LogKind.Death => "death   " + text2, 
			LogKind.Headmarker => $"marker  {e.Name} {text} on {value}", 
			LogKind.Tether => $"tether  {e.Name} {text} {text2} -> {value}", 
			LogKind.MapEffect => $"mapfx   location {e.Param1:X2} ({e.Param1})  flags {e.Category:X8}", 
			LogKind.Added => $"add     {e.Name} [BaseId {e.DataId:X} / {e.DataId}] @ ({e.X:0.0}, {e.Y:0.0})", 
			LogKind.ActorControl => $"ctrl    cat {e.Category:X4} ({e.Category})  p1 {e.Param1:X} p2 {e.Param2:X} p3 {e.Param3:X} p4 {e.Param4:X}  src {text2}", 
			LogKind.AbilityExtra => $"pos     {text2} : {e.Name} {text} @ ({e.X:0.0}, {e.Y:0.0})", 
			LogKind.Vfx => $"vfx     {e.Name}  on {value}  from {text2}", 
			LogKind.Note => "note    " + e.Name, 
			_ => "?       " + e.Name + " " + text, 
		};
	}

	private static Vector4 SourceColor(LogEvent e)
	{
		return e.SourceKind switch
		{
			ActorKind.Enemy => ColEnemy, 
			ActorKind.You => ColYou, 
			ActorKind.Party => ColParty, 
			_ => ColDim, 
		};
	}

	private static Vector4 TargetColor(LogEvent e)
	{
		return e.TargetKind switch
		{
			ActorKind.Enemy => ColEnemy, 
			ActorKind.You => ColYou, 
			ActorKind.Party => ColParty, 
			_ => ColDim, 
		};
	}

	private void DrawActorCell(string name, uint id, Vector4 col)
	{
		bool flag = id != 0 && id == _focusId;
		if (!string.IsNullOrEmpty(name))
		{
			ImGui.TextColored(flag ? Ui.Gold : col, name);
		}
		if (_plugin.Configuration.ShowIds && id != 0)
		{
			if (!string.IsNullOrEmpty(name))
			{
				ImGui.SameLine();
			}
			Vector4 col2 = (flag ? Ui.Gold : ColId);
			ImU8String text = new ImU8String(2, 1);
			text.AppendLiteral("0x");
			text.AppendFormatted(id, "X8");
			ImGui.TextColored(in col2, text);
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("click to copy entity id");
			}
			if (ImGui.IsItemClicked())
			{
				ImU8String clipboardText = new ImU8String(0, 1);
				clipboardText.AppendFormatted(id, "X8");
				ImGui.SetClipboardText(clipboardText);
			}
		}
	}

	private void DrawToggle(string label, Func<bool> get, Action<bool> set)
	{
		bool v = get();
		if (ImGui.Checkbox(label, ref v))
		{
			set(v);
			_plugin.Configuration.Save();
			_lastEventCount = -1;
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
