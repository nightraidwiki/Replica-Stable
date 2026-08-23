using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Replica.Logging;
using Replica.QuickDraws;

namespace Replica.Windows;

public sealed class QuickDrawEditorWindow : Window, IDisposable
{
	private struct Point
	{
		public int X;

		public int Y;
	}

	private readonly Plugin _plugin;

	private QuickDrawDef? _t;

	private QuickDrawDef? _real;

	private QuickDrawModule? _owner;

	private bool _isNew;

	private bool _dirty;

	private int _sel = -1;

	private string _status = "";

	private string _librarySearch = "";

	private string _zoneSearch = "";

	private readonly Dictionary<string, string> _condSearch = new Dictionary<string, string>();

	private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new Dictionary<uint, ISharedImmediateTexture>();

	private Action<Vector3>? _groundPick;

	private bool _wasLmbDown;

	private bool _wasEscDown;

	private int _groundPickGrace;

	private bool _padSnapGrid;

	private static readonly string[] ShapeNames = new string[11]
	{
		"Circle", "Donut", "Fan", "Rectangle", "Line", "Tower", "Knockback", "Laser", "Text", "Arrow",
		"Path"
	};

	private static readonly (DrawAnchor V, string Label)[] AnchorOpts = new(DrawAnchor, string)[16]
	{
		(DrawAnchor.Source, "On the caster"),
		(DrawAnchor.Target, "On the target"),
		(DrawAnchor.Self, "On me"),
		(DrawAnchor.FixedPosition, "Fixed spot"),
		(DrawAnchor.EventPosition, "Where it happened"),
		(DrawAnchor.ArenaCenter, "Arena centre"),
		(DrawAnchor.WaymarkA, "Waymark A"),
		(DrawAnchor.WaymarkB, "Waymark B"),
		(DrawAnchor.WaymarkC, "Waymark C"),
		(DrawAnchor.WaymarkD, "Waymark D"),
		(DrawAnchor.Waymark1, "Waymark 1"),
		(DrawAnchor.Waymark2, "Waymark 2"),
		(DrawAnchor.Waymark3, "Waymark 3"),
		(DrawAnchor.Waymark4, "Waymark 4"),
		(DrawAnchor.LinkedShape, "Another shape"),
		(DrawAnchor.NearbyActorById, "Nearest actor by id")
	};

	private static readonly (LinkTarget V, string Label)[] LinkOpts = new(LinkTarget, string)[17]
	{
		(LinkTarget.EventTarget, "Event target"),
		(LinkTarget.EventSource, "Event caster"),
		(LinkTarget.MyTarget, "My target"),
		(LinkTarget.NearestPlayer, "Nearest player"),
		(LinkTarget.NearestEnemy, "Nearest enemy"),
		(LinkTarget.PlayerWithSameStatus, "Player w/ same debuff"),
		(LinkTarget.FixedSpot, "Fixed spot"),
		(LinkTarget.ArenaCenter, "Arena centre"),
		(LinkTarget.WaymarkA, "Waymark A"),
		(LinkTarget.WaymarkB, "Waymark B"),
		(LinkTarget.WaymarkC, "Waymark C"),
		(LinkTarget.WaymarkD, "Waymark D"),
		(LinkTarget.Waymark1, "Waymark 1"),
		(LinkTarget.Waymark2, "Waymark 2"),
		(LinkTarget.Waymark3, "Waymark 3"),
		(LinkTarget.Waymark4, "Waymark 4"),
		(LinkTarget.LinkedShape, "Another shape")
	};

	private static readonly string[] AnchorLabels = Array.ConvertAll(AnchorOpts, ((DrawAnchor V, string Label) o) => o.Label);

	private static readonly string[] LinkLabels = Array.ConvertAll(LinkOpts, ((LinkTarget V, string Label) o) => o.Label);

	private static readonly (string Name, Vector4 Color)[] ColorPresets = new(string, Vector4)[8]
	{
		("Meteor / spread (orange)", new Vector4(1f, 0.55f, 0.1f, 1f)),
		("Cone / bait (yellow)", new Vector4(1f, 0.8f, 0.1f, 1f)),
		("Stack (green)", new Vector4(0.1f, 0.75f, 0.4f, 1f)),
		("Tower / grab (green)", new Vector4(0.2f, 0.95f, 0.35f, 1f)),
		("Safe (cyan)", new Vector4(0.2f, 0.9f, 1f, 1f)),
		("Mechanic (violet)", new Vector4(0.45f, 0.4f, 1f, 1f)),
		("Dark (purple)", new Vector4(0.6f, 0f, 1f, 1f)),
		("Danger (red)", new Vector4(0.96f, 0.2f, 0.2f, 1f))
	};

	private readonly Dictionary<uint, ISharedImmediateTexture?> _iconPreview = new Dictionary<uint, ISharedImmediateTexture>();

	private static readonly string[] MatchNames = new string[9] { "Anything", "Cast started", "Status gained", "Status lost", "Death", "Headmarker", "Tether", "Chat / battle-log", "Cast ended (snapshot)" };

	private static readonly string[] SourceNames = new string[4] { "Anyone", "Boss / enemy", "You", "Party" };

	private static readonly string[] RoleNames = new string[4] { "Any", "Tank", "Healer", "DPS" };

	private static readonly string[] ToNames = new string[5] { "Anyone", "Me", "a Tank", "a Healer", "a DPS" };

	private static readonly string[] NumFieldNames = new string[11]
	{
		"Stack count", "Value (cast/skill)", "Caster HP %", "Target HP %", "Param 1", "Param 2", "Param 3", "Param 4", "Dist source→target", "Dist me→source",
		"Dist me→target"
	};

	private static readonly string[] StatusWhoNames = new string[3] { "Me", "Source", "Target" };

	private static readonly string[] NumOpNames = new string[6] { "is", "is not", "less than", "at most", "more than", "at least" };

	private static readonly string[] VarOpNames = new string[2] { "set to", "add" };

	private static readonly string[] ConcurrencyNames = new string[3] { "Wait — keep the first shape", "Replace — newest shape wins", "Stack — show both" };

	private static readonly string[] MatchModeNames = new string[2] { "all of", "any of" };

	private static readonly string[] ClearEventNames = new string[8] { "a cast starts", "a status is gained", "a status is lost", "a headmarker appears", "a tether forms", "someone dies", "chat says", "the cast resolves" };

	private static readonly string[] FollowUpNames = new string[9] { "Wait, then draw", "When a cast starts", "When a status is gained", "When a status is lost", "When a headmarker appears", "When a tether forms", "When someone dies", "When chat says…", "When the cast resolves" };

	public QuickDrawEditorWindow(Plugin plugin)
		: base("Edit Quick Draw###ReplicaEditor")
	{
		_plugin = plugin;
		base.SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(720f, 360f),
			MaximumSize = new Vector2(1600f, 1400f)
		};
		base.Size = new Vector2(1000f, 720f);
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

	public void Open(QuickDrawDef t)
	{
		_real = t;
		_owner = FindOwner(t);
		_t = t.Clone();
		QuickDrawEngine.EnsureIds(_t);
		_isNew = false;
		_dirty = false;
		_sel = -1;
		_status = "";
		base.IsOpen = true;
	}

	public void OpenFor(LogEvent e)
	{
		bool flag = e.TargetId == Plugin.PlayerState.EntityId;
		bool onlyOnSelf = (e.IsStatus || e.Kind == LogKind.Headmarker) & flag;
		SourceFilter sourceFilter = ((!e.IsStatus) ? (e.SourceKind switch
		{
			ActorKind.Enemy => SourceFilter.Enemy, 
			ActorKind.You => SourceFilter.You, 
			ActorKind.Party => SourceFilter.Party, 
			_ => SourceFilter.Anyone, 
		}) : SourceFilter.Anyone);
		SourceFilter source = sourceFilter;
		RoleFilter targetRole = ((!flag) ? RoleOf(e.TargetId) : RoleFilter.Any);
		uint territoryType = Plugin.ClientState.TerritoryType;
		TriggerMatch triggerMatch = e.Kind switch
		{
			LogKind.CastStart => TriggerMatch.Cast, 
			LogKind.StatusGain => TriggerMatch.StatusGain, 
			LogKind.StatusLose => TriggerMatch.StatusLose, 
			LogKind.Death => TriggerMatch.Death, 
			LogKind.Headmarker => TriggerMatch.Headmarker, 
			LogKind.Tether => TriggerMatch.Tether, 
			_ => TriggerMatch.Any, 
		};
		DrawAnchor anchor = ((e.IsStatus || e.Kind == LogKind.Headmarker) ? ((!flag) ? DrawAnchor.Target : DrawAnchor.Self) : DrawAnchor.Source);
		bool flag2 = e.Kind == LogKind.Vfx;
		DrawSpec drawSpec = new DrawSpec
		{
			Anchor = anchor
		};
		if (flag2)
		{
			drawSpec.CustomVfx = e.Name;
		}
		else if (e.IsCast)
		{
			ApplyActionShape(drawSpec, e.DataId);
		}
		_t = new QuickDrawDef
		{
			Name = e.Name,
			Pattern = (flag2 ? "" : e.Name),
			On = ((!flag2) ? triggerMatch : TriggerMatch.Any),
			Source = source,
			OnlyOnSelf = onlyOnSelf,
			TargetRole = targetRole,
			MatchById = (!flag2 && e.DataId != 0),
			DataId = ((!flag2) ? e.DataId : 0u),
			IconId = e.IconId,
			AnyZone = (territoryType == 0),
			Zones = ((territoryType != 0) ? new List<uint> { territoryType } : new List<uint>()),
			Draw = drawSpec
		};
		QuickDrawEngine.EnsureIds(_t);
		_real = null;
		_owner = _plugin.Configuration.QuickModule();
		_isNew = true;
		_dirty = true;
		_sel = -1;
		_status = "";
		base.IsOpen = true;
	}

