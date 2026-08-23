using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using Replica.Logging;
using Replica.QuickDraws;
using Replica.Windows;

namespace Replica.Modules.M12S.Body;

public class IdyllicDream : ISpecialAction
{
	public enum Dir
	{
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW
	}

	public enum Towers : uint
	{
		WindLight = 2015013u,
		DoomLight = 2015014u,
		Fire = 2015016u,
		Earth = 2015015u
	}

	public enum TetherKind : uint
	{
		Stack = 369u,
		Defamation = 368u
	}

	public enum TowerPosition
	{
		MeleeLeft,
		MeleeRight,
		RangedLeft,
		RangedRight
	}

	public enum PickupOrder
	{
		Defamation_1,
		Defamation_2,
		Defamation_3,
		Defamation_4,
		Stack_1,
		Stack_2,
		Stack_3,
		Stack_4
	}

	public class Config
	{
		public int ConfigVersion = 2;

		public bool Active;

		public TowerPosition TowerPosition = TowerPosition.RangedRight;

		public bool IsGroup1 = true;

		public bool TakenCheckConditionIsTakenTower = true;

		public bool TakenFarIsEarth;

		public bool TakenFarIsMelee = true;

		public bool DontShowElementsP11S1;

		public List<PickupOrder> Pickups = new List<PickupOrder>
		{
			PickupOrder.Defamation_1,
			PickupOrder.Stack_1,
			PickupOrder.Stack_2,
			PickupOrder.Defamation_2,
			PickupOrder.Defamation_3,
			PickupOrder.Stack_3,
			PickupOrder.Stack_4,
			PickupOrder.Defamation_4
		};

		public bool AltCloneResolution = true;

		public List<Dir> AltCloneDirections = new List<Dir>
		{
			Dir.W,
			Dir.SW
		};

		public bool StackEnumPrioHorizontal;

		public bool StackEnumVerticalNorth = true;

		public bool StackEnumHorizontalWest = true;

		public HashSet<Dir> LP2CardinalStackFirst = new HashSet<Dir>
		{
			Dir.N,
			Dir.NE,
			Dir.E,
			Dir.SE
		};

		public HashSet<Dir> LP2CardinalDefamationFirst = new HashSet<Dir>
		{
			Dir.N,
			Dir.NE,
			Dir.E,
			Dir.SE
		};

		public bool ShowTetherLine = true;

		public bool ShowTetherCircle = true;

		public bool ShowGuidePath = true;

		public bool ShowGuideText = true;

		public bool SkipIndiMechs;

		public int ColorIndex = 4;

		public bool Preview;
	}

	private sealed class TowerData
	{
		public Dir Side;

		public Towers Kind;

		public Vector3 Position = Vector3.Zero;

		public uint AssignToPlayerEntityId;
	}

	private const int CurrentConfigVersion = 2;

	private static bool _enableMigrated;

	private static bool _configMigrated;

	private const uint PlayerCloneBaseId = 19210u;

	private const uint BossCloneNameId = 14380u;

	private const float Alpha = 0.85f;

	private static readonly Dictionary<Dir, Vector2> ReenactmentDirections = new Dictionary<Dir, Vector2>
	{
		[Dir.N] = new Vector2(100f, 86f),
		[Dir.NE] = new Vector2(110f, 90f),
		[Dir.E] = new Vector2(114f, 100f),
		[Dir.SE] = new Vector2(110f, 110f),
		[Dir.S] = new Vector2(100f, 114f),
		[Dir.SW] = new Vector2(90f, 110f),
		[Dir.W] = new Vector2(86f, 100f),
		[Dir.NW] = new Vector2(90f, 90f)
	};

	private static readonly int[] ReenactSeqCardinalA = new int[4] { 0, 2, 4, 6 };

	private static readonly int[] ReenactSeqIntercardA = new int[4] { 1, 3, 5, 7 };

	private static readonly int[] ReenactSeqCardinalB = new int[4] { 1, 3, 5, 7 };

	private static readonly int[] ReenactSeqIntercardB = new int[4] { 0, 2, 4, 6 };

	private int _phase;

	private int _phase7Sub;

	private int _phase11Sub;

	private int _defamationAttack;

	private int _playerPosition = -1;

	private bool? _isCardinalFirst;

	private bool? _isThDecreasingResistance;

	private bool? _isConeSafeNorth;

	private bool? _nextCleavesNorthSouth;

	private Vector3? _nextAOE;

	private readonly HashSet<(Vector3 Pos, float Rot)> _nextCleaves = new HashSet<(Vector3, float)>();

	private readonly Dictionary<uint, Vector3> _clonePositions = new Dictionary<uint, Vector3>();

	private readonly Dictionary<uint, bool> _defamationPlayers = new Dictionary<uint, bool>();

	private readonly Dictionary<uint, int> _playerOrder = new Dictionary<uint, int>();

	private readonly Dictionary<uint, (uint tetherId, uint playerId)> _cloneTethers = new Dictionary<uint, (uint, uint)>();

	private long _captureTowersAt;

	private bool _towersCaptured;

	private TowerData[] _towers = MakeTowerArray();

	private readonly Dictionary<string, StaticVfx> _el = new Dictionary<string, StaticVfx>();

	private bool _built;

	private const string GuideOwner = "m12s_idyllic_guide";

	private Vector3? _stackFinal;

	private bool _guideLive;

	private Vector3? _lastGuideSpot;

	private string _lastGuideLabel = "";

	private bool _lastShowText;

	private bool _lastShowPath;

	private static readonly Vector4 DefaColor = new Vector4(0.45f, 0.4f, 1f, 0.55f);

	private static readonly Vector4 StackColor = new Vector4(0.1f, 0.75f, 0.4f, 0.5f);

	private static readonly Vector4 ConeColor = new Vector4(1f, 0.8f, 0.1f, 0.3f);

	private static readonly Vector4 MeteorColor = new Vector4(1f, 0.55f, 0.1f, 0.32f);

	private static readonly Vector4 SafeColor = new Vector4(0.2f, 0.9f, 1f, 0.5f);

	private static readonly Vector4 TowerColor = new Vector4(0.2f, 0.95f, 0.35f, 0.6f);

	private static readonly string[] TowerNames = new string[4] { "Melee Left", "Melee Right", "Ranged Left", "Ranged Right" };

	private static Config C => ModuleConfig.Get<Config>();

	public static bool IsRunning { get; private set; }

	public override string Name => "Idyllic Dream (Uptime)";

	public override string? ModuleEnableKey => "Lindblum/Idyllic Dream (Uptime)";

	public override bool Registered => false;

	public override uint Phase => 2u;

