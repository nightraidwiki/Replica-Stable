using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using Replica.Logging;
using Replica.QuickDraws;
using Replica.Strats;

namespace Replica.Windows;

public sealed class StratEditorWindow : Window, IDisposable
{
	private readonly Plugin _plugin;

	private readonly MapCanvas _canvas;

	private StratPack? _pack;

	private int _slideIdx = -1;

	private int _branchIdx = -1;

	private StratRole _placing;

	private bool _snap;

	private static readonly string[] RoleNames = new string[8] { "MT", "OT", "M1", "M2", "R1", "R2", "H1", "H2" };

	private static readonly string[] OnNames = new string[8] { "Cast start", "Cast end", "Status gain", "Status lose", "Headmarker", "Tether", "Death", "Any" };

	private static readonly TriggerMatch[] OnVals = new TriggerMatch[8]
	{
		TriggerMatch.Cast,
		TriggerMatch.CastEnd,
		TriggerMatch.StatusGain,
		TriggerMatch.StatusLose,
		TriggerMatch.Headmarker,
		TriggerMatch.Tether,
		TriggerMatch.Death,
		TriggerMatch.Any
	};

	private static readonly string[] CondKindNames = new string[4] { "My debuff/status", "My role", "Boss position", "Tether on me" };

	private static readonly string[] RoleCatNames = new string[5] { "Tank", "Healer", "DPS (any)", "Melee", "Ranged" };

	private static readonly string[] CompassNames = new string[8] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

	private static readonly string[] ShapeNames = new string[5] { "Circle", "Tower", "Donut", "Rectangle", "Fan" };

	private static readonly QuickShape[] ShapeVals = new QuickShape[5]
	{
		QuickShape.Circle,
		QuickShape.Tower,
		QuickShape.Donut,
		QuickShape.Rectangle,
		QuickShape.Fan
	};

