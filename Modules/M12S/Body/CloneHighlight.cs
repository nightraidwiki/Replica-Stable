using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using Replica.Logging;
using Replica.QuickDraws;
using Replica.Windows;

namespace Replica.Modules.M12S.Body;

public class CloneHighlight : ISpecialAction
{
	public enum Strat
	{
		StaticEU,
		DN,
		Relative
	}

	public enum Role
	{
		MT,
		OT,
		M1,
		M2,
		H1,
		H2,
		R1,
		R2
	}

	private enum RoleType
	{
		Tank,
		Melee,
		Healer,
		Ranged
	}

	public enum Dir
	{
		None,
		NE,
		SE,
		SW,
		NW
	}

	public class Config
	{
		public bool Active;

		public Strat Strat;

		public Role Role;

		public int ColorIndex = 4;

		public bool RelDarkLeft = true;

		public bool RelFireLeft = true;

		public bool ShowTether = true;

		public bool Preview;

		public Dir PreviewDir = Dir.NW;

		public bool PreviewFire;
	}

	private const uint CloneBaseId = 19204u;

	private const uint DarkResistanceDown = 3323u;

	private static bool _enableMigrated;

	private static readonly Dictionary<Dir, Vector2> CloneDirections = new Dictionary<Dir, Vector2>
	{
		[Dir.NE] = new Vector2(109.88013f, 90.073975f),
		[Dir.SE] = new Vector2(109.88013f, 109.88013f),
		[Dir.SW] = new Vector2(90.073975f, 109.88013f),
		[Dir.NW] = new Vector2(90.073975f, 90.073975f)
	};

	private ulong _darkMaster;

	private ulong _fireMaster;

	private readonly List<ulong> _darkClones = new List<ulong>();

	private readonly List<ulong> _fireClones = new List<ulong>();

	private readonly Dictionary<ulong, StaticVfx> _rings = new Dictionary<ulong, StaticVfx>();

	private uint _phase;

	private Dir _north;

	private StaticVfx _bait;

	private Vector3? _baitAt;

	private const string GuideOwner = "m12s_rep1_guide";

	private bool _guideLive;

	private bool? _lastGuideDark;

	private Vector3? _lastGuideSpot;

	private bool _lastTether;

	private static readonly string[] StratNames = new string[3] { "Static/EU", "DN/NA", "Clone Relative" };

	private static readonly string[] RoleNames = new string[8] { "MT", "OT", "M1", "M2", "H1", "H2", "R1", "R2" };

	private static readonly string[] PreviewDirNames = new string[4] { "NE", "SE", "SW", "NW" };

	private static Config C => ModuleConfig.Get<Config>();

	private static Vector4 DarkColor => new Vector4(0.6f, 0f, 1f, GroundOmen.Red.W);

	private static Vector4 FireColor => GroundOmen.Red;

	private static Vector4 BaitColor
	{
		get
		{
			Vector4 result = StratUI.SwatchColor(C.ColorIndex);
			result.W = GroundOmen.Red.W;
			return result;
		}
	}

	public override string Name => "Replication 1 (Clones + Bait)";