	public void OpenForCatalog(FightCatalog.Entry entry, uint territory)
	{
		TriggerMatch triggerMatch = entry.Kind switch
		{
			FightCatalog.Kind.Cast => TriggerMatch.Cast, 
			FightCatalog.Kind.Status => TriggerMatch.StatusGain, 
			FightCatalog.Kind.Headmarker => TriggerMatch.Headmarker, 
			FightCatalog.Kind.Tether => TriggerMatch.Tether, 
			_ => TriggerMatch.Cast, 
		};
		DrawAnchor drawAnchor = ((entry.Kind != FightCatalog.Kind.Cast) ? DrawAnchor.Target : DrawAnchor.Source);
		DrawAnchor anchor = drawAnchor;
		DrawSpec drawSpec = new DrawSpec
		{
			Anchor = anchor
		};
		if (entry.Kind == FightCatalog.Kind.Cast)
		{
			ApplyActionShape(drawSpec, entry.Id);
		}
		QuickDrawDef quickDrawDef = new QuickDrawDef();
		quickDrawDef.Name = entry.Name;
		quickDrawDef.Pattern = entry.Name;
		quickDrawDef.On = triggerMatch;
		quickDrawDef.Source = ((triggerMatch == TriggerMatch.Cast) ? SourceFilter.Enemy : SourceFilter.Anyone);
		QuickDrawDef quickDrawDef2 = quickDrawDef;
		bool onlyOnSelf = ((triggerMatch == TriggerMatch.StatusGain || triggerMatch - 5 <= TriggerMatch.Cast) ? true : false);
		quickDrawDef2.OnlyOnSelf = onlyOnSelf;
		quickDrawDef.MatchById = entry.Id != 0;
		quickDrawDef.DataId = entry.Id;
		quickDrawDef.IconId = entry.Icon;
		quickDrawDef.AnyZone = territory == 0;
		quickDrawDef.Zones = ((territory != 0) ? new List<uint> { territory } : new List<uint>());
		quickDrawDef.Draw = drawSpec;
		_t = quickDrawDef;
		QuickDrawEngine.EnsureIds(_t);
		_real = null;
		_owner = _plugin.Configuration.QuickModule();
		_isNew = true;
		_dirty = true;
		_sel = -1;
		_status = "";
		base.IsOpen = true;
	}

	public void OpenForMapAoe(MapAoe aoe, string? actionName = null, uint territory = 0)
	{
		uint actId = aoe.ActionId;
		string name = !string.IsNullOrEmpty(actionName) ? actionName : (actId != 0 ? $"Cast #{actId}" : "AoE Draw");
		uint terr = territory != 0 ? territory : Plugin.ClientState.TerritoryType;

		DrawSpec drawSpec = new DrawSpec
		{
			Anchor = (aoe.TargetId != 0 && aoe.TargetId != aoe.SourceId) ? DrawAnchor.Target : DrawAnchor.Source
		};

		switch (aoe.Kind)
		{
			case MapAoeKind.Circle:
			case MapAoeKind.SafeSpot:
				drawSpec.Shape = QuickShape.Circle;
				drawSpec.Radius = MathF.Max(1f, aoe.Param1 > 0 ? aoe.Param1 : 5f);
				break;
			case MapAoeKind.Donut:
				drawSpec.Shape = QuickShape.Donut;
				drawSpec.Radius = MathF.Max(2f, aoe.Param1);
				drawSpec.InnerRadius = MathF.Max(0f, aoe.Param2);
				break;
			case MapAoeKind.Cone:
				drawSpec.Shape = QuickShape.Fan;
				drawSpec.Radius = MathF.Max(2f, aoe.Param1);
				drawSpec.FanAngle = (int)MathF.Round((aoe.Param2 > 0.01f ? aoe.Param2 * 2f : 1.57f) * 180f / MathF.PI);
				drawSpec.OrientToFacing = true;
				break;
			case MapAoeKind.Rect:
				drawSpec.Shape = QuickShape.Rectangle;
				drawSpec.Length = MathF.Max(2f, aoe.Param1 + aoe.Param2);
				drawSpec.HalfWidth = MathF.Max(1f, aoe.Param3);
				drawSpec.OrientToFacing = true;
				break;
			case MapAoeKind.Cross:
				drawSpec.Shape = QuickShape.Rectangle;
				drawSpec.Length = MathF.Max(2f, aoe.Param1);
				drawSpec.HalfWidth = MathF.Max(1f, aoe.Param2);
				drawSpec.OrientToFacing = true;
				break;
			case MapAoeKind.MovementArrow:
				drawSpec.Shape = QuickShape.Arrow;
				drawSpec.Radius = 5f;
				break;
			default:
				drawSpec.Shape = QuickShape.Circle;
				drawSpec.Radius = 5f;
				break;
		}

		if (aoe.IsSafe)
		{
			drawSpec.Color = new Vector4(0.2f, 0.9f, 0.6f, 0.4f);
		}

		_t = new QuickDrawDef
		{
			Name = name,
			Pattern = (actId != 0 ? name : ""),
			On = (actId != 0 ? TriggerMatch.Cast : TriggerMatch.Any),
			Source = SourceFilter.Enemy,
			OnlyOnSelf = false,
			MatchById = (actId != 0),
			DataId = actId,
			AnyZone = (terr == 0),
			Zones = (terr != 0 ? new List<uint> { terr } : new List<uint>()),
			Draw = drawSpec
		};
		QuickDrawEngine.EnsureIds(_t);
		_real = null;
		_owner = _plugin.Configuration.QuickModule();
		_isNew = true;
		_dirty = true;
		_sel = -1;
		_status = "";
		base.IsOpen = true;
	}

	private static void ApplyActionShape(DrawSpec d, uint actionId)
	{
		ActionShape.Geom? geom = ActionShape.Resolve(actionId);
		if (!geom.HasValue)
		{
			return;
		}
		ActionShape.Geom value = geom.Value;
		d.Shape = value.Shape;
		switch (value.Shape)
		{
		case QuickShape.Circle:
		case QuickShape.Donut:
			if (value.Radius > 0f)
			{
				d.Radius = value.Radius;
			}
			break;
		case QuickShape.Fan:
			if (value.Radius > 0f)
			{
				d.Radius = value.Radius;
			}
			if (value.FanAngle > 0)
			{
				d.FanAngle = value.FanAngle;
			}
			d.OrientToFacing = true;
			break;
		case QuickShape.Rectangle:
			if (value.Length > 0f)
			{
				d.Length = value.Length;
			}
			if (value.HalfWidth > 0f)
			{
				d.HalfWidth = value.HalfWidth;
			}
			d.OrientToFacing = true;
			break;
		}
	}

	private QuickDrawModule? FindOwner(QuickDrawDef t)
	{
		foreach (QuickDrawModule quickDrawModule in _plugin.Configuration.QuickDrawModules)
		{
			if (quickDrawModule.Draws.Contains(t))
			{
				return quickDrawModule;
			}
		}
		return null;
	}

	private void Commit()
	{
		Configuration configuration = _plugin.Configuration;
		if (_t == null)
		{
			return;
		}
		if (_isNew)
		{
			if (_owner == null)
			{
				_owner = configuration.QuickModule();
			}
			_owner.Draws.Add(_t);
			_isNew = false;
		}
		else if (_real != null && _owner != null)
		{
			int num = _owner.Draws.IndexOf(_real);
			if (num >= 0)
			{
				_owner.Draws[num] = _t;
			}
			else
			{
				_owner.Draws.Add(_t);
			}
		}
		else
		{
			(_owner ?? configuration.QuickModule()).Draws.Add(_t);
		}
		configuration.Save();
		_real = _t;
		_t = _t.Clone();
		_dirty = false;
		_status = "Saved";
	}