	public override bool HasConfig => true;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		46345u, 48098u, 46358u, 46360u, 46361u, 48099u, 46356u, 46367u, 46327u, 46330u,
		46324u, 46352u
	};

	private static Vector4 Guide
	{
		get
		{
			Vector4 result = HsvToRgb((float)(Environment.TickCount64 % 2000) / 2000f, 1f, 1f);
			result.W = 0.85f;
			return result;
		}
	}

	private static void EnsureEnableMigrated()
	{
		if (!_configMigrated)
		{
			_configMigrated = true;
			if (C.ConfigVersion < 2)
			{
				ModuleConfig.Set(new Config());
			}
		}
		if (!_enableMigrated)
		{
			_enableMigrated = true;
			ModuleConfig.MigrateLegacyActive("Lindblum/Idyllic Dream (Uptime)", C.Active);
		}
	}

	private static TowerData[] MakeTowerArray()
	{
		return new TowerData[8]
		{
			new TowerData
			{
				Side = Dir.W,
				Kind = Towers.Fire
			},
			new TowerData
			{
				Side = Dir.W,
				Kind = Towers.Earth
			},
			new TowerData
			{
				Side = Dir.W,
				Kind = Towers.WindLight
			},
			new TowerData
			{
				Side = Dir.W,
				Kind = Towers.DoomLight
			},
			new TowerData
			{
				Side = Dir.E,
				Kind = Towers.Fire
			},
			new TowerData
			{
				Side = Dir.E,
				Kind = Towers.Earth
			},
			new TowerData
			{
				Side = Dir.E,
				Kind = Towers.WindLight
			},
			new TowerData
			{
				Side = Dir.E,
				Kind = Towers.DoomLight
			}
		};
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 46345 && _phase == 0)
		{
			_phase = 1;
		}
		if (info.ActionId == 48098)
		{
			_phase++;
		}
		if (info.ActionId == 46352 && (_phase == 3 || _phase == 4))
		{
			_isConeSafeNorth = info.Pos.Z < 100f;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 46345u:
			_phase = 1;
			break;
		case 48098u:
			_phase++;
			break;
		case 46358u:
			_nextAOE = null;
			_nextCleaves.Clear();
			if (_phase == 17)
			{
				_nextCleavesNorthSouth = null;
				Reset();
			}
			break;
		case 46360u:
		case 46361u:
			if (_phase == 9)
			{
				_defamationAttack++;
			}
			break;
		case 48099u:
		{
			int phase = _phase;
			if (((uint)(phase - 13) <= 1u || (uint)(phase - 16) <= 1u) ? true : false)
			{
				_defamationAttack++;
			}
			break;
		}
		case 46356u:
			if (_phase == 7 && _phase7Sub == 0)
			{
				_phase7Sub++;
			}
			break;
		case 46367u:
			if (_phase == 7)
			{
				_captureTowersAt = Environment.TickCount64 + 1000;
			}
			break;
		case 46327u:
		{
			int phase = _phase;
			bool flag = (uint)(phase - 10) <= 1u;
			if (flag && _phase11Sub == 1)
			{
				_phase11Sub++;
			}
			break;
		}
		case 46330u:
		{
			int phase = _phase;
			bool flag = (uint)(phase - 10) <= 1u;
			if (flag && _phase11Sub == 2)
			{
				_phase11Sub++;
			}
			break;
		}
		case 46324u:
		{
			int phase = _phase;
			if ((uint)(phase - 10) <= 1u)
			{
				AssignTower(info);
			}
			break;
		}
		}
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (_phase == 7 && info.StatusID == 4164 && !_isThDecreasingResistance.HasValue && ((uint)info.TargetID).GameObject() is IPlayerCharacter chara)
		{
			_isThDecreasingResistance = chara.GetRole() != CombatRole.DPS;
		}
		int phase = _phase;
		bool flag = (uint)(phase - 10) <= 1u;
		bool flag2 = flag && _phase11Sub == 0;
		if (flag2)
		{
			uint statusID = info.StatusID;
			bool flag3 = statusID - 4766 <= 1;
			flag2 = flag3;
		}
		if (flag2)
		{
			_phase11Sub++;
		}
	}

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (_phase == 1 && !_isCardinalFirst.HasValue && id == 4562 && source != null)
		{
			Vector2 value = V2(source.Position);
			if (Vector2.Distance(value, new Vector2(100f, 86f)) < 2f)
			{
				_isCardinalFirst = true;
			}
			else if (Vector2.Distance(value, new Vector2(110f, 90f)) < 2f)
			{
				_isCardinalFirst = false;
			}
		}
	}

	public override void OnActorTetherEvent(uint actorId, uint id, ulong targetId)
	{
		if ((id - 367 <= 2 || id - 373 <= 1) ? true : false)
		{
			_cloneTethers[actorId] = (id, (uint)targetId);
		}
	}

	public override void OnActorTetherCancelEvent(uint actorId)
	{
		_cloneTethers.Remove(actorId);
	}

	public override void Update()
	{
		EnsureEnableMigrated();
		if (C.Preview)
		{
			IsRunning = false;
			Build();
			HideAll();
			ClearGuide();
			DrawPreview();
			return;
		}
		if (!ModuleConfig.IsEnabled(ModuleEnableKey))
		{
			IsRunning = false;
			HideAll();
			ClearGuide();
			return;
		}
		if (_phase <= 0)
		{
			IsRunning = false;
			HideAll();
			ClearGuide();
			return;
		}
		IsRunning = true;
		Build();
		HideAll();
		_stackFinal = null;
		IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer == null)
		{
			ClearGuide();
			return;
		}
		if (_captureTowersAt != 0L && Environment.TickCount64 >= _captureTowersAt)
		{
			CaptureTowers();
		}
		switch (_phase)
		{
		case 1:
			Phase1();
			break;
		case 2:
			ScanCleaves(46354u, 1f, 2f, 46353u);
			break;
		case 4:
			ScanCleaves(46354u, 1f, 999f, 46353u);
			break;
		}
		int phase = _phase;
		if ((uint)(phase - 5) <= 1u)
		{
			Phase5(localPlayer);
		}
		if (_phase == 7 && _phase7Sub == 0)
		{
			Phase7Cone();
		}
		if (_phase == 7 && _phase7Sub == 1)
		{
			Phase7Tower(localPlayer);
		}
		if (_phase == 9 && Adj() < 4)
		{
			Phase9(localPlayer);
		}
		phase = _phase;
		if ((uint)(phase - 10) <= 1u)
		{
			Phase1011(localPlayer);
		}
		if (_phase == 12)
		{
			ScanCleaves12();
		}
		phase = _phase;
		if ((uint)(phase - 13) <= 1u)
		{
			ReenactA();
		}
		phase = _phase;
		if ((uint)(phase - 16) <= 1u)
		{
			ReenactB();
		}
		if ((_phase == 13 && Adj() < 5) || ((_phase == 16 || _phase == 17) && Adj() < 6))
		{
			StackTether();
		}
		phase = _phase;
		if ((uint)(phase - 12) <= 3u)
		{
			SafePlatform();
		}
		phase = _phase;
		if ((uint)(phase - 6) <= 2u)
		{
			ShowStoredCleaves((_phase == 6) ? 0.2f : 0.5f);
		}
		phase = _phase;
		if ((uint)(phase - 14) <= 2u)
		{
			ShowStoredCleaves((_phase == 14) ? 0.2f : 0.5f);
		}
		if (_phase == 17)
		{
			PortalCones();
		}
		UpdatePersonalGuide(localPlayer);
	}

	private void Phase1()
	{
		List<IGameObject> list = EnumCw(Svc.Objects.Where((IGameObject o) => o.BaseId == 19210 && _cloneTethers.ContainsKey(o.EntityId)), new Vector2(100f, 100f), new Vector2(96f, 86f));
		if (list.Count != 8)
		{
			return;
		}
		uint num = Svc.Objects.LocalPlayer?.EntityId ?? 0;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			IGameObject gameObject = list[num2];
			if (_cloneTethers.TryGetValue(gameObject.EntityId, out (uint, uint) value))
			{
				if (value.Item2 == num)
				{
					_playerPosition = num2;
				}
				_clonePositions[value.Item2] = gameObject.Position;
			}
		}
		_nextCleaves.Clear();
	}

	private void Phase5(IPlayerCharacter me)
	{
		List<IGameObject> bossClones = GetBossClones();
		if (bossClones.Count != 8)
		{
			return;
		}
		PickupOrder pickupOrder = C.Pickups[Math.Clamp(_playerPosition, 0, 7)];
		bool flag = pickupOrder <= PickupOrder.Defamation_4;
		int num = (int)(flag ? pickupOrder : (pickupOrder - 4));
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < bossClones.Count; i++)
		{
			IGameObject gameObject = bossClones[i];
			if (!_cloneTethers.TryGetValue(gameObject.EntityId, out (uint, uint) value))
			{
				continue;
			}
			Vector3 playerPos = value.Item2.GameObject()?.Position ?? me.Position;
			_playerOrder[value.Item2] = i;
			if (value.Item1 == 368)
			{
				_defamationPlayers[value.Item2] = true;
				if (flag && num2 == num)
				{
					PointPick(gameObject, playerPos);
				}
				num2++;
			}
			else if (value.Item1 == 369)
			{
				_defamationPlayers[value.Item2] = false;
				if (!flag && num3 == num)
				{
					PointPick(gameObject, playerPos);
				}
				num3++;
			}
		}
	}

	private void PointPick(IGameObject clone, Vector3 playerPos)
	{
		if (!C.SkipIndiMechs)
		{
			if (C.ShowTetherLine)
			{
				ShowLine("PickTether", clone.Position, playerPos, Guide);
			}
			if (C.ShowTetherCircle)
			{
				ShowAt("PickTetherCircle", clone.Position, Guide);
			}
		}
	}

	private void ShowStoredCleaves(float intensity)
	{
		Vector4 coneColor = ConeColor;
		coneColor.W = ConeColor.W * (intensity / 0.5f);
		Vector4 color = coneColor;
		coneColor = MeteorColor;
		coneColor.W = MeteorColor.W * (intensity / 0.5f);
		Vector4 color2 = coneColor;
		int num = 0;
		foreach (var nextCleaf in _nextCleaves)
		{
			Vector3 item = nextCleaf.Pos;
			float item2 = nextCleaf.Rot;
			num++;
			ShowCone($"Cone{num}", item, item2, color);
		}
		if (_nextAOE.HasValue)
		{
			ShowAt("Circle", _nextAOE.Value, color2);
		}
	}

	private void DrawPreview()
	{
		ShowAt("DefamationGroup1", DefaGroupPos(1), DefaColor);
		ShowAt("DefamationGroup2", DefaGroupPos(2), DefaColor);
		ShowAt("StackGroup1", StackGroupPos(1), StackColor);
		ShowAt("StackGroup2", StackGroupPos(2), StackColor);
		ShowAt("SafespotGroup1", SafespotPos(1), Guide);
		ShowAt("SafespotGroup2", SafespotPos(2), Guide);
		ShowAt("Given Far", BaitPos("Given Far"), TowerColor);
		ShowAt("Given Near", BaitPos("Given Near"), TowerColor);
		ShowAt("Taken Far", BaitPos("Taken Far"), TowerColor);
		ShowAt("Taken Near", BaitPos("Taken Near"), TowerColor);
	}

	private void Phase7Cone()
	{
		if (_isConeSafeNorth.HasValue)
		{
			float z = (_isConeSafeNorth.Value ? 90f : 110f);
			float x = (C.IsGroup1 ? 90f : 110f);
			ShowAt("p7sub1 tether", new Vector3(x, 0f, z), Guide);
		}
	}

	private void Phase7Tower(IPlayerCharacter me)
	{
		Vector3 vector = new Vector3(90.243f, 0f, 95.757f);
		Vector3 vector2 = new Vector3(81.757f, 0f, 95.757f);
		Vector3 vector3;
		switch (C.TowerPosition)
		{
		case TowerPosition.MeleeLeft:
			vector3 = vector;
			break;
		case TowerPosition.MeleeRight:
		{
			Vector3 vector4 = vector;
			vector4.Z = 200f - vector.Z;
			vector3 = vector4;
			break;
		}
		case TowerPosition.RangedLeft:
			vector3 = vector2;
			break;
		case TowerPosition.RangedRight:
		{
			Vector3 vector4 = vector2;
			vector4.Z = 200f - vector2.Z;
			vector3 = vector4;
			break;
		}
		default:
			vector3 = vector;
			break;
		}
		Vector3 vector5 = vector3;
		if (!C.IsGroup1)
		{
			vector3 = vector5;
			vector3.X = 200f - vector5.X;
			vector3.Z = 200f - vector5.Z;
			vector5 = vector3;
		}
		ShowAt("TowerTether", vector5, Guide);
		ShowAt("P7AOERadius", me.Position, MeteorColor);
	}

	private void Phase9(IPlayerCharacter me)
	{
		int num = Adj();
		uint num2 = FindByOrder(num);
		uint num3 = FindByOrder(4 + num);
		if (num2 == 0 || num3 == 0)
		{
			return;
		}
		IGameObject gameObject = num2.GameObject();
		IGameObject gameObject2 = num3.GameObject();
		if (gameObject == null || gameObject2 == null || !_defamationPlayers.TryGetValue(num2, out var value) || !_defamationPlayers.TryGetValue(num3, out var value2))
		{
			return;
		}
		Dir item = (_playerOrder.TryGetValue(me.EntityId, out var value3) ? ((Dir)value3) : Dir.N);
		uint num4 = FindByOrder(0);
		bool value4 = default(bool);
		int num5 = ((!(((num4 != 0 && _defamationPlayers.TryGetValue(num4, out value4)) & value4) ? C.LP2CardinalDefamationFirst : C.LP2CardinalStackFirst).Contains(item)) ? 1 : 2);
		bool flag = ((num2 == me.EntityId) & value) || ((num3 == me.EntityId) & value2);
		if (flag)
		{
			ShowAt("DefamationOnYou", me.Position, DefaColor);
		}
		if (value && !flag && !C.SkipIndiMechs)
		{
			ShowAt($"SafespotGroup{num5}", SafespotPos(num5), Guide);
		}
		if (value)
		{
			ShowAt("Defamation2", gameObject.Position, DefaColor);
			if (flag && num5 == 2 && !C.SkipIndiMechs)
			{
				ShowAt("DefamationGroup2", DefaGroupPos(2), Guide);
			}
		}
		if (value2)
		{
			ShowAt("Defamation1", gameObject2.Position, DefaColor);
			if (flag && num5 == 1 && !C.SkipIndiMechs)
			{
				ShowAt("DefamationGroup1", DefaGroupPos(1), Guide);
			}
		}
		if (!value)
		{
			ShowAt("Stack2", gameObject.Position, StackColor);
			if (num5 == 2 && !C.SkipIndiMechs)
			{
				ShowAt("StackGroup2", StackGroupPos(2), Guide);
			}
		}
		if (!value2)
		{
			ShowAt("Stack1", gameObject2.Position, StackColor);
			if (num5 == 1 && !C.SkipIndiMechs)
			{
				ShowAt("StackGroup1", StackGroupPos(1), Guide);
			}
		}
	}

	private void Phase1011(IPlayerCharacter me)
	{
		if (_phase11Sub == 0)
		{
			IGameObject shouldTakeTower = GetShouldTakeTower(me);
			if (shouldTakeTower == null)
			{
				return;
			}
			uint num = TowerKind(shouldTakeTower);
			TowerPosition towerPosition = C.TowerPosition;
			bool flag = (uint)towerPosition <= 1u;
			bool flag2 = flag;
			Vector3 position = shouldTakeTower.Position;
			switch (num)
			{
			case 2015014u:
				if (flag2)
				{
					position += new Vector3(0f, 0f, (shouldTakeTower.Position.Z > 100f) ? 1.5f : (-1.5f));
				}
				else
				{
					position += new Vector3((shouldTakeTower.Position.X > 100f) ? 1.5f : (-1.5f), 0f, 0f);
				}
				break;
			case 2015013u:
				position += new Vector3((shouldTakeTower.Position.X > 100f) ? (-1.5f) : 1.5f, 0f, 0f);
				break;
			}
			ShowAt("TowerTether", position, Guide);
		}
		else if (_phase11Sub == 1)
		{
			if (me.StatusList.Any((IStatus s) => s.StatusId == 4768))
			{
				ShowAt("TowerTether", me.Position, Guide);
			}
			List<TowerData> list = _towers.Where((TowerData t) => t.Kind == Towers.Earth).ToList();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				ShowAt($"Rock{num2 + 1}", list[num2].Position, TowerColor);
			}
		}
		else if (_phase11Sub == 2 && !C.DontShowElementsP11S1)
		{
			Phase11Cones(me);
		}
	}

	private void Phase11Cones(IPlayerCharacter me)
	{
		List<IPlayerCharacter> source = Svc.Objects.OfType<IPlayerCharacter>().ToList();
		List<IPlayerCharacter> list = source.Where((IPlayerCharacter x) => x.StatusList.Any((IStatus s) => s.StatusId == 4766)).ToList();
		List<IPlayerCharacter> list2 = source.Where((IPlayerCharacter x) => x.StatusList.Any((IStatus s) => s.StatusId == 4767)).ToList();
		if (list.Count + list2.Count == 4)
		{
			for (int num = 0; num < list.Count; num++)
			{
				IPlayerCharacter b = list[num];
				IPlayerCharacter playerCharacter = source.OrderByDescending((IPlayerCharacter x) => Vector3.DistanceSquared(x.Position, b.Position)).FirstOrDefault();
				if (playerCharacter != null)
				{
					ShowConeBetween($"FarCone{num + 1}", b.Position, playerCharacter.Position);
				}
			}
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				IPlayerCharacter b2 = list2[num2];
				IPlayerCharacter playerCharacter2 = source.OrderBy((IPlayerCharacter x) => Vector3.Distance(x.Position, b2.Position)).Skip(1).FirstOrDefault();
				if (playerCharacter2 != null)
				{
					ShowConeBetween($"NearCone{num2 + 1}", b2.Position, playerCharacter2.Position);
				}
			}
		}
		if (!C.TakenCheckConditionIsTakenTower)
		{
			TowerPosition towerPosition = C.TowerPosition;
			bool flag = (uint)towerPosition <= 1u;
			bool flag2 = flag;
			string text = (C.TakenFarIsMelee ? ((!flag2) ? "Taken Near" : "Taken Far") : ((!flag2) ? "Taken Far" : "Taken Near"));
			string name = text;
			if (me.StatusList.Any((IStatus s) => s.StatusId == 4766))
			{
				name = "Given Far";
			}
			else if (me.StatusList.Any((IStatus s) => s.StatusId == 4767))
			{
				name = "Given Near";
			}
			ShowBait(name, me);
			return;
		}
		uint num3 = ((!C.IsGroup1) ? (_towers.FirstOrDefault((TowerData x) => x.Kind == Towers.Earth && x.Side == Dir.E)?.AssignToPlayerEntityId ?? 0) : (_towers.FirstOrDefault((TowerData x) => x.Kind == Towers.Earth && x.Side == Dir.W)?.AssignToPlayerEntityId ?? 0));
		uint num4 = ((!C.IsGroup1) ? (_towers.FirstOrDefault((TowerData x) => x.Kind == Towers.Fire && x.Side == Dir.E)?.AssignToPlayerEntityId ?? 0) : (_towers.FirstOrDefault((TowerData x) => x.Kind == Towers.Fire && x.Side == Dir.W)?.AssignToPlayerEntityId ?? 0));
		if (num3 != 0 && num4 != 0)
		{
			bool flag3 = me.EntityId == num3;
			string name2 = (((flag3 && C.TakenFarIsEarth) || (!flag3 && !C.TakenFarIsEarth)) ? "Taken Far" : "Taken Near");
			if (flag3 && me.StatusList.Any((IStatus s) => s.StatusId == 4767))
			{
				name2 = "Given Near";
			}
			else if (!flag3 && me.StatusList.Any((IStatus s) => s.StatusId == 4766))
			{
				name2 = "Given Far";
			}
			if (me.StatusList.Any((IStatus s) => s.StatusId == 4766))
			{
				name2 = "Given Far";
			}
			else if (me.StatusList.Any((IStatus s) => s.StatusId == 4767))
			{
				name2 = "Given Near";
			}
			ShowBait(name2, me);
		}
	}

	private void ShowBait(string name, IPlayerCharacter me)
	{
		Vector3 vector = BaitPos(name);
		if ((me.Position.X > 100f && vector.X < 100f) || (me.Position.X < 100f && vector.X > 100f))
		{
			Vector3 vector2 = vector;
			vector2.X = 200f - vector.X;
			vector = vector2;
		}
		ShowLine(name, me.Position, vector, Guide);
	}

	private IGameObject? GetShouldTakeTower(IPlayerCharacter me)
	{
		IEnumerable<IGameObject> source = Svc.Objects.Where(delegate(IGameObject x)
		{
			uint num = TowerKind(x);
			return num - 2015015 <= 1;
		});
		IEnumerable<IGameObject> source2 = (C.IsGroup1 ? source.Where((IGameObject x) => x.Position.X < 100f) : source.Where((IGameObject x) => x.Position.X > 100f));
		IEnumerable<IGameObject> source3 = Svc.Objects.Where(delegate(IGameObject x)
		{
			uint num = TowerKind(x);
			return num - 2015013 <= 1;
		});
		IEnumerable<IGameObject> source4 = (C.IsGroup1 ? source3.Where((IGameObject x) => x.Position.X < 100f) : source3.Where((IGameObject x) => x.Position.X > 100f));
		if (source2.Count() + source4.Count() != 4)
		{
			return null;
		}
		if (!_isThDecreasingResistance.HasValue)
		{
			return null;
		}
		bool flag = me.GetRole() == CombatRole.DPS == _isThDecreasingResistance.Value;
		TowerPosition towerPosition = C.TowerPosition;
		bool flag2 = (uint)towerPosition <= 1u;
		Vector2 center = new Vector2(100f, 100f);
		if (flag2)
		{
			if (!flag)
			{
				return source2.OrderBy((IGameObject x) => Vector2.Distance(V2(x.Position), center)).FirstOrDefault();
			}
			return source4.OrderBy((IGameObject x) => Vector2.Distance(V2(x.Position), center)).FirstOrDefault();
		}
		if (!flag)
		{
			return source2.OrderByDescending((IGameObject x) => Vector2.Distance(V2(x.Position), center)).FirstOrDefault();
		}
		return source4.OrderByDescending((IGameObject x) => Vector2.Distance(V2(x.Position), center)).FirstOrDefault();
	}

	private void AssignTower(ActorAbilityInfo info)
	{
		Vector3 src = info.Source?.Position ?? info.Pos;
		TowerData towerData = _towers.FirstOrDefault((TowerData x) => Vector2.Distance(V2(x.Position), V2(src)) < 2f);
		if (towerData != null)
		{
			IPlayerCharacter playerCharacter = Svc.Objects.OfType<IPlayerCharacter>().FirstOrDefault((IPlayerCharacter x) => Vector2.Distance(V2(x.Position), V2(src)) < 2f);
			if (playerCharacter != null)
			{
				towerData.AssignToPlayerEntityId = playerCharacter.EntityId;
			}
		}
	}

	private void CaptureTowers()
	{
		_captureTowersAt = 0L;
		foreach (IGameObject @object in Svc.Objects)
		{
			uint id = TowerKind(@object);
			uint num = id;
			if (num - 2015013 <= 3)
			{
				Dir ew = ((@object.Position.X > 100f) ? Dir.E : Dir.W);
				TowerData towerData = _towers.FirstOrDefault((TowerData x) => x.Side == ew && x.Kind == (Towers)id);
				if (towerData != null)
				{
					towerData.Position = @object.Position;
				}
			}
		}
		_towersCaptured = true;
	}

	private void ScanCleaves12()
	{
		_nextCleaves.Clear();
		foreach (IGameObject @object in Svc.Objects)
		{
			if (@object is IBattleChara { IsCasting: not false, CurrentCastTime: var currentCastTime } battleChara)
			{
				if (battleChara.CastActionId == 46352 && currentCastTime >= 1f && currentCastTime <= 2f)
				{
					_nextCleaves.Add((battleChara.Position, 0f));
					_nextCleaves.Add((battleChara.Position, (float)Math.PI));
					_nextCleavesNorthSouth = false;
				}
				if (battleChara.CastActionId == 46351 && currentCastTime >= 1f && currentCastTime <= 2f)
				{
					_nextCleaves.Add((battleChara.Position, (float)Math.PI / 2f));
					_nextCleaves.Add((battleChara.Position, 4.712389f));
					_nextCleavesNorthSouth = true;
				}
				if (battleChara.CastActionId == 48303 && currentCastTime >= 1f && currentCastTime <= 2f)
				{
					_nextAOE = battleChara.Position;
				}
			}
		}
	}

	private void ReenactA()
	{
		if (Adj() < 5)
		{
			if (_isCardinalFirst == true)
			{
				Stored(0);
				Stored(2);
				Stored(4);
				Stored(6);
			}
			else if (_isCardinalFirst == false)
			{
				Stored(1);
				Stored(3);
				Stored(5);
				Stored(7);
			}
		}
	}

	private void ReenactB()
	{
		if (Adj() < 6)
		{
			if (_isCardinalFirst == true)
			{
				Stored(1);
				Stored(3);
				Stored(5);
				Stored(7);
			}
			else if (_isCardinalFirst == false)
			{
				Stored(0);
				Stored(2);
				Stored(4);
				Stored(6);
			}
		}
	}

	private void Stored(int index)
	{
		List<KeyValuePair<uint, Vector3>> list = EnumCwKv(_clonePositions, new Vector2(100f, 100f), new Vector2(98f, 86f));
		if (index >= list.Count)
		{
			return;
		}
		uint key = list[index].Key;
		if (!_defamationPlayers.TryGetValue(key, out var value))
		{
			return;
		}
		string[] array = ((!value) ? new string[2] { "Stack1", "Stack2" } : new string[2] { "Defamation1", "Defamation2" });
		foreach (string key2 in array)
		{
			if (_el.TryGetValue(key2, out StaticVfx value2) && !value2.Enable)
			{
				ShowAt(key2, list[index].Value, value ? DefaColor : StackColor);
				break;
			}
		}
	}

	private void StackTether()
	{
		Vector3 item = ElPos("Stack1");
		Vector3 item2 = ElPos("Stack2");
		List<Vector3> list = new List<Vector3> { item, item2 };
		Vector3? stackFinal = null;
		if (C.AltCloneResolution)
		{
			Vector3 vector = list.FirstOrDefault((Vector3 x) => C.AltCloneDirections.Any((Dir a) => Vector2.Distance(V2(x), ReenactmentDirections[a]) < 2f));
			if (vector != default(Vector3))
			{
				stackFinal = vector;
			}
		}
		else if (C.StackEnumPrioHorizontal)
		{
			if (Approx(list[0].X, list[1].X, 1f))
			{
				list = list.OrderBy((Vector3 x) => x.Z).ToList();
				stackFinal = list[(!C.StackEnumVerticalNorth) ? 1 : 0];
			}
			else
			{
				list = list.OrderBy((Vector3 x) => x.X).ToList();
				stackFinal = list[(!C.StackEnumHorizontalWest) ? 1 : 0];
			}
		}
		else if (Approx(list[0].Z, list[1].Z, 1f))
		{
			list = list.OrderBy((Vector3 x) => x.X).ToList();
			stackFinal = list[(!C.StackEnumHorizontalWest) ? 1 : 0];
		}
		else
		{
			list = list.OrderBy((Vector3 x) => x.Z).ToList();
			stackFinal = list[(!C.StackEnumVerticalNorth) ? 1 : 0];
		}
		_stackFinal = stackFinal;
		if (stackFinal.HasValue && !C.SkipIndiMechs)
		{
			ShowAt("stack tether", stackFinal.Value, Guide);
		}
	}

	private int MyParty(IPlayerCharacter me)
	{
		Dir item = (_playerOrder.TryGetValue(me.EntityId, out var value) ? ((Dir)value) : Dir.N);
		uint num = FindByOrder(0);
		bool value2 = default(bool);
		if (!(((num != 0 && _defamationPlayers.TryGetValue(num, out value2)) & value2) ? C.LP2CardinalDefamationFirst : C.LP2CardinalStackFirst).Contains(item))
		{
			return 1;
		}
		return 2;
	}

	private int ReenactActiveOrderIndex()
	{
		if (!_isCardinalFirst.HasValue)
		{
			return -1;
		}
		int num = Adj();
		int phase = _phase;
		int[] array;
		if ((uint)(phase - 13) <= 1u)
		{
			array = ((_isCardinalFirst == true) ? ReenactSeqCardinalA : ReenactSeqIntercardA);
		}
		else
		{
			int phase2 = _phase;
			bool flag = (uint)(phase2 - 16) <= 1u;
			array = ((!flag) ? Array.Empty<int>() : ((_isCardinalFirst == true) ? ReenactSeqCardinalB : ReenactSeqIntercardB));
		}
		int[] array2 = array;
		phase = _phase;
		bool flag2 = (uint)(phase - 13) <= 1u;
		if (flag2 && num >= array2.Length)
		{
			return -1;
		}
		phase = _phase;
		flag2 = (uint)(phase - 16) <= 1u;
		if (flag2 && num >= array2.Length)
		{
			return -1;
		}
		if (num >= array2.Length)
		{
			return -1;
		}
		return array2[num];
	}

	private Vector3 DefaSpotFor(IPlayerCharacter me)
	{
		if (_el.TryGetValue("Defamation1", out StaticVfx value) && value.Enable && value.Position != Vector3.Zero)
		{
			return value.Position;
		}
		if (_el.TryGetValue("Defamation2", out StaticVfx value2) && value2.Enable && value2.Position != Vector3.Zero)
		{
			return value2.Position;
		}
		if (_clonePositions.TryGetValue(me.EntityId, out var value3))
		{
			return DefaGroupPos((value3.X < 100f) ? 1 : 2);
		}
		return DefaGroupPos(MyParty(me));
	}

	private Vector3? TowerGuideSpot(IPlayerCharacter me)
	{
		if (_phase == 7 && _phase7Sub == 0 && _isConeSafeNorth.HasValue)
		{
			float z = (_isConeSafeNorth.Value ? 90f : 110f);
			return new Vector3(C.IsGroup1 ? 90f : 110f, 0f, z);
		}
		if (_phase == 7 && _phase7Sub == 1)
		{
			Vector3 vector = new Vector3(90.243f, 0f, 95.757f);
			Vector3 vector2 = new Vector3(81.757f, 0f, 95.757f);
			Vector3 vector3;
			switch (C.TowerPosition)
			{
			case TowerPosition.MeleeLeft:
				vector3 = vector;
				break;
			case TowerPosition.MeleeRight:
			{
				Vector3 vector4 = vector;
				vector4.Z = 200f - vector.Z;
				vector3 = vector4;
				break;
			}
			case TowerPosition.RangedLeft:
				vector3 = vector2;
				break;
			case TowerPosition.RangedRight:
			{
				Vector3 vector4 = vector2;
				vector4.Z = 200f - vector2.Z;
				vector3 = vector4;
				break;
			}
			default:
				vector3 = vector;
				break;
			}
			Vector3 vector5 = vector3;
			if (!C.IsGroup1)
			{
				vector3 = vector5;
				vector3.X = 200f - vector5.X;
				vector3.Z = 200f - vector5.Z;
				vector5 = vector3;
			}
			return vector5;
		}
		int phase = _phase;
		bool flag = (uint)(phase - 10) <= 1u;
		if (flag && _phase11Sub == 0)
		{
			IGameObject shouldTakeTower = GetShouldTakeTower(me);
			if (shouldTakeTower == null)
			{
				return null;
			}
			uint num = TowerKind(shouldTakeTower);
			TowerPosition towerPosition = C.TowerPosition;
			flag = (uint)towerPosition <= 1u;
			bool flag2 = flag;
			Vector3 position = shouldTakeTower.Position;
			switch (num)
			{
			case 2015014u:
				if (flag2)
				{
					position += new Vector3(0f, 0f, (shouldTakeTower.Position.Z > 100f) ? 1.5f : (-1.5f));
				}
				else
				{
					position += new Vector3((shouldTakeTower.Position.X > 100f) ? 1.5f : (-1.5f), 0f, 0f);
				}
				break;
			case 2015013u:
				position += new Vector3((shouldTakeTower.Position.X > 100f) ? (-1.5f) : 1.5f, 0f, 0f);
				break;
			}
			return position;
		}
		phase = _phase;
		flag = (uint)(phase - 10) <= 1u;
		if (flag && _phase11Sub == 1 && me.StatusList.Any((IStatus s) => s.StatusId == 4768))
		{
			return me.Position;
		}
		return null;
	}

	private Vector3? OutsideShareSpot(IPlayerCharacter me)
	{
		if (!_clonePositions.TryGetValue(me.EntityId, out var value))
		{
			return null;
		}
		if (MathF.Abs(value.X - 100f) > 8f)
		{
			return value;
		}
		if (_playerOrder.TryGetValue(me.EntityId, out var value2) && (value2 == 2 || value2 == 6))
		{
			return value;
		}
		return null;
	}

	private void UpdatePersonalGuide(IPlayerCharacter me)
	{
		if (C.SkipIndiMechs)
		{
			ClearGuide();
			return;
		}
		Vector3? spot = TowerGuideSpot(me);
		if (spot.HasValue)
		{
			string label = ((_phase == 7 && _phase7Sub == 0) ? "SAFE" : "TOWER");
			Vector4 color = ((_phase == 7 && _phase7Sub == 0) ? SafeColor : Guide);
			RefreshGuide(spot, label, color);
			return;
		}
		int phase = _phase;
		if ((uint)(phase - 15) <= 2u)
		{
			phase = _phase;
			bool flag = (uint)(phase - 16) <= 1u;
			if (flag || (_phase == 15 && Adj() >= 5))
			{
				Vector3? spot2 = OutsideShareSpot(me);
				if (spot2.HasValue)
				{
					string label2 = ((spot2.Value.X < 100f) ? "WEST" : "EAST");
					RefreshGuide(spot2, label2, Guide);
					return;
				}
			}
		}
		Vector3? spot3 = null;
		string text = "";
		Vector4 color2 = StackColor;
		if (_phase == 9 && Adj() < 4 && _defamationPlayers.TryGetValue(me.EntityId, out var value))
		{
			int num = MyParty(me);
			int num2 = Adj();
			uint num3 = FindByOrder(num2);
			uint num4 = FindByOrder(4 + num2);
			bool value2 = default(bool);
			bool value3 = default(bool);
			if (((num3 == me.EntityId && _defamationPlayers.TryGetValue(num3, out value2)) & value2) || ((num4 == me.EntityId && _defamationPlayers.TryGetValue(num4, out value3)) & value3))
			{
				spot3 = DefaGroupPos(num);
				text = "DEFAMATION";
				color2 = DefaColor;
			}
			else if (value)
			{
				spot3 = SafespotPos(num);
				text = "SAFE";
				color2 = SafeColor;
			}
			else
			{
				spot3 = StackGroupPos(num);
				text = "STACK";
				color2 = StackColor;
			}
		}
		else
		{
			bool flag = _phase == 13 && Adj() < 5;
			if (!flag)
			{
				phase = _phase;
				bool flag2 = (uint)(phase - 16) <= 1u;
				flag = flag2 && Adj() < 6;
			}
			if (flag)
			{
				int num5 = ReenactActiveOrderIndex();
				if (num5 >= 0)
				{
					List<KeyValuePair<uint, Vector3>> list = EnumCwKv(_clonePositions, new Vector2(100f, 100f), new Vector2(98f, 86f));
					if (num5 < list.Count && list[num5].Key == me.EntityId && _defamationPlayers.TryGetValue(me.EntityId, out var value4))
					{
						if (value4)
						{
							spot3 = DefaSpotFor(me);
							text = "DEFAMATION";
							color2 = DefaColor;
						}
						else
						{
							spot3 = _stackFinal;
							text = "STACK";
							color2 = StackColor;
						}
					}
				}
			}
		}
		if (text.Length == 0)
		{
			ClearGuide();
		}
		else
		{
			RefreshGuide(spot3, text, color2);
		}
	}

	private void RefreshGuide(Vector3? spot, string label, Vector4 color)
	{
		if (Plugin.Instance == null)
		{
			return;
		}
		bool showGuideText = C.ShowGuideText;
		bool flag = C.ShowGuidePath && spot.HasValue;
		bool flag2 = _lastGuideSpot.HasValue == spot.HasValue && (!spot.HasValue || Vector3.Distance(_lastGuideSpot.Value, spot.Value) < 0.05f);
		if (!(_guideLive & flag2) || !(_lastGuideLabel == label) || _lastShowText != showGuideText || _lastShowPath != flag)
		{
			_guideLive = true;
			_lastGuideSpot = spot;
			_lastGuideLabel = label;
			_lastShowText = showGuideText;
			_lastShowPath = flag;
			LogEvent e = new LogEvent
			{
				Name = "idyllic_guide"
			};
			Plugin.Instance.Engine.ClearExternal("m12s_idyllic_guide");
			if (showGuideText)
			{
				Plugin.Instance.Engine.SpawnExternal("m12s_idyllic_guide", new DrawSpec
				{
					Shape = QuickShape.Text,
					Anchor = DrawAnchor.Self,
					AttachToActor = true,
					Color = color,
					Duration = 600f,
					Label = label,
					LabelColor = color,
					LabelSize = 1.2f,
					LabelHeight = 2f
				}, e, previewSelf: true);
			}
			if (flag)
			{
				Plugin.Instance.Engine.SpawnExternal("m12s_idyllic_guide", new DrawSpec
				{
					Shape = QuickShape.ChevronPath,
					Anchor = DrawAnchor.Self,
					AttachToActor = true,
					Link = LinkTarget.FixedSpot,
					LinkPosition = spot.Value,
					Color = color,
					ChevronSpacing = 2f,
					LineThickness = 4f,
					Length = 30f,
					Duration = 600f
				}, e, previewSelf: true);
			}
		}
	}

	private void ClearGuide()
	{
		if (_guideLive)
		{
			_guideLive = false;
			_lastGuideSpot = null;
			_lastGuideLabel = "";
			_lastShowText = false;
			_lastShowPath = false;
			Plugin.Instance?.Engine.ClearExternal("m12s_idyllic_guide");
		}
	}

	private void SafePlatform()
	{
		if (_nextCleavesNorthSouth.HasValue)
		{
			bool num = _nextCleaves.Any(((Vector3 Pos, float Rot) x) => x.Pos.X < 100f);
			bool flag = _phase > 13 || (_phase == 13 && Adj() >= 5);
			string text = (num ? "West" : "East");
			string text2 = (_nextCleavesNorthSouth.Value ? "LeftRight" : "FrontBack");
			string key = "Safe" + text + text2 + (flag ? "A" : "");
			ShowAt(key, SafePlatformPos(key), flag ? Guide : SafeColor);
		}
	}

	private void PortalCones()
	{
		if (_nextCleavesNorthSouth == true)
		{
			ShowCone("PortalConeNS1", new Vector3(100f, 0f, 92.5f), 0f, ConeColor);
			ShowCone("PortalConeNS2", new Vector3(100f, 0f, 92.5f), (float)Math.PI, ConeColor);
		}
		else if (_nextCleavesNorthSouth == false)
		{
			ShowCone("PortalConeEW1", new Vector3(100f, 0f, 92.5f), 4.712389f, ConeColor);
			ShowCone("PortalConeEW2", new Vector3(100f, 0f, 92.5f), (float)Math.PI / 2f, ConeColor);
		}
	}

	private void ScanCleaves(uint coneAction, float min, float max, uint aoeAction)
	{
		foreach (IGameObject @object in Svc.Objects)
		{
			if (@object is IBattleChara { IsCasting: not false, CurrentCastTime: var currentCastTime } battleChara)
			{
				if (battleChara.CastActionId == coneAction && currentCastTime >= min && currentCastTime <= max)
				{
					_nextCleaves.Add((battleChara.Position, battleChara.Rotation));
				}
				if (battleChara.CastActionId == aoeAction && (max > 900f || (currentCastTime >= min && currentCastTime <= max)))
				{
					_nextAOE = battleChara.Position;
				}
			}
		}
	}

	private int Adj()
	{
		return _defamationAttack / 2;
	}

	private uint FindByOrder(int order)
	{
		foreach (KeyValuePair<uint, int> item in _playerOrder)
		{
			if (item.Value == order)
			{
				return item.Key;
			}
		}
		return 0u;
	}

	private static uint TowerKind(IGameObject o)
	{
		uint num = (o as IBattleNpc)?.NameId ?? 0;
		if (num == 0)
		{
			num = o.BaseId;
		}
		return num;
	}

	private List<IGameObject> GetBossClones()
	{
		return EnumCw(Svc.Objects.Where((IGameObject o) => TowerKind(o) == 14380 && _cloneTethers.ContainsKey(o.EntityId)), new Vector2(100f, 100f), new Vector2(96f, 86f));
	}

	private static Vector2 V2(Vector3 p)
	{
		return new Vector2(p.X, p.Z);
	}

	private static float AngleCw(Vector2 v)
	{
		float num = MathF.Atan2(v.X, 0f - v.Y) * (180f / (float)Math.PI);
		if (num < 0f)
		{
			num += 360f;
		}
		return num;
	}

	private static List<IGameObject> EnumCw(IEnumerable<IGameObject> objs, Vector2 center, Vector2 start)
	{
		float s = AngleCw(start - center);
		return objs.OrderBy((IGameObject o) => Norm(AngleCw(V2(o.Position) - center) - s)).ToList();
	}

	private static List<KeyValuePair<uint, Vector3>> EnumCwKv(Dictionary<uint, Vector3> map, Vector2 center, Vector2 start)
	{
		float s = AngleCw(start - center);
		return map.OrderBy((KeyValuePair<uint, Vector3> kv) => Norm(AngleCw(V2(kv.Value) - center) - s)).ToList();
	}

	private static float Norm(float a)
	{
		return (a % 360f + 360f) % 360f;
	}

	private static bool Approx(float a, float b, float tol)
	{
		return MathF.Abs(a - b) <= tol;
	}

	private static Vector3 SafespotPos(int party)
	{
		if (party != 1)
		{
			return new Vector3(101f, 0f, 109f);
		}
		return new Vector3(98.5f, 0f, 109f);
	}

	private static Vector3 DefaGroupPos(int g)
	{
		if (g != 1)
		{
			return new Vector3(113.899f, 0f, 86.115f);
		}
		return new Vector3(86.234f, 0f, 86.02f);
	}

	private static Vector3 StackGroupPos(int g)
	{
		if (g != 1)
		{
			return new Vector3(108f, 0f, 100f);
		}
		return new Vector3(92f, 0f, 100f);
	}

	private static Vector3 BaitPos(string name)
	{
		return name switch
		{
			"Given Far" => new Vector3(110.152f, 0f, 98.237f), 
			"Given Near" => new Vector3(106.973f, 0f, 94.048f), 
			"Taken Far" => new Vector3(114.708f, 0f, 109.144f), 
			"Taken Near" => new Vector3(108.921f, 0f, 92.552f), 
			_ => new Vector3(100f, 0f, 100f), 
		};
	}

	private static Vector3 SafePlatformPos(string key)
	{
		if (key != null)
		{
			int length = key.Length;
			if (length != 17)
			{
				if (length == 18)
				{
					char c = key[4];
					if (c != 'E')
					{
						if (c == 'W')
						{
							if (key == "SafeWestLeftRightA")
							{
								goto IL_00c6;
							}
							if (key == "SafeWestFrontBackA")
							{
								goto IL_00f4;
							}
						}
					}
					else
					{
						if (key == "SafeEastLeftRightA")
						{
							goto IL_00dd;
						}
						if (key == "SafeEastFrontBackA")
						{
							goto IL_010b;
						}
					}
				}
			}
			else
			{
				char c = key[4];
				if (c != 'E')
				{
					if (c == 'W')
					{
						if (key == "SafeWestLeftRight")
						{
							goto IL_00c6;
						}
						if (key == "SafeWestFrontBack")
						{
							goto IL_00f4;
						}
					}
				}
				else
				{
					if (key == "SafeEastLeftRight")
					{
						goto IL_00dd;
					}
					if (key == "SafeEastFrontBack")
					{
						goto IL_010b;
					}
				}
			}
		}
		return new Vector3(100f, 0f, 100f);
		IL_010b:
		return new Vector3(108f, 0f, 100f);
		IL_00f4:
		return new Vector3(92f, 0f, 100f);
		IL_00c6:
		return new Vector3(85f, 0f, 95f);
		IL_00dd:
		return new Vector3(115f, 0f, 95f);
	}

	private Vector3 ElPos(string key)
	{
		if (!_el.TryGetValue(key, out StaticVfx value) || !(value.Position != Vector3.Zero))
		{
			return new Vector3(100f, 0f, 100f);
		}
		return value.Position;
	}

	private void Build()
	{
		if (!_built)
		{
			_built = true;
			string[] array = new string[12]
			{
				"DefamationGroup1", "DefamationGroup2", "StackGroup1", "StackGroup2", "SafespotGroup1", "SafespotGroup2", "Given Far", "Given Near", "Taken Far", "Taken Near",
				"p7sub1 tether", "DefamationOnYou"
			};
			foreach (string key in array)
			{
				MakeCircle(key, 1f, Guide);
			}
			MakeCircle("TowerTether", 3f, Guide);
			MakeCircle("stack tether", 4.5f, Guide);
			MakeCircle("P7AOERadius", 6.3f, MeteorColor);
			MakeCircle("Rock1", 4f, TowerColor);
			MakeCircle("Rock2", 4f, TowerColor);
			MakeCircle("PickTetherCircle", 2.5f, Guide);
			array = new string[8] { "SafeWestLeftRight", "SafeEastLeftRight", "SafeWestFrontBack", "SafeEastFrontBack", "SafeWestLeftRightA", "SafeEastLeftRightA", "SafeWestFrontBackA", "SafeEastFrontBackA" };
			foreach (string key2 in array)
			{
				MakeCircle(key2, 3f, SafeColor);
			}
			MakeDonut("Defamation1", 1f, 19f, DefaColor);
			MakeDonut("Defamation2", 1f, 19f, DefaColor);
			MakeCircle("Stack1", 4.5f, StackColor);
			MakeCircle("Stack2", 4.5f, StackColor);
			MakeCircle("Circle", 10f, MeteorColor);
			MakeLine("PickTether", Guide);
			array = new string[8] { "Cone1", "Cone2", "Cone3", "Cone4", "PortalConeNS1", "PortalConeNS2", "PortalConeEW1", "PortalConeEW2" };
			foreach (string key3 in array)
			{
				MakeCone(key3, 40f, 90, ConeColor);
			}
			array = new string[4] { "FarCone1", "FarCone2", "NearCone1", "NearCone2" };
			foreach (string key4 in array)
			{
				MakeCone(key4, 60f, 30, ConeColor);
			}
		}
	}

	private void Add(string key, StaticVfx? v)
	{
		if (v != null)
		{
			v.Enable = false;
			_el[key] = v;
			aoes.Add(v);
		}
	}

	private void MakeCircle(string key, float radius, Vector4 color)
	{
		Add(key, DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customCircle",
			radiusX = radius,
			radiusZ = radius,
			drawOnObject = false,
			Position = new Vector3(100f, 0f, 100f),
			refColor = color,
			refTargetColor = color,
			destroyTime = 6000000f
		}));
	}

	private void MakeDonut(string key, float inner, float outer, Vector4 color)
	{
		Add(key, DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customDonut",
			radiusX = outer,
			radiusZ = outer,
			refRadian = inner / outer,
			drawOnObject = false,
			Position = new Vector3(100f, 0f, 100f),
			refColor = color,
			refTargetColor = color,
			destroyTime = 6000000f
		}));
	}

	private void MakeCone(string key, float length, int degree, Vector4 color)
	{
		Add(key, DrawManager.Draw(new DrawElement
		{
			drawAvfx = ShapeUtil.GetGameFanOmen(degree),
			refRadian = degree.Degrees().Rad,
			radiusX = length,
			radiusZ = length,
			fixRotation = true,
			drawOnObject = false,
			Position = new Vector3(100f, 0f, 100f),
			refColor = color,
			refTargetColor = color,
			destroyTime = 6000000f
		}));
	}

	private void MakeLine(string key, Vector4 color)
	{
		Add(key, DrawManager.Draw(new DrawElement
		{
			drawAvfx = "customRect",
			radiusX = 0.4f,
			radiusY = 1f,
			radiusZ = 1f,
			drawOnObject = false,
			endToTarget = true,
			Position = new Vector3(100f, 0f, 100f),
			targetPosition = new Vector3(100f, 0f, 100f),
			refColor = color,
			refTargetColor = color,
			destroyTime = 6000000f
		}));
	}

	private void HideAll()
	{
		foreach (StaticVfx value in _el.Values)
		{
			value.Enable = false;
		}
	}

	private void ShowAt(string key, Vector3 pos, Vector4 color)
	{
		if (_el.TryGetValue(key, out StaticVfx value))
		{
			value.Position = pos;
			value.Color = color;
			value.TargetColor = color;
			value.Enable = true;
		}
	}

	private void ShowLine(string key, Vector3 from, Vector3 to, Vector4 color)
	{
		if (_el.TryGetValue(key, out StaticVfx value))
		{
			value.Position = from;
			value.TargetPosition = to;
			value.Color = color;
			value.TargetColor = color;
			value.Enable = true;
		}
	}

	private void ShowCone(string key, Vector3 pos, float rotRad, Vector4 color)
	{
		if (_el.TryGetValue(key, out StaticVfx value))
		{
			value.Position = pos;
			value.Rotation = rotRad.Radians();
			value.Color = color;
			value.TargetColor = color;
			value.Enable = true;
		}
	}

	private void ShowConeBetween(string key, Vector3 from, Vector3 to)
	{
		Vector3 vector = to - from;
		float rotRad = MathF.Atan2(vector.X, vector.Z);
		ShowCone(key, from, rotRad, ConeColor);
	}

	public override void Reset()
	{
		IsRunning = false;
		ClearGuide();
		base.Reset();
		_el.Clear();
		_built = false;
		_stackFinal = null;
		_phase = 0;
		_phase7Sub = 0;
		_phase11Sub = 0;
		_defamationAttack = 0;
		_playerPosition = -1;
		_isCardinalFirst = null;
		_isThDecreasingResistance = null;
		_isConeSafeNorth = null;
		_nextCleavesNorthSouth = null;
		_nextAOE = null;
		_nextCleaves.Clear();
		_clonePositions.Clear();
		_defamationPlayers.Clear();
		_playerOrder.Clear();
		_cloneTethers.Clear();
		_captureTowersAt = 0L;
		_towersCaptured = false;
		_towers = MakeTowerArray();
	}

	private static Vector4 HsvToRgb(float h, float s, float v)
	{
		float x = 0f;
		float y = 0f;
		float z = 0f;
		int num = (int)(h * 6f);
		float num2 = h * 6f - (float)num;
		float num3 = v * (1f - s);
		float num4 = v * (1f - num2 * s);
		float num5 = v * (1f - (1f - num2) * s);
		switch (num % 6)
		{
		case 0:
			x = v;
			y = num5;
			z = num3;
			break;
		case 1:
			x = num4;
			y = v;
			z = num3;
			break;
		case 2:
			x = num3;
			y = v;
			z = num5;
			break;
		case 3:
			x = num3;
			y = num4;
			z = v;
			break;
		case 4:
			x = num5;
			y = num3;
			z = v;
			break;
		case 5:
			x = v;
			y = num3;
			z = num4;
			break;
		}
		return new Vector4(x, y, z, 1f);
	}

	public override void DrawConfig()
	{
		EnsureEnableMigrated();
		bool active = ModuleConfig.IsEnabled(ModuleEnableKey);
		if (StratUI.Header("Idyllic Dream — Uptime (Tired)", ref active))
		{
			ModuleConfig.SetEnabled(ModuleEnableKey, active);
			C.Active = active;
			ModuleConfig.Save<Config>();
		}
		StratUI.Hint("Defaults are the tired/zenith uptime guide (defamations north, stacks E/W). Set your platform, tower and bait below.");
		StratUI.Section("Platform");
		int selected = ((!C.IsGroup1) ? 1 : 0);
		if (StratUI.SegmentedBar(new string[2] { "West (LP1)", "East (LP2)" }, ref selected))
		{
			C.IsGroup1 = selected == 0;
			ModuleConfig.Save<Config>();
		}
		StratUI.Section("My tower position (looking at boss)");
		int selected2 = (int)C.TowerPosition;
		if (StratUI.SegmentedBar(TowerNames, ref selected2))
		{
			C.TowerPosition = (TowerPosition)selected2;
			ModuleConfig.Save<Config>();
		}
		StratUI.Section("Near / Far baits");
		int selected3 = ((!C.TakenCheckConditionIsTakenTower) ? 1 : 0);
		if (StratUI.SegmentedBar(new string[2] { "By taken tower", "By role" }, ref selected3))
		{
			C.TakenCheckConditionIsTakenTower = selected3 == 0;
			ModuleConfig.Save<Config>();
		}
		if (C.TakenCheckConditionIsTakenTower)
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TextDisabled("Wind (far) baited by:");
			ImGui.SameLine();
			int selected4 = ((!C.TakenFarIsEarth) ? 1 : 0);
			if (StratUI.SegmentedBar(new string[2] { "Earth player", "Fire player" }, ref selected4))
			{
				C.TakenFarIsEarth = selected4 == 0;
				ModuleConfig.Save<Config>();
			}
		}
		else
		{
			ImGui.AlignTextToFramePadding();
			ImGui.TextDisabled("Wind (far) baited by:");
			ImGui.SameLine();
			int selected5 = ((!C.TakenFarIsMelee) ? 1 : 0);
			if (StratUI.SegmentedBar(new string[2] { "Earth/Fire melee", "Earth/Fire ranged" }, ref selected5))
			{
				C.TakenFarIsMelee = selected5 == 0;
				ModuleConfig.Save<Config>();
			}
		}
		StratUI.Section("Show");
		bool v = C.ShowTetherLine;
		if (ImGui.Checkbox("Tether-pickup as line", ref v))
		{
			C.ShowTetherLine = v;
			ModuleConfig.Save<Config>();
		}
		bool v2 = C.ShowTetherCircle;
		if (ImGui.Checkbox("Mark the clone to grab from", ref v2))
		{
			C.ShowTetherCircle = v2;
			ModuleConfig.Save<Config>();
		}
		bool v3 = C.ShowGuidePath;
		if (ImGui.Checkbox("Path to my defamation/stack spot", ref v3))
		{
			C.ShowGuidePath = v3;
			ModuleConfig.Save<Config>();
		}
		bool v4 = C.ShowGuideText;
		if (ImGui.Checkbox("Callout text over me (DEFAMATION / STACK)", ref v4))
		{
			C.ShowGuideText = v4;
			ModuleConfig.Save<Config>();
		}
		bool v5 = C.DontShowElementsP11S1;
		if (ImGui.Checkbox("Don't visualise tower debuffs (cones/tethers)", ref v5))
		{
			C.DontShowElementsP11S1 = v5;
			ModuleConfig.Save<Config>();
		}
		StratUI.Hint("Hides the wind/doom tower cone + tether resolution (phase 11). Other AoEs still show.");
		bool v6 = C.SkipIndiMechs;
		if (ImGui.Checkbox("Don't resolve individual mechanics (only show danger AoEs)", ref v6))
		{
			C.SkipIndiMechs = v6;
			ModuleConfig.Save<Config>();
		}
		StratUI.Hint("Shows stack/defamation/stored AoEs only. Won't point you to your tether, your stack/spread spot, or your tower.");
		StratUI.Section("Preview");
		bool v7 = C.Preview;
		if (ImGui.Checkbox("Preview fixed positions in arena (out of combat)", ref v7))
		{
			C.Preview = v7;
			ModuleConfig.Save<Config>();
		}
		if (C.Preview)
		{
			StratUI.Hint("Drops the configured stand-here markers in the arena: defamations (purple), stacks (green), safe spots, and the four near/far baits. Live clone/tether/player resolution only renders during the real pull.");
		}
		if (ImGui.CollapsingHeader("Debug"))
		{
			ImU8String text = new ImU8String(28, 4);
			text.AppendLiteral("Phase ");
			text.AppendFormatted(_phase);
			text.AppendLiteral("  sub7 ");
			text.AppendFormatted(_phase7Sub);
			text.AppendLiteral("  sub11 ");
			text.AppendFormatted(_phase11Sub);
			text.AppendLiteral("  defa ");
			text.AppendFormatted(Adj());
			ImGui.TextUnformatted(text);
			ImU8String text2 = new ImU8String(26, 2);
			text2.AppendLiteral("PlayerPos ");
			text2.AppendFormatted(_playerPosition);
			text2.AppendLiteral("  CardinalFirst ");
			text2.AppendFormatted(_isCardinalFirst?.ToString() ?? "?");
			ImGui.TextUnformatted(text2);
			ImU8String text3 = new ImU8String(24, 2);
			text3.AppendLiteral("ConeSafeNorth ");
			text3.AppendFormatted(_isConeSafeNorth?.ToString() ?? "?");
			text3.AppendLiteral("  THlight ");
			text3.AppendFormatted(_isThDecreasingResistance?.ToString() ?? "?");
			ImGui.TextUnformatted(text3);
			ImU8String text4 = new ImU8String(30, 3);
			text4.AppendLiteral("NS cleaves ");
			text4.AppendFormatted(_nextCleavesNorthSouth?.ToString() ?? "?");
			text4.AppendLiteral("  cleaves ");
			text4.AppendFormatted(_nextCleaves.Count);
			text4.AppendLiteral("  towers ");
			text4.AppendFormatted(_towersCaptured ? "set" : "no");
			ImGui.TextUnformatted(text4);
			ImU8String text5 = new ImU8String(34, 3);
			text5.AppendLiteral("Clone tethers ");
			text5.AppendFormatted(_cloneTethers.Count);
			text5.AppendLiteral("  positions ");
			text5.AppendFormatted(_clonePositions.Count);
			text5.AppendLiteral("  order ");
			text5.AppendFormatted(_playerOrder.Count);
			ImGui.TextUnformatted(text5);
		}
	}
}