	public override string? ModuleEnableKey => "Lindblum/Replication 1 (Clones + Bait)";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46303u, 46301u, 46304u, 46368u, 46345u };

	public override bool HasConfig => true;

	private static void EnsureEnableMigrated()
	{
		if (!_enableMigrated)
		{
			_enableMigrated = true;
			ModuleConfig.MigrateLegacyActive("Lindblum/Replication 1 (Clones + Bait)", C.Active);
		}
	}

	public override void DrawConfig()
	{
		EnsureEnableMigrated();
		bool active = ModuleConfig.IsEnabled(ModuleEnableKey);
		if (StratUI.Header("Replication 1 — Clones", ref active))
		{
			ModuleConfig.SetEnabled(ModuleEnableKey, active);
			C.Active = active;
			ModuleConfig.Save<Config>();
		}
		StratUI.Section("Strategy");
		int selected = (int)C.Strat;
		if (StratUI.SegmentedBar(StratNames, ref selected))
		{
			C.Strat = (Strat)selected;
			ModuleConfig.Save<Config>();
		}
		StratUI.Section("Your role");
		StratUI.Hint("Left column = Group 1 (baits left) · right column = Group 2 (baits right)");
		int selected2 = (int)C.Role;
		if (StratUI.RoleGrid(RoleNames, ref selected2))
		{
			C.Role = (Role)selected2;
			ModuleConfig.Save<Config>();
		}
		if (C.Strat == Strat.StaticEU)
		{
			StratUI.Hint("Side is taken from your group automatically.");
		}
		else if (C.Strat == Strat.Relative)
		{
			ImGui.Spacing();
			bool v = C.RelDarkLeft;
			if (ImGui.Checkbox("Dark: bait on LEFT of your clone", ref v))
			{
				C.RelDarkLeft = v;
				ModuleConfig.Save<Config>();
			}
			if (!IsMelee(C.Role))
			{
				bool v2 = C.RelFireLeft;
				if (ImGui.Checkbox("Fire: bait on LEFT of your clone", ref v2))
				{
					C.RelFireLeft = v2;
					ModuleConfig.Save<Config>();
				}
			}
		}
		StratUI.Section("Marker color");
		int index = C.ColorIndex;
		if (StratUI.ColorSwatches(ref index))
		{
			C.ColorIndex = index;
			ModuleConfig.Save<Config>();
		}
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled(StratUI.SwatchName(C.ColorIndex));
		bool v3 = C.ShowTether;
		if (ImGui.Checkbox("Show guide line to the spot", ref v3))
		{
			C.ShowTether = v3;
			ModuleConfig.Save<Config>();
		}
		StratUI.Section("Preview");
		bool v4 = C.Preview;
		if (ImGui.Checkbox("Preview in arena (ignores phase, for testing)", ref v4))
		{
			C.Preview = v4;
			ModuleConfig.Save<Config>();
		}
		if (C.Preview)
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TextDisabled("Relative north:");
			ImGui.SameLine();
			int selected3 = (int)(C.PreviewDir - 1);
			if (selected3 < 0)
			{
				selected3 = 3;
			}
			if (StratUI.SegmentedBar(PreviewDirNames, ref selected3))
			{
				C.PreviewDir = (Dir)(selected3 + 1);
				ModuleConfig.Save<Config>();
			}
			bool v5 = C.PreviewFire;
			if (ImGui.Checkbox("You have Dark Resistance Down (bait fire)", ref v5))
			{
				C.PreviewFire = v5;
				ModuleConfig.Save<Config>();
			}
			Vector2 vector = ComputeBait(C.PreviewDir, C.PreviewFire);
			ImU8String text = new ImU8String(27, 3);
			text.AppendLiteral("Bait coord (");
			text.AppendFormatted(vector.X, "0.0");
			text.AppendLiteral(", ");
			text.AppendFormatted(vector.Y, "0.0");
			text.AppendLiteral(")  ·  radius ");
			text.AppendFormatted(BaitRadius(), "0.00");
			ImGui.TextDisabled(text);
		}
		if (ImGui.CollapsingHeader("Debug"))
		{
			ImU8String text2 = new ImU8String(24, 2);
			text2.AppendLiteral("Phase ");
			text2.AppendFormatted(_phase);
			text2.AppendLiteral("   Relative north ");
			text2.AppendFormatted(_north);
			ImGui.TextUnformatted(text2);
			ImU8String text3 = new ImU8String(27, 2);
			text3.AppendLiteral("Dark clones ");
			text3.AppendFormatted(_darkClones.Count);
			text3.AppendLiteral("   Fire clones ");
			text3.AppendFormatted(_fireClones.Count);
			ImGui.TextUnformatted(text3);
			ImU8String text4 = new ImU8String(22, 1);
			text4.AppendLiteral("Dark Resistance Down: ");
			text4.AppendFormatted(HasDarkResDown());
			ImGui.TextUnformatted(text4);
			ImU8String text5 = new ImU8String(34, 4);
			text5.AppendLiteral("Role ");
			text5.AppendFormatted(C.Role);
			text5.AppendLiteral(": melee=");
			text5.AppendFormatted(IsMelee(C.Role));
			text5.AppendLiteral(", group1/left=");
			text5.AppendFormatted(IsGroup1(C.Role));
			text5.AppendLiteral(", type=");
			text5.AppendFormatted(RoleOf(C.Role));
			ImGui.TextUnformatted(text5);
			ImGui.Separator();
			Dir[] array = new Dir[4]
			{
				Dir.NE,
				Dir.SE,
				Dir.SW,
				Dir.NW
			};
			foreach (Dir dir in array)
			{
				Vector2 vector2 = ComputeBait(dir, darkRes: false);
				Vector2 vector3 = ComputeBait(dir, darkRes: true);
				ImU8String text6 = new ImU8String(20, 5);
				text6.AppendFormatted(dir);
				text6.AppendLiteral(": dark (");
				text6.AppendFormatted(vector2.X, "0.0");
				text6.AppendLiteral(",");
				text6.AppendFormatted(vector2.Y, "0.0");
				text6.AppendLiteral(")  fire (");
				text6.AppendFormatted(vector3.X, "0.0");
				text6.AppendLiteral(",");
				text6.AppendFormatted(vector3.Y, "0.0");
				text6.AppendLiteral(")");
				ImGui.TextUnformatted(text6);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearAll();
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 46345:
		case 46368:
			ClearAll();
			break;
		case 46303:
			if (_darkMaster == 0L)
			{
				_darkMaster = info.SourceId;
			}
			break;
		case 46301:
			if (_fireMaster == 0L)
			{
				_fireMaster = info.SourceId;
			}
			break;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 46345)
		{
			ClearAll();
		}
		else if (info.ActionId == 46304 && _phase == 0 && _darkClones.Count == 2)
		{
			_phase = 1u;
		}
	}

	public override void Update()
	{
		EnsureEnableMigrated();
		Dictionary<ulong, Vector4> dictionary = new Dictionary<ulong, Vector4>();
		Resolve(_darkMaster, _darkClones, DarkColor, dictionary);
		Resolve(_fireMaster, _fireClones, FireColor, dictionary);
		foreach (ulong item in _rings.Keys.ToList())
		{
			if (!dictionary.ContainsKey(item))
			{
				_rings[item]?.Remove();
				aoes.Remove(_rings[item]);
				_rings.Remove(item);
			}
		}
		foreach (KeyValuePair<ulong, Vector4> item2 in dictionary)
		{
			if (_rings.TryGetValue(item2.Key, out StaticVfx value))
			{
				if (value != null)
				{
					value.Enable = true;
				}
				continue;
			}
			IGameObject gameObject = item2.Key.GameObject();
			if (gameObject != null)
			{
				StaticVfx staticVfx = Spawn(gameObject, item2.Value);
				if (staticVfx != null)
				{
					_rings[item2.Key] = staticVfx;
					aoes.Add(staticVfx);
				}
			}
		}
		UpdateBait();
	}

	private void UpdateBait()
	{
		if (IdyllicDream.IsRunning)
		{
			RemoveBait();
			ClearGuide();
			return;
		}
		if (_north == Dir.None && _darkClones.Count == 2)
		{
			_north = FindNorth();
		}
		Vector3? vector = ResolveBaitSpot();
		if (!vector.HasValue)
		{
			RemoveBait();
			ClearGuide();
			return;
		}
		bool darkDebuff = (C.Preview ? C.PreviewFire : HasDarkResDown());
		RefreshGuide(darkDebuff, vector.Value);
		if (_bait == null || !_baitAt.HasValue || Vector3.Distance(_baitAt.Value, vector.Value) > 0.05f)
		{
			RemoveBait();
			_bait = SpawnBait(vector.Value, BaitRadius());
			_baitAt = vector.Value;
			if (_bait != null)
			{
				aoes.Add(_bait);
			}
		}
	}

	private Vector3? ResolveBaitSpot()
	{
		if (C.Preview)
		{
			Vector2 vector = ComputeBait(C.PreviewDir, C.PreviewFire);
			if (!(vector != Vector2.Zero))
			{
				return null;
			}
			return new Vector3(vector.X, 0f, vector.Y);
		}
		if (!ModuleConfig.IsEnabled(ModuleEnableKey) || _north == Dir.None)
		{
			return null;
		}
		bool darkRes = HasDarkResDown();
		Vector2 vector2 = ComputeBait(_north, darkRes);
		if (!(vector2 != Vector2.Zero))
		{
			return null;
		}
		return new Vector3(vector2.X, 0f, vector2.Y);
	}

	private Dir FindNorth()
	{
		foreach (ulong darkClone in _darkClones)
		{
			IGameObject gameObject = darkClone.GameObject();
			if (gameObject == null)
			{
				continue;
			}
			Vector2 value = new Vector2(gameObject.Position.X, gameObject.Position.Z);
			foreach (KeyValuePair<Dir, Vector2> cloneDirection in CloneDirections)
			{
				if (Vector2.Distance(cloneDirection.Value, value) < 1f)
				{
					return cloneDirection.Key;
				}
			}
		}
		return Dir.None;
	}

	private static bool HasDarkResDown()
	{
		return Svc.Objects.LocalPlayer?.StatusList.Any((IStatus s) => s.StatusId == 3323) ?? false;
	}

	private static Vector2 ComputeBait(Dir n, bool darkRes)
	{
		if (C.Strat == Strat.DN)
		{
			return DnTable(n, RoleOf(C.Role), darkRes);
		}
		bool melee = IsMelee(C.Role);
		bool left = ((C.Strat != Strat.Relative) ? IsGroup1(C.Role) : (darkRes ? C.RelFireLeft : C.RelDarkLeft));
		if (!darkRes)
		{
			return StaticDark(n, melee, left);
		}
		return StaticFire(n, melee, left);
	}

	private static float BaitRadius()
	{
		if (!IsMelee(C.Role))
		{
			return 1.45f;
		}
		return 0.5f;
	}

	private static string BaitText(bool darkDebuff)
	{
		if (!darkDebuff)
		{
			return "BAIT DARK";
		}
		return "BAIT FIRE";
	}

	private static Vector4 GuideColor(bool darkDebuff)
	{
		if (!darkDebuff)
		{
			return DarkColor;
		}
		return FireColor;
	}

	private static bool IsMelee(Role r)
	{
		if ((uint)r <= 3u)
		{
			return true;
		}
		return false;
	}

	private static bool IsGroup1(Role r)
	{
		switch (r)
		{
		case Role.MT:
		case Role.M1:
		case Role.H1:
		case Role.R1:
			return true;
		default:
			return false;
		}
	}

	private static RoleType RoleOf(Role r)
	{
		switch (r)
		{
		case Role.MT:
		case Role.OT:
			return RoleType.Tank;
		case Role.M1:
		case Role.M2:
			return RoleType.Melee;
		case Role.H1:
		case Role.H2:
			return RoleType.Healer;
		default:
			return RoleType.Ranged;
		}
	}

	private static Vector2 StaticFire(Dir n, bool melee, bool left)
	{
		switch (n)
		{
		case Dir.NE:
			if (melee)
			{
				if (left)
				{
					return new Vector2(101.365f, 98.635f);
				}
				return new Vector2(101.365f, 98.635f);
			}
			if (left)
			{
				return new Vector2(86.35f, 100f);
			}
			return new Vector2(100f, 113.65f);
		case Dir.SE:
			if (melee)
			{
				if (left)
				{
					return new Vector2(101.365f, 101.365f);
				}
				return new Vector2(101.365f, 101.365f);
			}
			if (left)
			{
				return new Vector2(100f, 86.35f);
			}
			return new Vector2(86.35f, 100f);
		case Dir.SW:
			if (melee)
			{
				if (left)
				{
					return new Vector2(98.635f, 101.365f);
				}
				return new Vector2(98.635f, 101.365f);
			}
			if (left)
			{
				return new Vector2(113.65f, 100f);
			}
			return new Vector2(100f, 86.35f);
		case Dir.NW:
			if (melee)
			{
				if (left)
				{
					return new Vector2(98.635f, 98.635f);
				}
				return new Vector2(98.635f, 98.635f);
			}
			if (left)
			{
				return new Vector2(100f, 113.65f);
			}
			return new Vector2(113.65f, 100f);
		default:
			return Vector2.Zero;
		}
	}

	private static Vector2 StaticDark(Dir n, bool melee, bool left)
	{
		switch (n)
		{
		case Dir.NE:
			if (melee)
			{
				if (left)
				{
					return new Vector2(93.175f, 98.635f);
				}
				return new Vector2(101.365f, 106.825f);
			}
			if (left)
			{
				return new Vector2(100f, 86.35f);
			}
			return new Vector2(113.65f, 100f);
		case Dir.SE:
			if (melee)
			{
				if (left)
				{
					return new Vector2(101.365f, 93.175f);
				}
				return new Vector2(93.175f, 101.365f);
			}
			if (left)
			{
				return new Vector2(113.65f, 100f);
			}
			return new Vector2(100f, 113.65f);
		case Dir.SW:
			if (melee)
			{
				if (left)
				{
					return new Vector2(106.825f, 101.365f);
				}
				return new Vector2(98.635f, 93.175f);
			}
			if (left)
			{
				return new Vector2(100f, 113.65f);
			}
			return new Vector2(86.35f, 100f);
		case Dir.NW:
			if (melee)
			{
				if (left)
				{
					return new Vector2(98.635f, 106.825f);
				}
				return new Vector2(106.825f, 98.635f);
			}
			if (left)
			{
				return new Vector2(86.35f, 100f);
			}
			return new Vector2(100f, 86.35f);
		default:
			return Vector2.Zero;
		}
	}

	private static Vector2 DnTable(Dir n, RoleType role, bool darkRes)
	{
		switch (n)
		{
		case Dir.NE:
			switch (role)
			{
			case RoleType.Tank:
				if (darkRes)
				{
					return new Vector2(101.365f, 98.635f);
				}
				return new Vector2(101.365f, 106.825f);
			case RoleType.Melee:
				if (darkRes)
				{
					return new Vector2(101.365f, 98.635f);
				}
				return new Vector2(93.175f, 98.635f);
			case RoleType.Healer:
				if (darkRes)
				{
					return new Vector2(100f, 113.65f);
				}
				return new Vector2(100f, 86.35f);
			case RoleType.Ranged:
				if (darkRes)
				{
					return new Vector2(100f, 113.65f);
				}
				return new Vector2(113.65f, 100f);
			}
			break;
		case Dir.SE:
			switch (role)
			{
			case RoleType.Tank:
				if (darkRes)
				{
					return new Vector2(101.365f, 101.365f);
				}
				return new Vector2(101.365f, 93.175f);
			case RoleType.Melee:
				if (darkRes)
				{
					return new Vector2(101.365f, 101.365f);
				}
				return new Vector2(93.175f, 101.365f);
			case RoleType.Healer:
				if (darkRes)
				{
					return new Vector2(100f, 86.35f);
				}
				return new Vector2(113.65f, 100f);
			case RoleType.Ranged:
				if (darkRes)
				{
					return new Vector2(100f, 86.35f);
				}
				return new Vector2(100f, 113.65f);
			}
			break;
		case Dir.SW:
			switch (role)
			{
			case RoleType.Tank:
				if (darkRes)
				{
					return new Vector2(98.635f, 101.365f);
				}
				return new Vector2(98.635f, 93.175f);
			case RoleType.Melee:
				if (darkRes)
				{
					return new Vector2(98.635f, 101.365f);
				}
				return new Vector2(106.825f, 101.365f);
			case RoleType.Healer:
				if (darkRes)
				{
					return new Vector2(100f, 86.35f);
				}
				return new Vector2(100f, 113.65f);
			case RoleType.Ranged:
				if (darkRes)
				{
					return new Vector2(100f, 86.35f);
				}
				return new Vector2(86.35f, 100f);
			}
			break;
		case Dir.NW:
			switch (role)
			{
			case RoleType.Tank:
				if (darkRes)
				{
					return new Vector2(98.635f, 98.635f);
				}
				return new Vector2(106.825f, 98.635f);
			case RoleType.Melee:
				if (darkRes)
				{
					return new Vector2(98.635f, 98.635f);
				}
				return new Vector2(98.635f, 106.825f);
			case RoleType.Healer:
				if (darkRes)
				{
					return new Vector2(113.65f, 100f);
				}
				return new Vector2(100f, 86.35f);
			case RoleType.Ranged:
				if (darkRes)
				{
					return new Vector2(113.65f, 100f);
				}
				return new Vector2(86.35f, 100f);
			}
			break;
		}
		return Vector2.Zero;
	}

	private static void Resolve(ulong master, List<ulong> clones, Vector4 color, Dictionary<ulong, Vector4> desired)
	{
		if (master == 0L)
		{
			return;
		}
		IGameObject gameObject = master.GameObject();
		if (clones.Count < 2 && gameObject != null)
		{
			foreach (IGameObject item in Plugin.ObjectTable)
			{
				if (item.BaseId == 19204 && !clones.Contains(item.GameObjectId) && Math.Abs(Vector3.Distance(gameObject.Position, item.Position) - 5f) <= 0.5f)
				{
					clones.Add(item.GameObjectId);
				}
			}
			if (clones.Count < 2)
			{
				clones.Clear();
			}
		}
		if (clones.Count >= 2)
		{
			foreach (ulong clone in clones)
			{
				desired[clone] = color;
			}
			return;
		}
		if (gameObject != null)
		{
			desired[master] = color;
		}
	}

	private static StaticVfx Spawn(IGameObject clone, Vector4 color)
	{
		return DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customCircle",
			radiusX = 3f,
			radiusZ = 3f,
			drawOnObject = true,
			refColor = color,
			refTargetColor = color,
			destroyTime = 600000f
		}, clone);
	}

	private static StaticVfx SpawnBait(Vector3 pos, float radius)
	{
		return DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customCircle",
			radiusX = radius,
			radiusZ = radius,
			drawOnObject = false,
			Position = pos,
			refColor = BaitColor,
			refTargetColor = BaitColor,
			destroyTime = 600000f
		});
	}

	private void RefreshGuide(bool darkDebuff, Vector3 spot)
	{
		if (Plugin.Instance == null)
		{
			return;
		}
		bool flag = _lastGuideSpot.HasValue && Vector3.Distance(_lastGuideSpot.Value, spot) < 0.05f;
		if (!((_guideLive && _lastGuideDark == darkDebuff) & flag) || _lastTether != C.ShowTether)
		{
			_guideLive = true;
			_lastGuideDark = darkDebuff;
			_lastGuideSpot = spot;
			_lastTether = C.ShowTether;
			Vector4 vector = GuideColor(darkDebuff);
			LogEvent e = new LogEvent
			{
				Name = "rep1_guide"
			};
			Plugin.Instance.Engine.ClearExternal("m12s_rep1_guide");
			Plugin.Instance.Engine.SpawnExternal("m12s_rep1_guide", new DrawSpec
			{
				Shape = QuickShape.Text,
				Anchor = DrawAnchor.Self,
				AttachToActor = true,
				Color = vector,
				Duration = 600f,
				Label = BaitText(darkDebuff),
				LabelColor = vector,
				LabelSize = 1.2f,
				LabelHeight = 2f
			}, e, previewSelf: true);
			if (C.ShowTether)
			{
				Plugin.Instance.Engine.SpawnExternal("m12s_rep1_guide", new DrawSpec
				{
					Shape = QuickShape.ChevronPath,
					Anchor = DrawAnchor.Self,
					AttachToActor = true,
					Link = LinkTarget.FixedSpot,
					LinkPosition = spot,
					Color = vector,
					ChevronSpacing = 2f,
					LineThickness = 4f,
					Length = 30f,
					Duration = 600f
				}, e, previewSelf: true);
			}
		}
	}

	private void RemoveBait()
	{
		if (_bait != null)
		{
			_bait.Remove();
			aoes.Remove(_bait);
			_bait = null;
		}
		_baitAt = null;
	}

	private void ClearGuide()
	{
		if (_guideLive || _lastGuideSpot.HasValue)
		{
			_guideLive = false;
			_lastGuideDark = null;
			_lastGuideSpot = null;
			Plugin.Instance?.Engine.ClearExternal("m12s_rep1_guide");
		}
	}

	private void ClearAll()
	{
		foreach (StaticVfx value in _rings.Values)
		{
			value?.Remove();
		}
		_rings.Clear();
		RemoveBait();
		ClearGuide();
		aoes.Clear();
		_darkMaster = 0uL;
		_fireMaster = 0uL;
		_phase = 0u;
		_north = Dir.None;
		_darkClones.Clear();
		_fireClones.Clear();
	}
}