	public override void Draw()
	{
		if (_t == null)
		{
			ImGui.TextDisabled("No quick draw selected.");
			return;
		}
		QuickDrawDef t = _t;
		Configuration configuration = _plugin.Configuration;
		ProcessGroundPick();
		Ui.NavBar(_plugin, "");
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Gold, "Quick Draw");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(-1f);
		string buf = t.Name;
		if (ImGui.InputTextWithHint("##name", "draw name", ref buf, 64))
		{
			t.Name = buf;
			_dirty = true;
		}
		ImGui.SetNextItemWidth(200f * ImGuiHelpers.GlobalScale);
		string buf2 = t.Group;
		if (ImGui.InputTextWithHint("Section (group)", "optional, e.g. Phase 2", ref buf2, 48))
		{
			t.Group = buf2;
			_dirty = true;
		}
		if (_sel >= t.FollowUps.Count)
		{
			_sel = -1;
		}
		float globalScale = ImGuiHelpers.GlobalScale;
		float num = ImGui.GetFrameHeightWithSpacing() * 2f + 12f * globalScale;
		float num2 = ImGui.GetContentRegionAvail().Y - num;
		if (num2 < 120f * globalScale)
		{
			num2 = 120f * globalScale;
		}
		float x = ImGui.GetContentRegionAvail().X * 0.54f;
		if (ImGui.BeginChild("##cfg", new Vector2(x, num2), border: true))
		{
			Banner("WHEN IT FIRES", "the moment to react to");
			DrawMatch(t, configuration);
			Banner("WHERE IT WORKS", "limit it to one duty — optional");
			DrawZones(t);
			Banner("THEN…", "chain more draws — click one to edit its shape →");
			DrawStepList(t, configuration);
			ImGui.Spacing();
			ImGui.SetNextItemOpen(isOpen: false, ImGuiCond.Appearing);
			if (ImGui.CollapsingHeader("ADVANCED — cooldown, filters, remember-a-value"))
			{
				DrawAdvanced(t, configuration);
			}
		}
		ImGui.EndChild();
		ImGui.SameLine();
		if (ImGui.BeginChild("##editor", new Vector2(0f, num2), border: true))
		{
			DrawShapeColumn(t, configuration);
		}
		ImGui.EndChild();
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.BeginDisabled(!_dirty);
		if (ImGui.Button(_isNew ? "Save draw" : "Save changes"))
		{
			Commit();
		}
		ImGui.EndDisabled();
		ImGui.SameLine();
		if (ImGui.Button("Test"))
		{
			_plugin.Engine.Preview(t);
		}
		ImGui.SameLine();
		if (ImGui.Button("Cancel"))
		{
			base.IsOpen = false;
		}
		ImGui.SameLine();
		if (ImGui.Button("Copy share code"))
		{
			ImGui.SetClipboardText(ShareCodec.Encode("YAPDRAW1:", t));
			_status = "Share code copied";
		}
		ImGui.SameLine();
		if (ImGui.Button("Paste code"))
		{
			PasteCode();
		}
		if (_dirty)
		{
			ImGui.SameLine();
			ImGui.TextColored(in Ui.Gold, "● unsaved");
		}
		else if (!string.IsNullOrEmpty(_status))
		{
			ImGui.SameLine();
			ImGui.TextColored(in Ui.Blue, _status);
		}
	}

	private void DrawShapeColumn(QuickDrawDef t, Configuration cfg)
	{
		Banner("WHAT IT DRAWS", "the shape on the floor");
		DrawShapeEditor("main", t, t.Draw, () => t.DrawEnabled, delegate(bool v)
		{
			t.DrawEnabled = v;
		});
		DrawExtraShapes("main", t, t.ExtraShapes);
		for (int num = 0; num < t.FollowUps.Count; num++)
		{
			FollowUpStep s = t.FollowUps[num];
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			Banner($"FOLLOW-UP #{num + 1} DRAWS", StepSummary(s, num + 1));
			DrawShapeEditor(s.Id, t, s.Draw, () => s.DrawEnabled, delegate(bool v)
			{
				s.DrawEnabled = v;
			});
			DrawExtraShapes(s.Id, t, s.ExtraShapes);
		}
	}

	private void DrawExtraShapes(string idPrefix, QuickDrawDef t, List<DrawSpec> shapes)
	{
		DrawSpec drawSpec = null;
		for (int i = 0; i < shapes.Count; i++)
		{
			ImGui.PushID(idPrefix + "_x" + i);
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			Banner($"EXTRA SHAPE #{i + 1}", "drawn together with the shapes above");
			if (ImGui.SmallButton("Remove this shape"))
			{
				drawSpec = shapes[i];
			}
			DrawShapeBody(t, shapes[i], ImGuiHelpers.GlobalScale);
			ImGui.PopID();
		}
		if (drawSpec != null)
		{
			shapes.Remove(drawSpec);
			_dirty = true;
		}
		ImGui.Spacing();
		ImU8String label = new ImU8String(24, 1);
		label.AppendLiteral("+ Add another shape##add");
		label.AppendFormatted(idPrefix);
		if (ImGui.SmallButton(label))
		{
			shapes.Add(new DrawSpec());
			_dirty = true;
		}
	}

	private void DrawShapeEditor(string id, QuickDrawDef t, DrawSpec d, Func<bool> getEnabled, Action<bool> setEnabled)
	{
		ImGui.PushID(id);
		float globalScale = ImGuiHelpers.GlobalScale;
		bool v = getEnabled();
		if (ImGui.Checkbox("Draw a shape", ref v))
		{
			setEnabled(v);
			_dirty = true;
		}
		if (getEnabled())
		{
			ImGui.SameLine();
			if (ImGui.SmallButton("Test this shape"))
			{
				d.EnsureId();
				_plugin.Engine.PreviewShape(t, d);
			}
			DrawShapeBody(t, d, globalScale);
		}
		ImGui.PopID();
	}

	private void DrawShapeBody(QuickDrawDef t, DrawSpec d, float scale)
	{
		ImGui.Indent(8f * scale);
		Ui.SectionHeader(FontAwesomeIcon.Shapes, "Look");
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Dimmed, "Shape");
		int selected = (int)d.Shape;
		bool flag;
		if (StratUI.SegmentedBarWrapped(ShapeNames, ref selected))
		{
			QuickShape quickShape = (QuickShape)selected;
			if (quickShape == QuickShape.Line && d.Shape != QuickShape.Line)
			{
				if (d.HalfWidth > 1.5f)
				{
					d.HalfWidth = 0.5f;
				}
				d.Link = LinkTarget.FixedSpot;
			}
			flag = quickShape - 2 <= QuickShape.Donut;
			if (flag && d.Shape != quickShape)
			{
				d.OrientToFacing = true;
			}
			flag = quickShape - 9 <= QuickShape.Donut;
			if (flag && d.Shape != quickShape)
			{
				d.OrientToFacing = false;
				if (d.Link == LinkTarget.EventTarget)
				{
					d.Link = LinkTarget.NearestPlayer;
				}
			}
			d.Shape = quickShape;
			_dirty = true;
		}
		bool flag2 = d.Shape == QuickShape.Text;
		if (!flag2)
		{
			Vector4 col = d.Color;
			if (ImGui.ColorEdit4("##col", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaPreview))
			{
				d.Color = col;
				_dirty = true;
			}
			ImGui.SameLine();
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(in Ui.Dimmed, "colour");
			ImGui.SameLine(0f, 14f);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(in Ui.Dimmed, "presets:");
			(string, Vector4)[] colorPresets = ColorPresets;
			for (int i = 0; i < colorPresets.Length; i++)
			{
				var (text, col2) = colorPresets[i];
				ImGui.SameLine(0f, 4f);
				ImU8String descId = new ImU8String(4, 1);
				descId.AppendFormatted(text);
				descId.AppendLiteral("##sw");
				if (ImGui.ColorButton(descId, in col2, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.AlphaPreview, new Vector2(18f * scale, 18f * scale)))
				{
					d.Color = col2;
					_dirty = true;
				}
				if (ImGui.IsItemHovered())
				{
					ImGui.SetTooltip(text);
				}
			}
			DragF("Transparency", () => d.Color.W, delegate(float w)
			{
				DrawSpec drawSpec = d;
				Vector4 color = d.Color;
				color.W = w;
				drawSpec.Color = color;
			}, 0.01f, 0.05f, 1f, "%.2f");
		}
		else
		{
			ImGui.TextColored(in Ui.Dimmed, "Floating text only — set the words below.");
		}
		if (!flag2)
		{
			ImGui.Spacing();
			Ui.SectionHeader(FontAwesomeIcon.RulerCombined, "Size");
			DrawShapeDims(d, scale);
			DrawRadialCopies(d);
		}
		ImGui.Spacing();
		DrawPlacement(t, d, scale);
		QuickShape shape = d.Shape;
		flag = ((shape == QuickShape.Line || shape - 9 <= QuickShape.Donut) ? true : false);
		if (flag || (d.Shape == QuickShape.Rectangle && d.SpanToTarget))
		{
			DrawLinkPicker(t, d, scale);
		}
		if (!flag2)
		{
			DrawOffsets(d);
		}
		ImGui.Spacing();
		Ui.SectionHeader(FontAwesomeIcon.Clock, "Timing");
		bool v = d.UseEventDuration;
		if (ImGui.Checkbox("Match the cast / debuff time", ref v))
		{
			d.UseEventDuration = v;
			_dirty = true;
		}
		if (!d.UseEventDuration)
		{
			DragF(flag2 ? "Seconds visible" : "Seconds on floor", () => d.Duration, delegate(float duration)
			{
				d.Duration = duration;
			}, 0.1f, 0.2f, 120f, "%.1fs");
		}
		DragF("Start delay (s)", () => d.StartDelay, delegate(float startDelay)
		{
			d.StartDelay = startDelay;
		}, 0.1f, 0f, 60f, "%.1fs");
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Wait this long after the trigger before this shape appears.");
		}
		if (flag2)
		{
			ImGui.Spacing();
			Ui.SectionHeader(FontAwesomeIcon.Font, "Text");
			DrawTextFields(d, scale, required: true);
		}
		else
		{
			ImGui.Spacing();
			Ui.SectionHeader(FontAwesomeIcon.Font, "Label");
			DrawTextFields(d, scale, required: false);
		}
		ImGui.Unindent(8f * scale);
	}

	private void DrawRadialCopies(DrawSpec d)
	{
		DragI("Copies around anchor", () => d.Repeat, delegate(int v)
		{
			d.Repeat = v;
		}, 1, 1, 36);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Spawn the same shape several times around the anchor (e.g. 8 fans around the boss).");
		}
		if (d.Repeat > 1)
		{
			DragF("Angle between copies (°)", () => d.RepeatStep, delegate(float v)
			{
				d.RepeatStep = v;
			}, 1f, -360f, 360f, "%.0f");
		}
	}

	private void DrawTextFields(DrawSpec d, float scale, bool required)
	{
		string buf = d.Label;
		ImGui.SetNextItemWidth(220f * scale);
		if (ImGui.InputTextWithHint("##label", required ? "text to show" : "label text (optional)", ref buf, 64))
		{
			d.Label = buf;
			_dirty = true;
		}
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Dimmed, required ? "the words" : "floating text");
		if (required || !string.IsNullOrWhiteSpace(d.Label))
		{
			Vector4 col = d.LabelColor;
			if (ImGui.ColorEdit4("##labelcol", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaPreview))
			{
				d.LabelColor = col;
				_dirty = true;
			}
			ImGui.SameLine();
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(in Ui.Dimmed, "text colour");
			DragF("Text size", () => d.LabelSize, delegate(float v)
			{
				d.LabelSize = v;
			}, 0.05f, 0.3f, 5f, "%.2fx");
			DragF("Height up (y)", () => d.LabelHeight, delegate(float v)
			{
				d.LabelHeight = v;
			}, 0.1f, 0f, 40f, "%.1fy");
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("How high above the spot the text floats.");
			}
		}
	}

	private void DrawPlacement(QuickDrawDef t, DrawSpec d, float scale)
	{
		Ui.SectionHeader(FontAwesomeIcon.MapMarkerAlt, "Place it");
		int currentItem = Array.FindIndex(AnchorOpts, ((DrawAnchor V, string Label) o) => o.V == d.Anchor);
		if (currentItem < 0)
		{
			currentItem = 0;
		}
		ImGui.SetNextItemWidth(220f * scale);
		if (ImGui.Combo("##anchor", ref currentItem, AnchorLabels, AnchorLabels.Length))
		{
			d.Anchor = AnchorOpts[currentItem].V;
			_dirty = true;
		}
		if (d.Anchor == DrawAnchor.FixedPosition)
		{
			DrawSpotPicker("anchor spot", () => d.FixedPosition, delegate(Vector3 p)
			{
				d.FixedPosition = p;
				_dirty = true;
			}, scale);
			return;
		}
		if (d.Anchor == DrawAnchor.LinkedShape)
		{
			DrawShapeRefPicker(t, d, () => d.AnchorShapeId, delegate(string anchorShapeId)
			{
				d.AnchorShapeId = anchorShapeId;
			}, scale);
			return;
		}
		if (d.Anchor == DrawAnchor.NearbyActorById)
		{
			int data = (int)d.AnchorActorBaseId;
			ImGui.SetNextItemWidth(160f * scale);
			if (ImGui.InputInt("Actor base id", ref data))
			{
				d.AnchorActorBaseId = (uint)Math.Max(0, data);
				_dirty = true;
			}
			bool v = d.AttachToActor;
			if (ImGui.Checkbox("Stick to the actor (follows them)", ref v))
			{
				d.AttachToActor = v;
				_dirty = true;
			}
			return;
		}
		DrawAnchor anchor = d.Anchor;
		if (anchor <= DrawAnchor.Self)
		{
			bool v2 = d.AttachToActor;
			if (ImGui.Checkbox("Stick to the actor (follows them)", ref v2))
			{
				d.AttachToActor = v2;
				_dirty = true;
			}
		}
	}

	private void DrawLinkPicker(QuickDrawDef t, DrawSpec d, float scale)
	{
		ImGui.Spacing();
		Ui.SectionHeader(FontAwesomeIcon.Link, "Connect to");
		int currentItem = Array.FindIndex(LinkOpts, ((LinkTarget V, string Label) o) => o.V == d.Link);
		if (currentItem < 0)
		{
			currentItem = 0;
		}
		ImGui.SetNextItemWidth(220f * scale);
		if (ImGui.Combo("##link", ref currentItem, LinkLabels, LinkLabels.Length))
		{
			d.Link = LinkOpts[currentItem].V;
			if (d.Link == LinkTarget.LinkedShape && string.IsNullOrEmpty(d.LinkShapeId))
			{
				List<(string, string)> list = CollectShapeOptions(t, d.Id);
				if (list.Count > 0)
				{
					d.LinkShapeId = list[0].Item1;
				}
			}
			_dirty = true;
		}
		if (d.Link == LinkTarget.FixedSpot)
		{
			DrawSpotPicker("far end", () => d.LinkPosition, delegate(Vector3 p)
			{
				d.LinkPosition = p;
				_dirty = true;
			}, scale);
		}
		else if (d.Link == LinkTarget.LinkedShape)
		{
			DrawShapeRefPicker(t, d, () => d.LinkShapeId, delegate(string v)
			{
				d.LinkShapeId = v;
			}, scale);
		}
	}

	private void DrawShapeRefPicker(QuickDrawDef t, DrawSpec self, Func<string> get, Action<string> set, float scale)
	{
		self.EnsureId();
		List<(string, string)> list = CollectShapeOptions(t, self.Id);
		if (list.Count == 0)
		{
			ImGui.TextColored(in Ui.Dimmed, "Add another shape first, then pick it here.");
			return;
		}
		string[] array = Array.ConvertAll(list.ToArray(), ((string Id, string Label) o) => o.Label);
		int currentItem = Array.FindIndex(list.ToArray(), ((string Id, string Label) o) => o.Id == get());
		if (currentItem < 0)
		{
			currentItem = 0;
		}
		if (string.IsNullOrEmpty(get()))
		{
			set(list[currentItem].Item1);
			_dirty = true;
		}
		ImGui.SetNextItemWidth(260f * scale);
		if (ImGui.Combo("##shaperef", ref currentItem, array, array.Length))
		{
			set(list[currentItem].Item1);
			_dirty = true;
		}
	}

	private static void AddShapeOption(List<(string Id, string Label)> list, DrawSpec d, string slot, string excludeId)
	{
		d.EnsureId();
		if (!(d.Id == excludeId))
		{
			string label = d.Label;
			string text = (string.IsNullOrWhiteSpace(label) ? d.Shape.ToString() : $"{d.Shape} \"{label}\"");
			list.Add((d.Id, slot + ": " + text));
		}
	}

	private static List<(string Id, string Label)> CollectShapeOptions(QuickDrawDef t, string excludeId)
	{
		List<(string, string)> list = new List<(string, string)>();
		AddShapeOption(list, t.Draw, "Main", excludeId);
		for (int i = 0; i < t.ExtraShapes.Count; i++)
		{
			AddShapeOption(list, t.ExtraShapes[i], $"Extra {i + 1}", excludeId);
		}
		for (int j = 0; j < t.FollowUps.Count; j++)
		{
			FollowUpStep followUpStep = t.FollowUps[j];
			AddShapeOption(list, followUpStep.Draw, $"Follow-up {j + 1}", excludeId);
			for (int k = 0; k < followUpStep.ExtraShapes.Count; k++)
			{
				AddShapeOption(list, followUpStep.ExtraShapes[k], $"Follow-up {j + 1} extra {k + 1}", excludeId);
			}
		}
		return list;
	}

	private void DrawShapeDims(DrawSpec d, float scale)
	{
		switch (d.Shape)
		{
		case QuickShape.Circle:
			DragF("Radius (y)", () => d.Radius, delegate(float radius)
			{
				d.Radius = radius;
			}, 0.1f, 0.5f, 60f);
			break;
		case QuickShape.Donut:
			DragF("Inner radius (y)", () => d.InnerRadius, delegate(float innerRadius)
			{
				d.InnerRadius = innerRadius;
			}, 0.1f, 0f, 60f);
			DragF("Outer radius (y)", () => d.Radius, delegate(float radius)
			{
				d.Radius = radius;
			}, 0.1f, 0.5f, 60f);
			break;
		case QuickShape.Fan:
			DragF("Length (y)", () => d.Radius, delegate(float radius)
			{
				d.Radius = radius;
			}, 0.1f, 0.5f, 60f);
			DragI("Angle (°)", () => d.FanAngle, delegate(int fanAngle)
			{
				d.FanAngle = fanAngle;
			}, 1, 5, 360);
			DrawFacing(d);
			break;
		case QuickShape.Rectangle:
		{
			bool v = d.SpanToTarget;
			if (ImGui.Checkbox("Span from anchor to a far point (A→B)", ref v))
			{
				d.SpanToTarget = v;
				if (v && d.Link == LinkTarget.EventTarget)
				{
					d.Link = LinkTarget.FixedSpot;
				}
				_dirty = true;
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("On: the rectangle stretches from the anchor to a second point (set under \"Connect to\"), like a wide line.\nOff: a fixed length in a set facing.");
			}
			DragF("Half-width (y)", () => d.HalfWidth, delegate(float halfWidth)
			{
				d.HalfWidth = halfWidth;
			}, 0.1f, 0.5f, 60f);
			if (d.SpanToTarget)
			{
				ImGui.TextColored(in Ui.Dimmed, "Length stretches automatically to the far point.");
				break;
			}
			DragF("Length (y)", () => d.Length, delegate(float length)
			{
				d.Length = length;
			}, 0.1f, 0.5f, 100f);
			DrawFacing(d);
			break;
		}
		case QuickShape.Line:
			DragF("Half-width (y)", () => d.HalfWidth, delegate(float halfWidth)
			{
				d.HalfWidth = halfWidth;
			}, 0.1f, 0.2f, 30f);
			ImGui.TextColored(in Ui.Dimmed, "Length stretches automatically to the far end.");
			break;
		case QuickShape.Arrow:
			DragF("Line thickness (px)", () => d.LineThickness, delegate(float lineThickness)
			{
				d.LineThickness = lineThickness;
			}, 0.5f, 1f, 20f);
			DragF("Arrowhead size (y)", () => d.HalfWidth, delegate(float halfWidth)
			{
				d.HalfWidth = halfWidth;
			}, 0.1f, 0.5f, 10f);
			DragF("Length (y)", () => d.Length, delegate(float length)
			{
				d.Length = length;
			}, 0.1f, 1f, 100f);
			ImGui.TextColored(in Ui.Dimmed, "Connect it to a target and the length reaches it automatically.");
			DrawFacing(d);
			break;
		case QuickShape.ChevronPath:
			DragF("Line thickness (px)", () => d.LineThickness, delegate(float lineThickness)
			{
				d.LineThickness = lineThickness;
			}, 0.5f, 1f, 20f);
			DragF("Chevron spacing (y)", () => d.ChevronSpacing, delegate(float chevronSpacing)
			{
				d.ChevronSpacing = chevronSpacing;
			}, 0.1f, 0.5f, 20f);
			DragF("Length (y)", () => d.Length, delegate(float length)
			{
				d.Length = length;
			}, 0.1f, 1f, 100f);
			ImGui.TextColored(in Ui.Dimmed, "Connect it to a target and the length reaches it automatically.");
			DrawFacing(d);
			break;
		case QuickShape.Tower:
			DragF("Radius (y)", () => d.Radius, delegate(float radius)
			{
				d.Radius = radius;
			}, 0.1f, 0.5f, 30f);
			ImGui.TextColored(in Ui.Dimmed, "Stand-here soak marker.");
			break;
		case QuickShape.Knockback:
			DragF("Radius (y)", () => d.Radius, delegate(float radius)
			{
				d.Radius = radius;
			}, 0.1f, 0.5f, 60f);
			DrawFacing(d);
			break;
		case QuickShape.Laser:
			DragF("Length (y)", () => d.Length, delegate(float length)
			{
				d.Length = length;
			}, 0.1f, 0.5f, 100f);
			DragF("Half-width (y)", () => d.HalfWidth, delegate(float halfWidth)
			{
				d.HalfWidth = halfWidth;
			}, 0.1f, 0.2f, 60f);
			DrawFacing(d);
			break;
		case QuickShape.Text:
			break;
		}
	}

	private void DrawFacing(DrawSpec d)
	{
		bool v = d.OrientToFacing;
		if (ImGui.Checkbox("Spin with the actor's facing", ref v))
		{
			d.OrientToFacing = v;
			_dirty = true;
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("On: the shape turns as the actor (you, the caster, the target) turns.\nThe angle below becomes an offset from straight-ahead.\nOff: the angle is a fixed compass bearing (0 = north).");
		}
		DragF(d.OrientToFacing ? "Offset from facing (°)" : "Facing (°)", () => d.Rotation, delegate(float rotation)
		{
			d.Rotation = rotation;
		}, 1f, -360f, 360f);
	}

	private void DrawOffsets(DrawSpec d)
	{
		ImGui.Spacing();
		ImGui.TextColored(in Ui.Dimmed, "Nudge (relative to facing when spinning, else world):");
		DragF("Forward (y)", () => d.OffsetForward, delegate(float v)
		{
			d.OffsetForward = v;
		}, 0.1f, -40f, 40f);
		ImGui.SameLine();
		DragF("Side (y)", () => d.OffsetSide, delegate(float v)
		{
			d.OffsetSide = v;
		}, 0.1f, -40f, 40f);
	}

	public void TickGroundPick()
	{
		if (_groundPick == null)
		{
			base.IsClickthrough = false;
			_wasLmbDown = false;
			_wasEscDown = false;
			_groundPickGrace = 0;
			return;
		}
		base.IsClickthrough = true;
		bool flag = (GetAsyncKeyState(27) & 0x8000) != 0;
		if (flag && !_wasEscDown)
		{
			_groundPick = null;
			_wasEscDown = flag;
			return;
		}
		_wasEscDown = flag;
		bool flag2 = (GetAsyncKeyState(1) & 0x8000) != 0;
		if (_groundPickGrace > 0)
		{
			_groundPickGrace--;
			_wasLmbDown = flag2;
			return;
		}
		if (flag2 && !_wasLmbDown && GetCursorPos(out var pt))
		{
			Vector2 screenPos = new Vector2(pt.X, pt.Y);
			if (Plugin.GameGui.ScreenToWorld(screenPos, out var worldPos))
			{
				_groundPick(new Vector3(MathF.Round(worldPos.X, 2), 0f, MathF.Round(worldPos.Z, 2)));
				_dirty = true;
			}
			_groundPick = null;
		}
		_wasLmbDown = flag2;
	}

	private void ProcessGroundPick()
	{
		if (_groundPick != null)
		{
			ImDrawListPtr foregroundDrawList = ImGui.GetForegroundDrawList();
			Vector2 mousePos = ImGui.GetMousePos();
			foregroundDrawList.AddCircle(mousePos, 9f, ImGui.ColorConvertFloat4ToU32(Ui.Accent), 16, 2f);
			foregroundDrawList.AddText(new Vector2(mousePos.X + 14f, mousePos.Y + 2f), ImGui.ColorConvertFloat4ToU32(Ui.Gold), "click the ground  (Esc to cancel)");
		}
	}

	private void DrawSpotPicker(string id, Func<Vector3> get, Action<Vector3> set, float scale)
	{
		ImGui.PushID(id);
		bool flag = _groundPick != null;
		if (flag)
		{
			Vector4 accent = Ui.Accent;
			accent.W = 0.9f;
			ImGui.PushStyleColor(ImGuiCol.Button, accent);
		}
		if (ImGui.SmallButton(flag ? "Picking…" : "Pick on ground"))
		{
			if (flag)
			{
				_groundPick = null;
			}
			else
			{
				_groundPick = set;
				_groundPickGrace = 3;
			}
		}
		if (flag)
		{
			ImGui.PopStyleColor();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Click here, then click a spot on the ground in-game to grab its coordinates.");
		}
		ArenaPad.Draw(id, _plugin, get, set, scale, _padSnapGrid, delegate(bool v)
		{
			_padSnapGrid = v;
		}, delegate
		{
			_dirty = true;
		});
		DrawCoordHelper(get, set, scale);
		ImGui.PopID();
	}

	private void DrawCoordHelper(Func<Vector3> get, Action<Vector3> set, float scale)
	{
		Vector3 vector = get();
		float num = vector.X - 100f;
		float num2 = vector.Z - 100f;
		float v = MathF.Sqrt(num * num + num2 * num2);
		float v2 = MathF.Atan2(num, 0f - num2) * 180f / (float)Math.PI;
		if (v2 < 0f)
		{
			v2 += 360f;
		}
		ImGui.SetNextItemWidth(90f * scale);
		if (ImGui.DragFloat("dist##ch", ref v, 0.1f, 0f, 30f, "%.1fy", ImGuiSliderFlags.AlwaysClamp))
		{
			SetPolar(set, v, v2);
		}
		ImGui.SameLine();
		ImGui.SetNextItemWidth(90f * scale);
		if (ImGui.DragFloat("bearing##ch", ref v2, 1f, 0f, 360f, "%.0f°"))
		{
			SetPolar(set, v, v2);
		}
		ImGui.SameLine();
		ImGui.TextColored(in Ui.Dimmed, "from centre");
		string[] array = new string[8] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				ImGui.SameLine();
			}
			ImU8String label = new ImU8String(4, 2);
			label.AppendFormatted(array[i]);
			label.AppendLiteral("##ch");
			label.AppendFormatted(i);
			if (ImGui.SmallButton(label))
			{
				SetPolar(set, (v <= 0.1f) ? 15f : v, (float)i * 45f);
			}
		}
	}

	private void SetPolar(Action<Vector3> set, float dist, float bearing)
	{
		float x = bearing * (float)Math.PI / 180f;
		float x2 = 100f + dist * MathF.Sin(x);
		float x3 = 100f - dist * MathF.Cos(x);
		set(new Vector3(MathF.Round(x2, 2), 0f, MathF.Round(x3, 2)));
		_dirty = true;
	}

	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);

	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(out Point pt);

	private bool DragF(string label, Func<float> get, Action<float> set, float step, float min, float max, string fmt = "%.1f")
	{
		float v = get();
		ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
		if (ImGui.DragFloat(label, ref v, step, min, max, fmt, ImGuiSliderFlags.AlwaysClamp))
		{
			set(v);
			_dirty = true;
			return true;
		}
		return false;
	}

	private bool DragI(string label, Func<int> get, Action<int> set, int step, int min, int max)
	{
		int v = get();
		ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
		if (ImGui.DragInt(label, ref v, step, min, max))
		{
			set(Math.Clamp(v, min, max));
			_dirty = true;
			return true;
		}
		return false;
	}

	private void DrawIconPreview(uint iconId, float size)
	{
		if (!_iconPreview.TryGetValue(iconId, out ISharedImmediateTexture value))
		{
			value = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId));
			_iconPreview[iconId] = value;
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

	private static bool AdvSection(string title, int count)
	{
		return ImGui.CollapsingHeader((count > 0) ? $"{title}   ({count})###{title}" : (title + "###" + title));
	}

	private void DrawAdvanced(QuickDrawDef t, Configuration cfg)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		if (AdvSection("Cooldown & overlap", 0))
		{
			ImGui.Indent(8f * globalScale);
			float secs = t.Cooldown;
			if (TimeDrag("Cooldown (0 = default)", ref secs, 120f, 170f))
			{
				t.Cooldown = secs;
				_dirty = true;
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("Shortest gap between two draws from this trigger.");
			}
			ImGui.TextColored(in Ui.Dimmed, "If it fires again while the shape is still showing:");
			int currentItem = (int)((!t.NoReentry || t.Concurrency != Concurrency.Stack) ? t.Concurrency : Concurrency.Wait);
			ImGui.SetNextItemWidth(260f * globalScale);
			if (ImGui.Combo("##conc", ref currentItem, ConcurrencyNames, ConcurrencyNames.Length))
			{
				t.Concurrency = (Concurrency)currentItem;
				t.NoReentry = t.Concurrency == Concurrency.Wait;
				_dirty = true;
			}
			ImGui.Unindent(8f * globalScale);
			ImGui.Spacing();
		}
		if (AdvSection("Remove the shape early", t.ClearOn.Enabled ? 1 : 0))
		{
			ImGui.Indent(8f * globalScale);
			ImGui.TextColored(in Ui.Dimmed, "Wipe the shape the moment a matching event lands — e.g. clear the telegraph as soon as the cast goes off.");
			ClearRule clearOn = t.ClearOn;
			bool v = clearOn.Enabled;
			if (ImGui.Checkbox("Clear the shape when…", ref v))
			{
				clearOn.Enabled = v;
				_dirty = true;
			}
			if (clearOn.Enabled)
			{
				ImGui.Indent(8f * globalScale);
				int currentItem2 = (int)(clearOn.On - 1);
				if (currentItem2 < 0)
				{
					currentItem2 = 0;
				}
				ImGui.SetNextItemWidth(160f * globalScale);
				if (ImGui.Combo("##clrev", ref currentItem2, ClearEventNames, ClearEventNames.Length))
				{
					clearOn.On = (FollowUpOn)(currentItem2 + 1);
					_dirty = true;
				}
				ImGui.SameLine();
				string buf = clearOn.Pattern;
				ImGui.SetNextItemWidth(180f * globalScale);
				if (ImGui.InputTextWithHint("##clrpat", "name (blank = same as above)", ref buf, 128))
				{
					clearOn.Pattern = buf;
					clearOn.MatchById = false;
					_dirty = true;
				}
				bool v2 = clearOn.OnlyOnSelf;
				if (ImGui.Checkbox("only when it's on me", ref v2))
				{
					clearOn.OnlyOnSelf = v2;
					_dirty = true;
				}
				ImGui.SameLine();
				float v3 = clearOn.Seconds;
				ImGui.SetNextItemWidth(120f * globalScale);
				if (ImGui.DragFloat("within (s)", ref v3, 0.5f, 1f, 120f, "%.0fs", ImGuiSliderFlags.AlwaysClamp))
				{
					clearOn.Seconds = v3;
					_dirty = true;
				}
				ImGui.Unindent(8f * globalScale);
			}
			ImGui.Unindent(8f * globalScale);
			ImGui.Spacing();
		}
		if (AdvSection("Only draw if a number matches", t.NumConds.Count))
		{
			ImGui.Indent(8f * globalScale);
			ImGui.TextColored(in Ui.Dimmed, "e.g. only when the stack count is 3, or the caster is below 20% HP. Every rule must pass.");
			int num = -1;
			for (int i = 0; i < t.NumConds.Count; i++)
			{
				NumCond numCond = t.NumConds[i];
				ImU8String strId = new ImU8String(3, 1);
				strId.AppendLiteral("num");
				strId.AppendFormatted(i);
				ImGui.PushID(strId);
				int currentItem3 = (int)numCond.Field;
				ImGui.SetNextItemWidth(155f * globalScale);
				if (ImGui.Combo("##f", ref currentItem3, NumFieldNames, NumFieldNames.Length))
				{
					numCond.Field = (NumField)currentItem3;
					_dirty = true;
				}
				ImGui.SameLine();
				int currentItem4 = (int)numCond.Op;
				ImGui.SetNextItemWidth(95f * globalScale);
				if (ImGui.Combo("##op", ref currentItem4, NumOpNames, NumOpNames.Length))
				{
					numCond.Op = (NumOp)currentItem4;
					_dirty = true;
				}
				ImGui.SameLine();
				float data = numCond.Value;
				ImGui.SetNextItemWidth(90f * globalScale);
				if (ImGui.InputFloat("##v", ref data))
				{
					numCond.Value = data;
					_dirty = true;
				}
				ImGui.SameLine();
				if (ImGui.SmallButton("remove##n"))
				{
					num = i;
				}
				ImGui.PopID();
			}
			if (num >= 0)
			{
				t.NumConds.RemoveAt(num);
				_dirty = true;
			}
			if (ImGui.SmallButton("+ add a number rule"))
			{
				t.NumConds.Add(new NumCond());
				_dirty = true;
			}
			ImGui.Unindent(8f * globalScale);
			ImGui.Spacing();
		}
		if (AdvSection("Only draw if a status matches", t.StatusGates.Count))
		{
			ImGui.Indent(8f * globalScale);
			ImGui.TextColored(in Ui.Dimmed, "e.g. only when you have Dark Resistance Down. Every rule must pass.");
			int num2 = -1;
			for (int j = 0; j < t.StatusGates.Count; j++)
			{
				StatusGate statusGate = t.StatusGates[j];
				ImU8String strId2 = new ImU8String(2, 1);
				strId2.AppendLiteral("st");
				strId2.AppendFormatted(j);
				ImGui.PushID(strId2);
				int currentItem5 = (int)statusGate.Who;
				ImGui.SetNextItemWidth(90f * globalScale);
				if (ImGui.Combo("##who", ref currentItem5, StatusWhoNames, StatusWhoNames.Length))
				{
					statusGate.Who = (StatusGateWho)currentItem5;
					_dirty = true;
				}
				ImGui.SameLine();
				bool v4 = statusGate.Have;
				if (ImGui.Checkbox("has", ref v4))
				{
					statusGate.Have = v4;
					_dirty = true;
				}
				ImGui.SameLine();
				int data2 = (int)statusGate.StatusId;
				ImGui.SetNextItemWidth(100f * globalScale);
				if (ImGui.InputInt("id##st", ref data2))
				{
					statusGate.StatusId = (uint)Math.Max(0, data2);
					_dirty = true;
				}
				ImGui.SameLine();
				string buf2 = statusGate.Name;
				ImGui.SetNextItemWidth(140f * globalScale);
				if (ImGui.InputTextWithHint("##stn", "name", ref buf2, 64))
				{
					statusGate.Name = buf2;
					_dirty = true;
				}
				ImGui.SameLine();
				if (ImGui.SmallButton("remove##st"))
				{
					num2 = j;
				}
				ImGui.PopID();
			}
			if (num2 >= 0)
			{
				t.StatusGates.RemoveAt(num2);
				_dirty = true;
			}
			if (ImGui.SmallButton("+ add a status rule"))
			{
				t.StatusGates.Add(new StatusGate());
				_dirty = true;
			}
			ImGui.Unindent(8f * globalScale);
			ImGui.Spacing();
		}
		if (!AdvSection("Remember a value (link two draws)", t.SetVars.Count + t.VarConds.Count))
		{
			return;
		}
		ImGui.Indent(8f * globalScale);
		ImGui.TextColored(in Ui.Dimmed, "One draw writes a note; another reads it later to decide whether / where to draw.");
		ImGui.TextColored(in Ui.Blue, "e.g. on \"Bomb\" debuff save  bomb = {target} ,  then on \"Tower\" only draw if  bomb is {target}");
		ImGui.Spacing();
		ImGui.Text("When this draws, save a note:");
		int num3 = -1;
		for (int k = 0; k < t.SetVars.Count; k++)
		{
			VarAction varAction = t.SetVars[k];
			ImU8String strId3 = new ImU8String(3, 1);
			strId3.AppendLiteral("set");
			strId3.AppendFormatted(k);
			ImGui.PushID(strId3);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(in Ui.Dimmed, "call it");
			ImGui.SameLine();
			string buf3 = varAction.Name;
			ImGui.SetNextItemWidth(110f * globalScale);
			if (ImGui.InputTextWithHint("##n", "bomb", ref buf3, 32))
			{
				varAction.Name = buf3;
				_dirty = true;
			}
			ImGui.SameLine();
			int currentItem6 = (int)varAction.Op;
			ImGui.SetNextItemWidth(95f * globalScale);
			if (ImGui.Combo("##sop", ref currentItem6, VarOpNames, VarOpNames.Length))
			{
				varAction.Op = (VarOp)currentItem6;
				_dirty = true;
			}
			ImGui.SameLine();
			string buf4 = varAction.Value;
			ImGui.SetNextItemWidth(150f * globalScale);
			if (ImGui.InputTextWithHint("##sv", "{target}", ref buf4, 64))
			{
				varAction.Value = buf4;
				_dirty = true;
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("remove##s"))
			{
				num3 = k;
			}
			ImGui.PopID();
		}
		if (num3 >= 0)
		{
			t.SetVars.RemoveAt(num3);
			_dirty = true;
		}
		if (ImGui.SmallButton("+ save a note"))
		{
			t.SetVars.Add(new VarAction
			{
				Value = "{target}"
			});
			_dirty = true;
		}
		ImGui.Spacing();
		ImGui.Text("Only draw if a saved note matches:");
		int num4 = -1;
		for (int l = 0; l < t.VarConds.Count; l++)
		{
			VarCond varCond = t.VarConds[l];
			ImU8String strId4 = new ImU8String(2, 1);
			strId4.AppendLiteral("vc");
			strId4.AppendFormatted(l);
			ImGui.PushID(strId4);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(in Ui.Dimmed, "the note");
			ImGui.SameLine();
			string buf5 = varCond.Name;
			ImGui.SetNextItemWidth(110f * globalScale);
			if (ImGui.InputTextWithHint("##vn", "bomb", ref buf5, 32))
			{
				varCond.Name = buf5;
				_dirty = true;
			}
			ImGui.SameLine();
			int currentItem7 = (int)varCond.Op;
			ImGui.SetNextItemWidth(95f * globalScale);
			if (ImGui.Combo("##vop", ref currentItem7, NumOpNames, NumOpNames.Length))
			{
				varCond.Op = (NumOp)currentItem7;
				_dirty = true;
			}
			ImGui.SameLine();
			string buf6 = varCond.Value;
			ImGui.SetNextItemWidth(150f * globalScale);
			if (ImGui.InputTextWithHint("##vv", "{target}", ref buf6, 64))
			{
				varCond.Value = buf6;
				_dirty = true;
			}
			ImGui.SameLine();
			bool v5 = varCond.Numeric;
			if (ImGui.Checkbox("123", ref v5))
			{
				varCond.Numeric = v5;
				_dirty = true;
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("compare as numbers instead of text");
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("remove##v"))
			{
				num4 = l;
			}
			ImGui.PopID();
		}
		if (num4 >= 0)
		{
			t.VarConds.RemoveAt(num4);
			_dirty = true;
		}
		if (ImGui.SmallButton("+ require a note"))
		{
			t.VarConds.Add(new VarCond
			{
				Value = "{target}"
			});
			_dirty = true;
		}
		ImGui.Unindent(8f * globalScale);
		ImGui.Spacing();
	}

	private void PasteCode()
	{
		string clipboardText = ImGui.GetClipboardText();
		if (_t != null && ShareCodec.TryDecode<QuickDrawDef>("YAPDRAW1:", clipboardText, out QuickDrawDef value) && value != null)
		{
			value.Id = _t.Id;
			_t = value;
			_dirty = true;
			_status = "Loaded from code";
		}
		else
		{
			_status = "Clipboard isn't a quick-draw code";
		}
	}

	private void DrawMatch(QuickDrawDef t, Configuration cfg)
	{
		int index = (int)t.On;
		if (Combo("Event", MatchNames, ref index))
		{
			t.On = (TriggerMatch)index;
			_dirty = true;
		}
		if (t.On == TriggerMatch.Chat)
		{
			ImGui.SameLine();
			ImGui.TextDisabled("fires on a chat / battle-log line");
			string buf = t.Pattern;
			ImGui.SetNextItemWidth(-1f);
			if (ImGui.InputTextWithHint("##chatpat", "text or regex to match in the line", ref buf, 256))
			{
				t.Pattern = buf;
				_dirty = true;
			}
			ImGui.SameLine();
			bool v = t.UseRegex;
			if (ImGui.Checkbox("regex", ref v))
			{
				t.UseRegex = v;
				_dirty = true;
			}
			float secs = t.DelaySeconds;
			if (TimeDrag("Delay before drawing", ref secs, 60f, 220f))
			{
				t.DelaySeconds = secs;
				_dirty = true;
			}
			return;
		}
		TriggerMatch triggerMatch = t.On;
		if (triggerMatch - 5 <= TriggerMatch.Cast)
		{
			int data = (int)t.DataId;
			ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
			if (ImGui.InputInt((t.On == TriggerMatch.Headmarker) ? "Marker id (0 = any)" : "Tether id (0 = any)", ref data))
			{
				t.DataId = (uint)Math.Max(0, data);
				t.MatchById = t.DataId != 0;
				_dirty = true;
			}
		}
		else
		{
			DrawPicker(t);
		}
		DrawWho(t);
		float secs2 = t.DelaySeconds;
		if (TimeDrag("Delay before drawing", ref secs2, 60f, 220f))
		{
			t.DelaySeconds = secs2;
			_dirty = true;
		}
	}

	private static RoleFilter RoleOf(uint actorId)
	{
		if (actorId == 0)
		{
			return RoleFilter.Any;
		}
		try
		{
			if (Plugin.ObjectTable.SearchById(actorId) is IBattleChara { ClassJob: { IsValid: not false }, ClassJob: { Value: var value } })
			{
				switch (value.Role)
				{
				case 1:
					return RoleFilter.Tank;
				case 4:
					return RoleFilter.Healer;
				case 2:
				case 3:
					return RoleFilter.Dps;
				default:
					return RoleFilter.Any;
				}
			}
		}
		catch
		{
		}
		return RoleFilter.Any;
	}

	private void DrawWho(QuickDrawDef t)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		bool flag = t.On == TriggerMatch.Tether;
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Gold, "From");
		ImGui.SameLine();
		int currentItem = (int)t.Source;
		ImGui.SetNextItemWidth(120f * globalScale);
		if (ImGui.Combo("##src", ref currentItem, SourceNames, SourceNames.Length))
		{
			t.Source = (SourceFilter)currentItem;
			_dirty = true;
		}
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Dimmed, "who's a");
		ImGui.SameLine();
		int currentItem2 = (int)t.SourceRole;
		ImGui.SetNextItemWidth(95f * globalScale);
		if (ImGui.Combo("##srole", ref currentItem2, RoleNames, RoleNames.Length))
		{
			t.SourceRole = (RoleFilter)currentItem2;
			_dirty = true;
		}
		ImGui.SameLine();
		ImGui.TextDisabled("(the caster / applier)");
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Gold, "To  ");
		ImGui.SameLine();
		int num = (t.OnlyOnSelf ? 1 : (t.TargetRole switch
		{
			RoleFilter.Tank => 2, 
			RoleFilter.Healer => 3, 
			RoleFilter.Dps => 4, 
			_ => 0, 
		}));
		int currentItem3 = num;
		ImGui.SetNextItemWidth(120f * globalScale);
		if (ImGui.Combo("##to", ref currentItem3, ToNames, ToNames.Length))
		{
			t.OnlyOnSelf = currentItem3 == 1;
			t.TargetRole = currentItem3 switch
			{
				2 => RoleFilter.Tank, 
				3 => RoleFilter.Healer, 
				4 => RoleFilter.Dps, 
				_ => RoleFilter.Any, 
			};
			_dirty = true;
		}
		ImGui.SameLine();
		ImGui.TextDisabled(flag ? "(either end of the tether)" : "(who it lands on)");
	}

	private static bool TimeDrag(string label, ref float secs, float max, float width = 130f)
	{
		ImGui.SetNextItemWidth(width * ImGuiHelpers.GlobalScale);
		return ImGui.DragFloat(label, ref secs, 0.1f, 0f, max, "%.1fs", ImGuiSliderFlags.AlwaysClamp);
	}

	private void DrawZones(QuickDrawDef t)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		uint territoryType = Plugin.ClientState.TerritoryType;
		bool v = t.AnyZone;
		if (ImGui.Checkbox("Works in any zone", ref v))
		{
			t.AnyZone = v;
			_dirty = true;
		}
		if (t.AnyZone)
		{
			ImGui.TextDisabled("This draw fires everywhere.");
			return;
		}
		if (ImGui.Button("+ Add current zone") && territoryType != 0 && !t.Zones.Contains(territoryType))
		{
			t.Zones.Add(territoryType);
			_dirty = true;
		}
		ImGui.SameLine();
		ImU8String text = new ImU8String(11, 1);
		text.AppendLiteral("you're in: ");
		text.AppendFormatted(ZoneLibrary.NameOf(territoryType));
		ImGui.TextDisabled(text);
		ImGui.SetNextItemWidth(260f * globalScale);
		ImGui.InputTextWithHint("##zoneq", "type a duty / zone name…", ref _zoneSearch, 64);
		if (!string.IsNullOrWhiteSpace(_zoneSearch) && ImGui.BeginChild("##zoneres", new Vector2(0f, 120f * globalScale), border: true))
		{
			foreach (ZoneLibrary.Zone item in ZoneLibrary.Search(_zoneSearch))
			{
				ImU8String label = new ImU8String(0, 1);
				label.AppendFormatted(item.Name);
				if (ImGui.Selectable(label))
				{
					if (!t.Zones.Contains(item.TerritoryId))
					{
						t.Zones.Add(item.TerritoryId);
						_dirty = true;
					}
					_zoneSearch = "";
				}
			}
			ImGui.EndChild();
		}
		if (t.Zones.Count == 0)
		{
			ImGui.TextColored(new Vector4(1f, 0.6f, 0.3f, 1f), "No zones picked — add one, or enable \"any zone\".");
			return;
		}
		uint num = 0u;
		foreach (uint zone in t.Zones)
		{
			ImGui.BulletText(ZoneLibrary.NameOf(zone));
			ImGui.SameLine();
			ImU8String label2 = new ImU8String(9, 1);
			label2.AppendLiteral("remove##z");
			label2.AppendFormatted(zone);
			if (ImGui.SmallButton(label2))
			{
				num = zone;
			}
		}
		if (num != 0)
		{
			t.Zones.Remove(num);
			_dirty = true;
		}
	}

	private void DrawStepList(QuickDrawDef t, Configuration cfg)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		FollowUpStep followUpStep = null;
		for (int i = 0; i < t.FollowUps.Count; i++)
		{
			FollowUpStep followUpStep2 = t.FollowUps[i];
			ImU8String strId = new ImU8String(2, 1);
			strId.AppendLiteral("fu");
			strId.AppendFormatted(followUpStep2.Id);
			ImGui.PushID(strId);
			if (ImGui.SmallButton("✕"))
			{
				followUpStep = followUpStep2;
			}
			ImGui.SameLine();
			bool flag = _sel == i;
			if (ImGui.Selectable(StepSummary(followUpStep2, i + 1), flag))
			{
				_sel = i;
			}
			if (flag)
			{
				ImGui.Indent(12f * globalScale);
				DrawStepConfig(followUpStep2);
				ImGui.Unindent(12f * globalScale);
				ImGui.Spacing();
			}
			ImGui.PopID();
		}
		if (followUpStep != null)
		{
			int num = t.FollowUps.IndexOf(followUpStep);
			t.FollowUps.Remove(followUpStep);
			_dirty = true;
			if (_sel == num)
			{
				_sel = -1;
			}
			else if (_sel > num)
			{
				_sel--;
			}
		}
		ImGui.Spacing();
		Vector4 accent = Ui.Accent;
		accent.W = 0.85f;
		ImGui.PushStyleColor(ImGuiCol.Button, accent);
		if (ImGui.Button("✚  Add follow-up", new Vector2(-1f, 0f)))
		{
			t.FollowUps.Add(new FollowUpStep());
			_sel = t.FollowUps.Count - 1;
			_dirty = true;
		}
		ImGui.PopStyleColor();
	}

	private static string StepSummary(FollowUpStep s, int n)
	{
		string value = s.On switch
		{
			FollowUpOn.Timer => $"after {s.Seconds:0.#}s", 
			FollowUpOn.Cast => "on a cast", 
			FollowUpOn.StatusGain => "on status gained", 
			FollowUpOn.StatusLose => "on status lost", 
			FollowUpOn.Headmarker => "on headmarker", 
			FollowUpOn.Tether => "on tether", 
			FollowUpOn.Death => "on death", 
			FollowUpOn.Chat => "on chat line", 
			_ => "follow-up", 
		};
		return $"#{n}  {value}  →  {s.Draw.Shape}";
	}

	private void DrawStepConfig(FollowUpStep s)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		int currentItem = (int)s.On;
		ImGui.SetNextItemWidth(200f * globalScale);
		if (ImGui.Combo("React to", ref currentItem, FollowUpNames, FollowUpNames.Length))
		{
			s.On = (FollowUpOn)currentItem;
			s.Conditions.Clear();
			_dirty = true;
		}
		float secs = s.Seconds;
		if (TimeDrag((s.On == FollowUpOn.Timer) ? "Wait (seconds)" : "Within (seconds)", ref secs, 180f, 120f))
		{
			s.Seconds = secs;
			_dirty = true;
		}
		if (s.On != FollowUpOn.Timer)
		{
			s.EnsureConditions();
			DrawConditions(s);
		}
	}

	private void DrawConditions(FollowUpStep s)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		FollowUpOn followUpOn = s.On;
		bool flag = followUpOn - 4 <= FollowUpOn.Cast;
		bool flag2 = flag;
		if (s.Conditions.Count > 1)
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TextDisabled("Match");
			ImGui.SameLine();
			int currentItem = ((!s.RequireAll) ? 1 : 0);
			ImGui.SetNextItemWidth(150f * globalScale);
			if (ImGui.Combo("##mode", ref currentItem, MatchModeNames, MatchModeNames.Length))
			{
				s.RequireAll = currentItem == 0;
				_dirty = true;
			}
			ImGui.SameLine();
			ImGui.TextDisabled(s.RequireAll ? "need all (within the window)" : "any one fires it");
		}
		bool flag3 = s.On == FollowUpOn.Chat;
		int num = -1;
		for (int i = 0; i < s.Conditions.Count; i++)
		{
			FollowCond followCond = s.Conditions[i];
			ImU8String strId = new ImU8String(1, 1);
			strId.AppendLiteral("c");
			strId.AppendFormatted(i);
			ImGui.PushID(strId);
			if (flag3)
			{
				string buf = followCond.Pattern;
				ImGui.SetNextItemWidth(260f * globalScale);
				if (ImGui.InputTextWithHint("##pat", "text or regex in the line", ref buf, 256))
				{
					followCond.Pattern = buf;
					_dirty = true;
				}
				ImGui.SameLine();
				bool v = followCond.UseRegex;
				if (ImGui.Checkbox("regex", ref v))
				{
					followCond.UseRegex = v;
					_dirty = true;
				}
			}
			else if (flag2)
			{
				int data = (int)followCond.DataId;
				ImGui.SetNextItemWidth(130f * globalScale);
				if (ImGui.InputInt("id (0=any)", ref data))
				{
					followCond.DataId = (uint)Math.Max(0, data);
					_dirty = true;
				}
			}
			else
			{
				string buf2 = followCond.Pattern;
				ImGui.SetNextItemWidth(200f * globalScale);
				string text = ((s.On == FollowUpOn.Death) ? "who (blank=any)" : "name (blank=any)");
				if (ImGui.InputTextWithHint("##pat", text, ref buf2, 64))
				{
					followCond.Pattern = buf2;
					followCond.MatchById = false;
					_dirty = true;
				}
				if (followCond.MatchById && followCond.DataId != 0)
				{
					ImGui.SameLine();
					ImU8String text2 = new ImU8String(1, 1);
					text2.AppendLiteral("#");
					text2.AppendFormatted(followCond.DataId);
					ImGui.TextDisabled(text2);
				}
			}
			if (!flag3)
			{
				ImGui.SameLine();
				bool v2 = followCond.OnlyOnSelf;
				if (ImGui.Checkbox((s.On == FollowUpOn.Tether) ? "me" : ((s.On == FollowUpOn.Death) ? "I die" : "on me"), ref v2))
				{
					followCond.OnlyOnSelf = v2;
					_dirty = true;
				}
			}
			followUpOn = s.On;
			if (followUpOn - 1 <= FollowUpOn.StatusGain)
			{
				ImGui.SameLine();
				DrawCondLibrary(s, i, followCond);
			}
			if (s.Conditions.Count > 1)
			{
				ImGui.SameLine();
				if (ImGui.SmallButton("x"))
				{
					num = i;
				}
			}
			if (!flag3 && !flag2)
			{
				DrawCondWho(followCond);
			}
			ImGui.PopID();
		}
		if (num >= 0)
		{
			s.Conditions.RemoveAt(num);
			_dirty = true;
		}
		if (ImGui.SmallButton("+ condition"))
		{
			s.Conditions.Add(new FollowCond());
			_dirty = true;
		}
		if (!flag2)
		{
			ImGui.SameLine();
			ImGui.TextDisabled(flag3 ? "add another line to require together" : "add a 2nd debuff/cast to require together");
		}
	}

	private void DrawCondWho(FollowCond c)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		ImGui.Indent(14f * globalScale);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Dimmed, "from");
		ImGui.SameLine();
		int currentItem = (int)c.Source;
		ImGui.SetNextItemWidth(110f * globalScale);
		if (ImGui.Combo("##fsrc", ref currentItem, SourceNames, SourceNames.Length))
		{
			c.Source = (SourceFilter)currentItem;
			_dirty = true;
		}
		ImGui.SameLine();
		int currentItem2 = (int)c.SourceRole;
		ImGui.SetNextItemWidth(85f * globalScale);
		if (ImGui.Combo("##fsrole", ref currentItem2, RoleNames, RoleNames.Length))
		{
			c.SourceRole = (RoleFilter)currentItem2;
			_dirty = true;
		}
		ImGui.SameLine(0f, 14f * globalScale);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Dimmed, "to");
		ImGui.SameLine();
		int currentItem3 = (int)c.TargetRole;
		ImGui.SetNextItemWidth(85f * globalScale);
		if (ImGui.Combo("##ftrole", ref currentItem3, RoleNames, RoleNames.Length))
		{
			c.TargetRole = (RoleFilter)currentItem3;
			_dirty = true;
		}
		ImGui.Unindent(14f * globalScale);
	}

	private static void Banner(string title, string hint)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		ImGui.Spacing();
		ImGui.Spacing();
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		float textLineHeight = ImGui.GetTextLineHeight();
		windowDrawList.AddRectFilled(cursorScreenPos, new Vector2(cursorScreenPos.X + 3f * globalScale, cursorScreenPos.Y + textLineHeight), ImGui.ColorConvertFloat4ToU32(Ui.Accent), 1f);
		ImGui.Dummy(new Vector2(9f * globalScale, textLineHeight));
		ImGui.SameLine();
		ImGui.TextColored(new Vector4(0.95f, 0.93f, 0.93f, 1f), title);
		ImGui.SameLine();
		ImGui.TextColored(in Ui.Dimmed, "  " + hint);
		ImGui.Separator();
		ImGui.Spacing();
	}

	private void DrawCondLibrary(FollowUpStep s, int i, FollowCond c)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		string text = s.Id + ":" + i;
		if (ImGui.SmallButton("Find"))
		{
			ImU8String strId = new ImU8String(4, 1);
			strId.AppendLiteral("find");
			strId.AppendFormatted(text);
			ImGui.OpenPopup(strId);
		}
		ImU8String strId2 = new ImU8String(4, 1);
		strId2.AppendLiteral("find");
		strId2.AppendFormatted(text);
		if (!ImGui.BeginPopup(strId2))
		{
			return;
		}
		if (!_condSearch.TryGetValue(text, out string value))
		{
			value = "";
		}
		ImGui.SetNextItemWidth(240f * globalScale);
		if (ImGui.InputTextWithHint("##q", "status / ability name…", ref value, 64))
		{
			_condSearch[text] = value;
		}
		FollowUpOn followUpOn = s.On;
		bool flag = followUpOn - 2 <= FollowUpOn.Cast;
		bool flag2 = flag;
		bool flag3 = s.On == FollowUpOn.Cast;
		if (!string.IsNullOrWhiteSpace(value) && ImGui.BeginChild("##res", new Vector2(280f * globalScale, 180f * globalScale), border: true))
		{
			foreach (GameLibrary.Entry item in GameLibrary.Search(value))
			{
				if ((!flag2 || item.IsStatus) && (!flag3 || !item.IsStatus))
				{
					ImU8String strId3 = new ImU8String(0, 2);
					strId3.AppendFormatted(item.IsStatus ? "s" : "a");
					strId3.AppendFormatted(item.Id);
					ImGui.PushID(strId3);
					DrawIcon(item.Icon, ImGui.GetTextLineHeight());
					ImGui.SameLine();
					ImU8String label = new ImU8String(6, 3);
					label.AppendFormatted(item.Name);
					label.AppendLiteral("  (");
					label.AppendFormatted(item.IsStatus ? "status" : "action");
					label.AppendLiteral(" #");
					label.AppendFormatted(item.Id);
					label.AppendLiteral(")");
					if (ImGui.Selectable(label))
					{
						c.Pattern = item.Name;
						c.DataId = item.Id;
						c.MatchById = true;
						_dirty = true;
						ImGui.CloseCurrentPopup();
					}
					ImGui.PopID();
				}
			}
			ImGui.EndChild();
		}
		ImGui.EndPopup();
	}

	private void DrawPicker(QuickDrawDef t)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		TriggerMatch triggerMatch = t.On;
		bool flag = triggerMatch - 2 <= TriggerMatch.Cast;
		string value = (flag ? "status" : "ability");
		if (t.MatchById && t.DataId != 0)
		{
			float frameHeight = ImGui.GetFrameHeight();
			if (t.IconId != 0)
			{
				DrawIconPreview(t.IconId, frameHeight);
			}
			else
			{
				DrawIcon(0u, frameHeight);
			}
			ImGui.SameLine();
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(new Vector4(0.95f, 0.93f, 0.93f, 1f), string.IsNullOrWhiteSpace(t.Pattern) ? "(unnamed)" : t.Pattern);
			ImGui.SameLine();
			ImGui.AlignTextToFramePadding();
			ImU8String text = new ImU8String(2, 2);
			text.AppendFormatted(value);
			text.AppendLiteral(" #");
			text.AppendFormatted(t.DataId);
			ImGui.TextColored(in Ui.Dimmed, text);
		}
		else
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(in Ui.Dimmed, "No ability / status picked yet —");
		}
		ImGui.SameLine();
		if (ImGui.Button("Find…"))
		{
			ImGui.OpenPopup("##findmain");
		}
		ImGui.SameLine();
		bool v = t.MatchById;
		if (ImGui.Checkbox("match by exact id", ref v))
		{
			t.MatchById = v;
			_dirty = true;
		}
		DrawPickerPopup(t);
		if (!t.MatchById)
		{
			string buf = t.Pattern;
			ImGui.SetNextItemWidth(240f * globalScale);
			if (ImGui.InputTextWithHint("Name contains", "e.g. Heavenly Hell", ref buf, 128))
			{
				t.Pattern = buf;
				_dirty = true;
			}
			ImGui.SameLine();
			bool v2 = t.UseRegex;
			if (ImGui.Checkbox("regex", ref v2))
			{
				t.UseRegex = v2;
				_dirty = true;
			}
		}
	}

	private void DrawPickerPopup(QuickDrawDef t)
	{
		float globalScale = ImGuiHelpers.GlobalScale;
		if (!ImGui.BeginPopup("##findmain"))
		{
			return;
		}
		ImGui.SetNextItemWidth(280f * globalScale);
		ImGui.InputTextWithHint("##lib", "type an ability or status name…", ref _librarySearch, 64);
		if (!string.IsNullOrWhiteSpace(_librarySearch) && ImGui.BeginChild("##libresults", new Vector2(320f * globalScale, 220f * globalScale), border: true))
		{
			foreach (GameLibrary.Entry item in GameLibrary.Search(_librarySearch))
			{
				ImU8String strId = new ImU8String(0, 2);
				strId.AppendFormatted(item.IsStatus ? "s" : "a");
				strId.AppendFormatted(item.Id);
				ImGui.PushID(strId);
				DrawIcon(item.Icon, ImGui.GetTextLineHeight());
				ImGui.SameLine();
				ImU8String label = new ImU8String(6, 3);
				label.AppendFormatted(item.Name);
				label.AppendLiteral("  (");
				label.AppendFormatted(item.IsStatus ? "status" : "action");
				label.AppendLiteral(" #");
				label.AppendFormatted(item.Id);
				label.AppendLiteral(")");
				if (ImGui.Selectable(label))
				{
					t.On = ((!item.IsStatus) ? TriggerMatch.Cast : TriggerMatch.StatusGain);
					t.MatchById = true;
					t.DataId = item.Id;
					t.Pattern = item.Name;
					t.IconId = item.Icon;
					if (string.IsNullOrWhiteSpace(t.Name) || t.Name == "New quick draw")
					{
						t.Name = item.Name;
					}
					_dirty = true;
					ImGui.CloseCurrentPopup();
				}
				ImGui.PopID();
			}
			ImGui.EndChild();
		}
		ImGui.EndPopup();
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

	private static bool Combo(string label, string[] names, ref int index)
	{
		ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
		return ImGui.Combo(label, ref index, names, names.Length);
	}
}