	public StratEditorWindow(Plugin plugin)
		: base("Strat Builder###ReplicaStrat")
	{
		_plugin = plugin;
		_canvas = new MapCanvas(plugin);
		base.SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(820f, 480f),
			MaximumSize = new Vector2(2000f, 1600f)
		};
		base.Size = new Vector2(1040f, 720f);
		base.SizeCondition = ImGuiCond.FirstUseEver;
	}

	public void Dispose()
	{
	}

	public override void PreDraw()
	{
		Ui.PushTheme();
	}

	public override void PostDraw()
	{
		Ui.PopTheme();
	}

	public void Open(StratPack pack)
	{
		_pack = pack;
		_slideIdx = ((pack.Slides.Count <= 0) ? (-1) : 0);
		_branchIdx = ((pack.Slides.Count <= 0 || pack.Slides[0].Branches.Count <= 0) ? (-1) : 0);
		base.IsOpen = true;
		BringToFront();
		_canvas.RecenterOnPlayer();
	}

	public override void Draw()
	{
		if (_pack == null)
		{
			ImGui.TextColored(in Ui.Dimmed, "Open a strat from the Strats tab.");
			return;
		}
		Configuration configuration = _plugin.Configuration;
		float globalScale = ImGuiHelpers.GlobalScale;
		ImGui.SetNextItemWidth(240f * globalScale);
		string buf = _pack.Name;
		if (ImGui.InputText("##sname", ref buf, 64))
		{
			_pack.Name = buf;
			configuration.Save();
		}
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImU8String text = new ImU8String(5, 1);
		text.AppendLiteral("zone ");
		text.AppendFormatted(_pack.Territory);
		ImGui.TextColored(in Ui.Dimmed, text);
		ImGui.SameLine();
		if (ImGui.SmallButton("Use current zone"))
		{
			_pack.Territory = Plugin.ClientState.TerritoryType;
			configuration.Save();
		}
		ImGui.SameLine();
		if (ImGui.SmallButton("Arena"))
		{
			ImGui.OpenPopup("##arenacfg");
		}
		DrawArenaSettings(configuration);
		ImGui.Separator();
		StratSlide stratSlide = ((_slideIdx >= 0 && _slideIdx < _pack.Slides.Count) ? _pack.Slides[_slideIdx] : null);
		StratBranch stratBranch = ((stratSlide != null && _branchIdx >= 0 && _branchIdx < stratSlide.Branches.Count) ? stratSlide.Branches[_branchIdx] : null);
		ImGui.BeginChild("##stratleft", new Vector2(360f * globalScale, 0f));
		ImGui.BeginChild("##castfeed", new Vector2(0f, 190f * globalScale), border: true);
		DrawCastFeed(configuration);
		ImGui.EndChild();
		ImGui.BeginChild("##stepsbox", new Vector2(0f, 150f * globalScale), border: true);
		DrawSteps(configuration);
		ImGui.EndChild();
		ImGui.BeginChild("##branchbox", new Vector2(0f, 0f), border: true);
		if (stratSlide != null)
		{
			DrawBranches(configuration, stratSlide);
		}
		if (stratSlide != null && ImGui.CollapsingHeader("Edit trigger manually"))
		{
			DrawTrigger(configuration, stratSlide);
		}
		ImGui.EndChild();
		ImGui.EndChild();
		ImGui.SameLine();
		ImGui.BeginChild("##stratright", new Vector2(0f, 0f));
		if (stratBranch != null)
		{
			DrawArenaPane(configuration, stratSlide, stratBranch, globalScale);
		}
		else
		{
			ImGui.TextColored(in Ui.Dimmed, "Add a step and a branch to start placing spots.");
		}
		ImGui.EndChild();
	}

	private void DrawArenaSettings(Configuration cfg)
	{
		if (!ImGui.BeginPopup("##arenacfg"))
		{
			return;
		}
		float globalScale = ImGuiHelpers.GlobalScale;
		ImGui.TextColored(in Ui.Gold, "Arena shape (clean ring over the map)");
		int currentItem = _pack.ArenaShape;
		ImGui.SetNextItemWidth(140f * globalScale);
		if (ImGui.Combo("shape##arena", ref currentItem, new string[2] { "Circle", "Square" }, 2))
		{
			_pack.ArenaShape = (byte)currentItem;
			cfg.Save();
		}
		float v = _pack.ArenaRadius;
		ImGui.SetNextItemWidth(140f * globalScale);
		if (ImGui.DragFloat("radius (y)##arena", ref v, 0.25f, 2f, 60f))
		{
			_pack.ArenaRadius = v;
			cfg.Save();
		}
		Vector2 data = new Vector2(_pack.ArenaCenterX, _pack.ArenaCenterZ);
		ImGui.SetNextItemWidth(160f * globalScale);
		if (ImGui.InputFloat2("center X/Z##arena", ref data))
		{
			_pack.ArenaCenterX = data.X;
			_pack.ArenaCenterZ = data.Y;
			cfg.Save();
		}
		if (ImGui.Button("Center = my target"))
		{
			IGameObject gameObject = Plugin.ObjectTable.LocalPlayer?.TargetObject;
			if (gameObject != null)
			{
				_pack.ArenaCenterX = gameObject.Position.X;
				_pack.ArenaCenterZ = gameObject.Position.Z;
				cfg.Save();
			}
		}
		ImGui.SameLine();
		if (ImGui.Button("Center = me"))
		{
			IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
			if (localPlayer != null)
			{
				_pack.ArenaCenterX = localPlayer.Position.X;
				_pack.ArenaCenterZ = localPlayer.Position.Z;
				cfg.Save();
			}
		}
		ImGui.EndPopup();
	}

	private void DrawCastFeed(Configuration cfg)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		ImGui.TextColored(in Ui.Gold, "Boss casts (live) — click to add a step");
		HashSet<uint> hashSet = new HashSet<uint>();
		bool flag = false;
		foreach (IGameObject item3 in Plugin.ObjectTable)
		{
			if (item3.ObjectKind == ObjectKind.BattleNpc && item3 is IBattleChara { IsCasting: not false, CastActionId: not 0u } battleChara && hashSet.Add(battleChara.CastActionId))
			{
				flag = true;
				float fraction = ((battleChara.TotalCastTime > 0f) ? (battleChara.CurrentCastTime / battleChara.TotalCastTime) : 0f);
				string text = ActionName(battleChara.CastActionId);
				ImU8String strId = new ImU8String(2, 1);
				strId.AppendLiteral("ac");
				strId.AppendFormatted(battleChara.CastActionId);
				ImGui.PushID(strId);
				ImGui.ProgressBar(fraction, new Vector2(130f * globalScale, ImGui.GetFrameHeight()), text);
				ImGui.SameLine();
				if (ImGui.SmallButton("+"))
				{
					CreateStepFromCast(cfg, battleChara.CastActionId, text, nsSplit: false);
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip("Add a step bound to this cast");
				}
				ImGui.SameLine();
				if (ImGui.SmallButton("N/S"))
				{
					CreateStepFromCast(cfg, battleChara.CastActionId, text, nsSplit: true);
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip("Add a step split by boss North/South");
				}
				ImGui.PopID();
			}
		}
		if (!flag)
		{
			ImGui.TextColored(in Ui.Dimmed, "No boss casting now — recent below.");
		}
		List<(uint, string)> list = RecentEnemyCasts();
		if (list.Count > 0)
		{
			ImGui.Spacing();
			ImGui.TextDisabled("Recent:");
			foreach (var item4 in list)
			{
				uint item = item4.Item1;
				string item2 = item4.Item2;
				ImU8String strId2 = new ImU8String(2, 1);
				strId2.AppendLiteral("rc");
				strId2.AppendFormatted(item);
				ImGui.PushID(strId2);
				ImU8String label = new ImU8String(3, 1);
				label.AppendFormatted(item2);
				label.AppendLiteral("##r");
				if (ImGui.SmallButton(label))
				{
					CreateStepFromCast(cfg, item, item2, nsSplit: false);
				}
				if (ImGui.IsItemHovered())
				{
					ImU8String tooltip = new ImU8String(53, 1);
					tooltip.AppendLiteral("action #");
					tooltip.AppendFormatted(item);
					tooltip.AppendLiteral("\nclick: add step · shift-click: add N/S split");
					ImGui.SetTooltip(tooltip);
				}
				if (ImGui.IsItemClicked() && ImGui.GetIO().KeyShift)
				{
					CreateStepFromCast(cfg, item, item2, nsSplit: true);
				}
				ImGui.SameLine();
				ImGui.PopID();
			}
			ImGui.NewLine();
		}
		IReadOnlyList<QuickDrawEngine.FireMark> recentFires = _plugin.Engine.RecentFires;
		if (recentFires.Count > 0)
		{
			ImGui.Spacing();
			ImGui.TextDisabled("YapDraw firing:");
			int num = 0;
			int num2 = recentFires.Count - 1;
			while (num2 >= 0 && num < 4)
			{
				ImU8String text2 = new ImU8String(6, 2);
				text2.AppendLiteral("  ");
				text2.AppendFormatted(recentFires[num2].Draw);
				text2.AppendLiteral("  (");
				text2.AppendFormatted(recentFires[num2].Trigger);
				text2.AppendLiteral(")");
				ImGui.TextColored(in Ui.Green, text2);
				num2--;
				num++;
			}
		}
	}

	public void CreateStepFromCast(uint actionId, string castName, bool nsSplit = false)
	{
		CreateStepFromCast(_plugin.Configuration, actionId, castName, nsSplit);
		base.IsOpen = true;
	}

	private void CreateStepFromCast(Configuration cfg, uint actionId, string castName, bool nsSplit)
	{
		StratSlide stratSlide = new StratSlide
		{
			Name = (string.IsNullOrEmpty(castName) ? $"Cast #{actionId}" : castName),
			On = TriggerMatch.Cast,
			MatchById = (actionId != 0),
			DataId = actionId,
			Pattern = castName
		};
		if (nsSplit)
		{
			IGameObject? obj = Plugin.ObjectTable.LocalPlayer?.TargetObject;
			uint bossId = obj?.EntityId ?? 0;
			string bossName = obj?.Name.TextValue ?? "";
			StratBranch stratBranch = new StratBranch
			{
				Name = "North"
			};
			stratBranch.Conditions.Add(new StratCondition
			{
				Kind = CondKind.BossSide,
				BossSide = Compass.N,
				BossId = bossId,
				BossName = bossName
			});
			StratBranch stratBranch2 = new StratBranch
			{
				Name = "South"
			};
			stratBranch2.Conditions.Add(new StratCondition
			{
				Kind = CondKind.BossSide,
				BossSide = Compass.S,
				BossId = bossId,
				BossName = bossName
			});
			stratSlide.Branches.Add(stratBranch);
			stratSlide.Branches.Add(stratBranch2);
		}
		else
		{
			stratSlide.Branches.Add(new StratBranch
			{
				Name = "Default"
			});
		}
		_pack.Slides.Add(stratSlide);
		_slideIdx = _pack.Slides.Count - 1;
		_branchIdx = 0;
		cfg.Save();
	}

	private List<(uint id, string name)> RecentEnemyCasts()
	{
		List<(uint, string)> list = new List<(uint, string)>();
		HashSet<uint> hashSet = new HashSet<uint>();
		IReadOnlyList<LogEvent> events = _plugin.Capture.Events;
		int num = events.Count - 1;
		while (num >= 0 && list.Count < 10)
		{
			LogEvent logEvent = events[num];
			if (logEvent.Kind == Replica.Logging.LogKind.CastStart && logEvent.SourceKind == ActorKind.Enemy && logEvent.DataId != 0 && hashSet.Add(logEvent.DataId))
			{
				list.Add((logEvent.DataId, string.IsNullOrEmpty(logEvent.Name) ? $"#{logEvent.DataId}" : logEvent.Name));
			}
			num--;
		}
		return list;
	}

	private static string ActionName(uint id)
	{
		if (id == 0)
		{
			return "";
		}
		string text = Plugin.Actions.GetRowOrDefault(id)?.Name.ExtractText();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return $"#{id}";
	}

	private void DrawSteps(Configuration cfg)
	{
		ImGui.TextColored(in Ui.Gold, "Steps");
		ImGui.SameLine();
		if (ImGui.SmallButton("+ Step"))
		{
			StratSlide stratSlide = new StratSlide
			{
				Name = $"Step {_pack.Slides.Count + 1}"
			};
			stratSlide.Branches.Add(new StratBranch
			{
				Name = "Default"
			});
			_pack.Slides.Add(stratSlide);
			_slideIdx = _pack.Slides.Count - 1;
			_branchIdx = 0;
			cfg.Save();
		}
		int num = -1;
		for (int i = 0; i < _pack.Slides.Count; i++)
		{
			StratSlide stratSlide2 = _pack.Slides[i];
			ImU8String strId = new ImU8String(5, 1);
			strId.AppendLiteral("slide");
			strId.AppendFormatted(i);
			ImGui.PushID(strId);
			ImU8String label = new ImU8String(5, 1);
			label.AppendFormatted(stratSlide2.Name);
			label.AppendLiteral("##sel");
			if (ImGui.Selectable(label, _slideIdx == i))
			{
				_slideIdx = i;
				_branchIdx = ((stratSlide2.Branches.Count <= 0) ? (-1) : 0);
			}
			ImU8String strId2 = new ImU8String(4, 1);
			strId2.AppendLiteral("sctx");
			strId2.AppendFormatted(i);
			if (ImGui.BeginPopupContextItem(strId2))
			{
				if (ImGui.MenuItem("Delete step"))
				{
					num = i;
				}
				ImGui.EndPopup();
			}
			ImGui.PopID();
		}
		if (num >= 0)
		{
			_pack.Slides.RemoveAt(num);
			_slideIdx = ((_pack.Slides.Count <= 0) ? (-1) : 0);
			_branchIdx = ((_slideIdx < 0 || _pack.Slides[0].Branches.Count <= 0) ? (-1) : 0);
			cfg.Save();
		}
	}

	private void DrawTrigger(Configuration cfg, StratSlide slide)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		ImGui.TextColored(in Ui.Gold, "Trigger");
		ImGui.SetNextItemWidth(200f * globalScale);
		string buf = slide.Name;
		if (ImGui.InputText("name##slide", ref buf, 64))
		{
			slide.Name = buf;
			cfg.Save();
		}
		int currentItem = Array.IndexOf(OnVals, slide.On);
		if (currentItem < 0)
		{
			currentItem = OnVals.Length - 1;
		}
		ImGui.SetNextItemWidth(160f * globalScale);
		if (ImGui.Combo("on##slide", ref currentItem, OnNames, OnNames.Length))
		{
			slide.On = OnVals[currentItem];
			cfg.Save();
		}
		bool v = slide.MatchById;
		if (ImGui.Checkbox("match by id##slide", ref v))
		{
			slide.MatchById = v;
			cfg.Save();
		}
		if (slide.MatchById)
		{
			int data = (int)slide.DataId;
			ImGui.SetNextItemWidth(140f * globalScale);
			if (ImGui.InputInt("action id##slide", ref data))
			{
				slide.DataId = (uint)Math.Max(0, data);
				cfg.Save();
			}
		}
		else
		{
			ImGui.SetNextItemWidth(200f * globalScale);
			string buf2 = slide.Pattern;
			if (ImGui.InputText("name contains##slide", ref buf2, 96))
			{
				slide.Pattern = buf2;
				cfg.Save();
			}
		}
		float v2 = slide.DelaySeconds;
		ImGui.SetNextItemWidth(140f * globalScale);
		if (ImGui.DragFloat("delay (s)##slide", ref v2, 0.1f, 0f, 30f))
		{
			slide.DelaySeconds = MathF.Max(0f, v2);
			cfg.Save();
		}
	}

	private void DrawBranches(Configuration cfg, StratSlide slide)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		ImGui.TextColored(in Ui.Gold, "Branches");
		ImGui.SameLine();
		if (ImGui.SmallButton("+ Branch"))
		{
			slide.Branches.Add(new StratBranch
			{
				Name = $"Variant {slide.Branches.Count + 1}"
			});
			_branchIdx = slide.Branches.Count - 1;
			cfg.Save();
		}
		int num = -1;
		for (int i = 0; i < slide.Branches.Count; i++)
		{
			ImU8String strId = new ImU8String(6, 1);
			strId.AppendLiteral("branch");
			strId.AppendFormatted(i);
			ImGui.PushID(strId);
			ImU8String label = new ImU8String(3, 1);
			label.AppendFormatted(slide.Branches[i].Name);
			label.AppendLiteral("##b");
			if (ImGui.Selectable(label, _branchIdx == i))
			{
				_branchIdx = i;
			}
			ImU8String strId2 = new ImU8String(4, 1);
			strId2.AppendLiteral("bctx");
			strId2.AppendFormatted(i);
			if (ImGui.BeginPopupContextItem(strId2))
			{
				if (ImGui.MenuItem("Delete branch"))
				{
					num = i;
				}
				ImGui.EndPopup();
			}
			ImGui.PopID();
		}
		if (num >= 0)
		{
			slide.Branches.RemoveAt(num);
			_branchIdx = ((slide.Branches.Count <= 0) ? (-1) : 0);
			cfg.Save();
		}
		StratBranch stratBranch = ((_branchIdx >= 0 && _branchIdx < slide.Branches.Count) ? slide.Branches[_branchIdx] : null);
		if (stratBranch == null)
		{
			return;
		}
		ImGui.Spacing();
		ImGui.SetNextItemWidth(200f * globalScale);
		string buf = stratBranch.Name;
		if (ImGui.InputText("branch name##b", ref buf, 64))
		{
			stratBranch.Name = buf;
			cfg.Save();
		}
		ImGui.TextColored(in Ui.Gold, "Use this variant when");
		ImGui.SameLine();
		if (ImGui.SmallButton("+ Condition"))
		{
			stratBranch.Conditions.Add(new StratCondition());
			cfg.Save();
		}
		if (stratBranch.Conditions.Count > 1)
		{
			int currentItem = ((!stratBranch.RequireAll) ? 1 : 0);
			ImGui.SameLine();
			ImGui.SetNextItemWidth(110f * globalScale);
			if (ImGui.Combo("##matchmode", ref currentItem, new string[2] { "match ALL", "match ANY" }, 2))
			{
				stratBranch.RequireAll = currentItem == 0;
				cfg.Save();
			}
		}
		if (stratBranch.Conditions.Count == 0)
		{
			ImGui.TextColored(in Ui.Dimmed, "No conditions = default / catch-all. Keep this branch last.");
		}
		int num2 = -1;
		for (int j = 0; j < stratBranch.Conditions.Count; j++)
		{
			ImU8String strId3 = new ImU8String(4, 1);
			strId3.AppendLiteral("cond");
			strId3.AppendFormatted(j);
			ImGui.PushID(strId3);
			if (DrawCondition(cfg, stratBranch.Conditions[j], j))
			{
				num2 = j;
			}
			ImGui.PopID();
		}
		if (num2 >= 0)
		{
			stratBranch.Conditions.RemoveAt(num2);
			cfg.Save();
		}
	}

	private bool DrawCondition(Configuration cfg, StratCondition c, int idx)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		bool result = false;
		if (ImGui.SmallButton("x"))
		{
			result = true;
		}
		ImGui.SameLine();
		int currentItem = (int)c.Kind;
		ImGui.SetNextItemWidth(140f * globalScale);
		if (ImGui.Combo("##ck", ref currentItem, CondKindNames, CondKindNames.Length))
		{
			c.Kind = (CondKind)currentItem;
			cfg.Save();
		}
		ImGui.SameLine();
		int currentItem2 = (c.Negate ? 1 : 0);
		ImGui.SetNextItemWidth(70f * globalScale);
		if (ImGui.Combo("##cneg", ref currentItem2, new string[2] { "is", "is NOT" }, 2))
		{
			c.Negate = currentItem2 == 1;
			cfg.Save();
		}
		ImGui.SameLine();
		switch (c.Kind)
		{
		case CondKind.MyStatus:
		{
			ImGui.SetNextItemWidth(150f * globalScale);
			string buf2 = c.StatusName;
			if (ImGui.InputText("##csn", ref buf2, 64))
			{
				c.StatusName = buf2;
				cfg.Save();
			}
			ImGui.SameLine();
			int data2 = (int)c.StatusId;
			ImGui.SetNextItemWidth(90f * globalScale);
			if (ImGui.InputInt("id##csid", ref data2))
			{
				c.StatusId = (uint)Math.Max(0, data2);
				cfg.Save();
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Pick"))
			{
				ImU8String strId = new ImU8String(7, 1);
				strId.AppendLiteral("##cpick");
				strId.AppendFormatted(idx);
				ImGui.OpenPopup(strId);
			}
			if (c.StatusId != 0)
			{
				ImGui.SameLine();
				ImGui.TextColored(SelfHasStatus(c.StatusId) ? Ui.Green : Ui.Dimmed, SelfHasStatus(c.StatusId) ? "on you" : "not on you");
			}
			DrawCondStatusPicker(cfg, c, $"##cpick{idx}");
			break;
		}
		case CondKind.MyRole:
		{
			int currentItem4 = (int)c.Role;
			ImGui.SetNextItemWidth(120f * globalScale);
			if (ImGui.Combo("##crole", ref currentItem4, RoleCatNames, RoleCatNames.Length))
			{
				c.Role = (RoleCat)currentItem4;
				cfg.Save();
			}
			break;
		}
		case CondKind.BossSide:
		{
			int currentItem3 = (int)c.BossSide;
			ImGui.SetNextItemWidth(70f * globalScale);
			if (ImGui.Combo("##cside", ref currentItem3, CompassNames, CompassNames.Length))
			{
				c.BossSide = (Compass)currentItem3;
				cfg.Save();
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Use target"))
			{
				IGameObject gameObject = Plugin.ObjectTable.LocalPlayer?.TargetObject;
				if (gameObject != null)
				{
					c.BossId = gameObject.EntityId;
					c.BossName = gameObject.Name.TextValue;
					cfg.Save();
				}
			}
			if (!string.IsNullOrEmpty(c.BossName))
			{
				ImGui.SameLine();
				ImGui.TextColored(in Ui.Dimmed, c.BossName);
			}
			break;
		}
		case CondKind.TetherOnMe:
		{
			ImGui.SetNextItemWidth(130f * globalScale);
			string buf = c.TetherName;
			if (ImGui.InputText("##ctn", ref buf, 48))
			{
				c.TetherName = buf;
				cfg.Save();
			}
			ImGui.SameLine();
			int data = (int)c.TetherId;
			ImGui.SetNextItemWidth(90f * globalScale);
			if (ImGui.InputInt("id##ctid", ref data))
			{
				c.TetherId = (uint)Math.Max(0, data);
				cfg.Save();
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Pick"))
			{
				IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
				if (localPlayer != null)
				{
					foreach (CombatLogCapture.LiveTether activeTether in _plugin.Capture.ActiveTethers)
					{
						if (activeTether.From == localPlayer.EntityId || activeTether.To == localPlayer.EntityId)
						{
							c.TetherId = activeTether.Id;
							cfg.Save();
							break;
						}
					}
				}
			}
			if (c.TetherId != 0)
			{
				ImGui.SameLine();
				ImU8String text = new ImU8String(1, 1);
				text.AppendLiteral("#");
				text.AppendFormatted(c.TetherId);
				ImGui.TextColored(in Ui.Dimmed, text);
			}
			break;
		}
		}
		return result;
	}

	private void DrawCondStatusPicker(Configuration cfg, StratCondition c, string popupId)
	{
		if (!ImGui.BeginPopup(popupId))
		{
			return;
		}
		ImGui.TextDisabled("Your current statuses — click to bind");
		ImGui.Separator();
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		bool flag = false;
		if (localPlayer != null)
		{
			foreach (IStatus status in localPlayer.StatusList)
			{
				if (status != null && status.StatusId != 0)
				{
					flag = true;
					string text = StatusName(status.StatusId);
					ImU8String label = new ImU8String(10, 3);
					label.AppendFormatted(text);
					label.AppendLiteral("  (#");
					label.AppendFormatted(status.StatusId);
					label.AppendLiteral(")##cst");
					label.AppendFormatted(status.StatusId);
					if (ImGui.Selectable(label))
					{
						c.StatusId = status.StatusId;
						c.StatusName = text;
						cfg.Save();
						ImGui.CloseCurrentPopup();
					}
				}
			}
		}
		if (!flag)
		{
			ImGui.TextColored(in Ui.Dimmed, "No statuses on you right now.");
		}
		ImGui.EndPopup();
	}

	private static bool SelfHasStatus(uint statusId)
	{
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (localPlayer == null || statusId == 0)
		{
			return false;
		}
		foreach (IStatus status in localPlayer.StatusList)
		{
			if (status != null && status.StatusId == statusId)
			{
				return true;
			}
		}
		return false;
	}

	private static string StatusName(uint id)
	{
		if (id == 0)
		{
			return "";
		}
		string text = Plugin.DataManager.GetExcelSheet<Status>().GetRowOrDefault(id)?.Name.ExtractText();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return $"#{id}";
	}

	private void DrawArenaPane(Configuration cfg, StratSlide slide, StratBranch branch, float scale)
	{
		ImGui.TextColored(in Ui.Dimmed, "Pick a role, then click the arena to drop your spot.");
		for (int i = 0; i < RoleNames.Length; i++)
		{
			StratRole role = (StratRole)i;
			bool flag = branch.Spots.Exists((RoleSpot s) => s.Role == role && s.Enabled);
			bool num = _placing == role;
			if (num)
			{
				Vector4 accent = Ui.Accent;
				accent.W = 0.85f;
				ImGui.PushStyleColor(ImGuiCol.Button, accent);
			}
			else if (flag)
			{
				ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.3f, 0.4f, 0.3f, 1f));
			}
			ImU8String label = new ImU8String(6, 1);
			label.AppendFormatted(RoleNames[i]);
			label.AppendLiteral("##role");
			if (ImGui.Button(label, new Vector2(40f * scale, 0f)))
			{
				_placing = role;
			}
			if (num | flag)
			{
				ImGui.PopStyleColor();
			}
			if (i < RoleNames.Length - 1)
			{
				ImGui.SameLine();
			}
		}
		RoleSpot roleSpot = branch.Spots.Find((RoleSpot s) => s.Role == _placing);
		ImGui.Spacing();
		if (roleSpot != null)
		{
			bool value = roleSpot.Enabled;
			if (Ui.ToggleSwitch("##spoten", ref value))
			{
				roleSpot.Enabled = value;
				cfg.Save();
			}
			ImGui.SameLine(0f, 8f);
			ImGui.AlignTextToFramePadding();
			Vector4 accent = (value ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed);
			ImU8String text = new ImU8String(5, 1);
			text.AppendFormatted(RoleNames[(uint)_placing]);
			text.AppendLiteral(" spot");
			ImGui.TextColored(in accent, text);
			int currentItem = Array.IndexOf(ShapeVals, roleSpot.Shape);
			if (currentItem < 0)
			{
				currentItem = 0;
			}
			ImGui.SameLine(0f, 16f);
			ImGui.SetNextItemWidth(110f * scale);
			if (ImGui.Combo("##spshape", ref currentItem, ShapeNames, ShapeNames.Length))
			{
				roleSpot.Shape = ShapeVals[currentItem];
				cfg.Save();
			}
			ImGui.SameLine();
			ImGui.SetNextItemWidth(120f * scale);
			float v = roleSpot.Radius;
			if (ImGui.DragFloat("radius##sp", ref v, 0.1f, 0.3f, 30f))
			{
				roleSpot.Radius = v;
				cfg.Save();
			}
			Vector4 col = roleSpot.Color;
			if (ImGui.ColorEdit4("marker##sp", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar))
			{
				roleSpot.Color = col;
				cfg.Save();
			}
			ImGui.SameLine();
			bool v2 = roleSpot.ShowLeash;
			if (ImGui.Checkbox("leash##sp", ref v2))
			{
				roleSpot.ShowLeash = v2;
				cfg.Save();
			}
			if (roleSpot.ShowLeash)
			{
				ImGui.SameLine();
				Vector4 col2 = roleSpot.LeashColor;
				if (ImGui.ColorEdit4("line##sp", ref col2, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar))
				{
					roleSpot.LeashColor = col2;
					cfg.Save();
				}
			}
			ImGui.SameLine();
			ImGui.SetNextItemWidth(110f * scale);
			float v3 = roleSpot.Duration;
			if (ImGui.DragFloat("hold (s)##sp", ref v3, 0.2f, 1f, 60f))
			{
				roleSpot.Duration = v3;
				cfg.Save();
			}
			int currentItem2 = (int)roleSpot.Anchor;
			ImGui.SetNextItemWidth(170f * scale);
			if (ImGui.Combo("##spanchor", ref currentItem2, new string[2] { "Fixed arena spot", "My tether (clone)" }, 2))
			{
				roleSpot.Anchor = (SpotAnchor)currentItem2;
				cfg.Save();
			}
			if (roleSpot.Anchor == SpotAnchor.TetheredToMe)
			{
				ImGui.SameLine();
				int data = (int)roleSpot.TetherId;
				ImGui.SetNextItemWidth(90f * scale);
				if (ImGui.InputInt("tether id##sp", ref data))
				{
					roleSpot.TetherId = (uint)Math.Max(0, data);
					cfg.Save();
				}
				ImGui.SameLine();
				if (ImGui.SmallButton("Pick##sptether"))
				{
					IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
					if (localPlayer != null)
					{
						foreach (CombatLogCapture.LiveTether activeTether in _plugin.Capture.ActiveTethers)
						{
							if (activeTether.From == localPlayer.EntityId || activeTether.To == localPlayer.EntityId)
							{
								roleSpot.TetherId = activeTether.Id;
								cfg.Save();
								break;
							}
						}
					}
				}
				ImGui.SameLine();
				ImGui.TextColored(in Ui.Dimmed, (roleSpot.TetherId == 0) ? "(any tether on me)" : $"#{roleSpot.TetherId}");
			}
		}
		else
		{
			ImU8String text2 = new ImU8String(35, 1);
			text2.AppendFormatted(RoleNames[(uint)_placing]);
			text2.AppendLiteral(" has no spot yet — click the arena.");
			ImGui.TextColored(in Ui.Dimmed, text2);
		}
		ImGui.Spacing();
		ImGui.Checkbox("Snap 1y", ref _snap);
		ImGui.SameLine();
		if (ImGui.Button("Test in game"))
		{
			_plugin.Strat.Preview(slide, branch);
		}
		ImGui.SameLine();
		if (ImGui.Button("Clear shapes"))
		{
			_plugin.Host.CleanVfx();
		}
		ImGui.SameLine();
		if (ImGui.Button("Recenter on me"))
		{
			_canvas.RecenterOnPlayer();
		}
		ImGui.SameLine();
		if (ImGui.Button("Center on target"))
		{
			IGameObject gameObject = Plugin.ObjectTable.LocalPlayer?.TargetObject;
			if (gameObject != null)
			{
				_canvas.CenterX = gameObject.Position.X;
				_canvas.CenterZ = gameObject.Position.Z;
			}
		}
		ImGui.SameLine(0f, 16f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Zoom");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(120f * scale);
		ImGui.SliderFloat("##stratzoom", ref _canvas.ViewRadius, 5f, _canvas.MaxRadius, "%.0fy", ImGuiSliderFlags.Logarithmic);
		ImGui.SameLine();
		ImGui.Checkbox("Map", ref _canvas.ShowGameMap);
		ImGui.SameLine();
		ImGui.Checkbox("Names", ref _canvas.ShowNames);
		ImGui.Spacing();
		float num2 = MathF.Min(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y) - 4f;
		if (num2 < 200f)
		{
			num2 = 200f;
		}
		MapCanvas.Frame f = _canvas.Begin("##stratpad", num2);
		_canvas.DrawArenaFloor(f, _pack.ArenaShape, _pack.ArenaRadius, _pack.ArenaCenterX, _pack.ArenaCenterZ);
		_canvas.DrawLiveActors(f);
		ImDrawListPtr dl = f.Dl;
		uint num3 = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f));
		uint col3 = ImGui.ColorConvertFloat4ToU32(new Vector4(0.05f, 0.05f, 0.05f, 1f));
		foreach (RoleSpot spot in branch.Spots)
		{
			if (!spot.Enabled)
			{
				continue;
			}
			Vector2 vector = _canvas.ToScreen(spot.Position.X, spot.Position.Z, f.Origin, f.Size);
			Vector4 accent = spot.Color;
			accent.W = 1f;
			uint col4 = ImGui.ColorConvertFloat4ToU32(accent);
			bool flag2 = spot.Role == _placing;
			if (spot.ShowLeash)
			{
				IPlayerCharacter localPlayer2 = Plugin.ObjectTable.LocalPlayer;
				if (localPlayer2 != null)
				{
					Vector2 p = _canvas.ToScreen(localPlayer2.Position.X, localPlayer2.Position.Z, f.Origin, f.Size);
					dl.AddLine(p, vector, ImGui.ColorConvertFloat4ToU32(spot.LeashColor), 1.6f);
				}
			}
			dl.AddCircleFilled(vector, flag2 ? 9f : 7f, col4);
			dl.AddCircle(vector, flag2 ? 12f : 9f, flag2 ? num3 : ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.6f)), 18, flag2 ? 2f : 1.2f);
			string text3 = RoleNames[(uint)spot.Role];
			Vector2 vector2 = ImGui.CalcTextSize(text3);
			dl.AddText(new Vector2(vector.X - vector2.X * 0.5f, vector.Y - vector2.Y * 0.5f), col3, text3);
		}
		_canvas.End(f);
		if (f.Active && ImGui.IsMouseDown(ImGuiMouseButton.Left))
		{
			Vector2 vector3 = _canvas.ToWorld(ImGui.GetMousePos(), f.Origin, f.Size);
			if (_snap)
			{
				vector3.X = MathF.Round(vector3.X);
				vector3.Y = MathF.Round(vector3.Y);
			}
			else
			{
				vector3.X = MathF.Round(vector3.X, 1);
				vector3.Y = MathF.Round(vector3.Y, 1);
			}
			RoleSpot roleSpot2 = branch.Spots.Find((RoleSpot s) => s.Role == _placing);
			if (roleSpot2 == null)
			{
				roleSpot2 = new RoleSpot
				{
					Role = _placing
				};
				branch.Spots.Add(roleSpot2);
			}
			roleSpot2.Position = new Vector3(vector3.X, 0f, vector3.Y);
			roleSpot2.Enabled = true;
			cfg.Save();
		}
	}
}
