using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;
using Replica.Logging;
using Replica.QuickDraws;

namespace Replica.Modules.DancingMad.P3;

public class MahjongCharge : ISpecialAction
{
	private const string GuideOwner = "mahjong_lc_guide";

	private static readonly Vector3 Center = new Vector3(100f, 0f, 100f);

	private const uint DashStartDataId = 19451u;

	private const uint UltimaBlasterHitId = 47844u;

	private const float CenterIgnoreRadius = 5f;

	private const float DestinationRadius = 19f;

	private const float GuideDurationSec = 8f;

	private const float GuideDurationMs = 8000f;

	private Vector3 _firstPos;

	private Vector3 _lastPos;

	private bool _clockwise;

	private readonly Dictionary<int, IGameObject> _players = new Dictionary<int, IGameObject>();

	private readonly Dictionary<ulong, int> _numbers = new Dictionary<ulong, int>();

	private int _dashStartCount;

	private int _firstStartDir;

	private Vector3 _firstStartPos;

	private int _firstDashDir;

	private int _dashStep;

	private bool _hasSolution;

	private bool _guideDrawn;

	private StaticVfx? _guideCircle;

	private int _lcHits;

	private bool _macroSent;

	private static readonly string[] SymOrder = new string[8] { "1", "A", "2", "B", "3", "C", "4", "D" };

	private static readonly string[] CwBase = new string[8] { "B3", "2B", "A2", "1A", "D1", "4D", "C4", "3C" };

	private static readonly string[] CcwBase = new string[8] { "3C", "C4", "4D", "D1", "1A", "A2", "2B", "B3" };

	public override string Name => "Limit Cut";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47843u, 47844u };

	public override bool HasConfig => true;

	public override void DrawConfig()
	{
		bool v = Plugin.Config.MahjongShowMyGuide;
		if (ImGui.Checkbox("Show my Limit Cut path", ref v))
		{
			Plugin.Config.MahjongShowMyGuide = v;
			Plugin.Config.Save();
		}
		ImGui.TextDisabled("Draws an ImGui path from you to your spot plus a circle to stand on.");
		ImGui.Separator();
		int currentItem = (int)Plugin.Config.MahjongMacroSend;
		string[] array = new string[3] { "Off", "Echo to me (/e)", "Party (/p)" };
		ImGui.SetNextItemWidth(180f);
		if (ImGui.Combo("Send spot macro", ref currentItem, array, array.Length))
		{
			Plugin.Config.MahjongMacroSend = (MahjongMacroMode)currentItem;
			Plugin.Config.Save();
		}
		ImGui.TextDisabled("Posts the number->spot list once per pull. Needs waymarks placed.");
		if (Plugin.Config.MahjongMacroSend == MahjongMacroMode.Party)
		{
			ImGui.TextDisabled("Party mode posts to /p for everyone. Test with Echo first.");
		}
		if (ImGui.Button("Test echo pattern"))
		{
			SendSpotMacro("/e ", "2", clockwise: true);
		}
		ImGui.SameLine();
		if (ImGui.Button("Test party pattern"))
		{
			SendSpotMacro("/p ", "2", clockwise: true);
		}
		ImGui.TextDisabled("Sends a sample 2-start CW list to /e or /p.");
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		CaptureDashStart(info.SourceId.GameObject());
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 47844)
		{
			_lcHits++;
			if (_lcHits >= 8)
			{
				ClearLcGuide();
			}
		}
		else if (info.ActionId == 47843)
		{
			base.NumCasts++;
			if (base.NumCasts == 1)
			{
				_firstPos = info.Source.Position;
			}
			if (base.NumCasts == 2)
			{
				_lastPos = info.Source.Position;
			}
			if (_firstPos != default(Vector3) && _lastPos != default(Vector3))
			{
				_clockwise = IsClockwise(Center, _firstPos, _lastPos);
			}
			CaptureDashStart(info.Source);
		}
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		int num = icon switch
		{
			336u => 1, 
			337u => 2, 
			338u => 3, 
			339u => 4, 
			437u => 5, 
			438u => 6, 
			439u => 7, 
			440u => 8, 
			_ => 0, 
		};
		if (num == 0)
		{
			return;
		}
		_players[num] = Source;
		_numbers[Source.GameObjectId] = num;
		if (_players.Count != 8)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>(8) { _firstPos };
		float num2 = (_clockwise ? (-1f) : 1f);
		Vector3 vector = _firstPos - Center;
		for (int i = 1; i < 8; i++)
		{
			float x = num2 * (float)i * ((float)Math.PI / 4f);
			float num3 = MathF.Cos(x);
			float num4 = MathF.Sin(x);
			float x2 = vector.X * num3 - vector.Z * num4;
			float z = vector.X * num4 + vector.Z * num3;
			list.Add(Center + new Vector3(x2, 0f, z));
		}
		for (int j = 0; j < _players.Count; j++)
		{
			if (_players[j + 1] != Svc.Objects.LocalPlayer)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					Position = list[j],
					drawOnObject = false,
					radiusX = 3f,
					radiusZ = 100f,
					target = _players[j + 1],
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47844u },
						TargetHitCount = j + 1
					}
				});
			}
		}
	}

	public override void Update()
	{
		TryDrawGuide();
		TrySendMacro();
	}

	private void CaptureDashStart(IGameObject? source)
	{
		if (source == null || _hasSolution || _dashStartCount >= 2 || source.BaseId != 19451)
		{
			return;
		}
		Vector3 vector = new Vector3(source.Position.X, 0f, source.Position.Z);
		if (Vector3.Distance(vector, Center) < 5f)
		{
			return;
		}
		int num = DirFromPos(vector);
		if (_dashStartCount == 0)
		{
			_firstStartDir = num;
			_firstStartPos = vector;
			_dashStartCount = 1;
		}
		else if (num != _firstStartDir)
		{
			_dashStartCount = 2;
			int num2 = Wrap(num - _firstStartDir);
			if (num2 != 0 && num2 != 4)
			{
				_firstDashDir = Wrap(_firstStartDir + 4);
				_dashStep = ((num2 < 4) ? 1 : (-1));
				_hasSolution = true;
			}
		}
	}

	private void TryDrawGuide()
	{
		if (_guideDrawn || !_hasSolution || !Plugin.Config.MahjongShowMyGuide || Plugin.Instance == null)
		{
			return;
		}
		IGameObject localPlayer = Svc.Objects.LocalPlayer;
		if (localPlayer != null && _numbers.TryGetValue(localPlayer.GameObjectId, out var value))
		{
			int num = -_dashStep;
			Vector3 vector = PosForDir((float)_firstDashDir + (float)num * ((float)(value - 1) + 0.5f), 19f);
			Vector4 vector2 = new Vector4(0.1f, 0.9f, 1f, 1f);
			Plugin.Instance.Engine.ClearExternal("mahjong_lc_guide");
			Plugin.Instance.Engine.SpawnExternal("mahjong_lc_guide", new DrawSpec
			{
				Shape = QuickShape.ChevronPath,
				Anchor = DrawAnchor.Self,
				AttachToActor = true,
				Link = LinkTarget.FixedSpot,
				LinkPosition = vector,
				Color = vector2,
				ChevronSpacing = 2f,
				LineThickness = 4f,
				Length = 30f,
				Duration = 8f
			}, new LogEvent
			{
				Name = "mahjong_lc_guide"
			}, previewSelf: true);
			DrawElement obj = new DrawElement
			{
				drawAvfx = "customCircle",
				drawOnObject = false,
				Position = vector,
				radiusX = 2f,
				radiusZ = 2f
			};
			Vector4 refColor = vector2;
			refColor.W = Plugin.Config.CustomAlpha;
			obj.refColor = refColor;
			refColor = vector2;
			refColor.W = Plugin.Config.CustomAlpha;
			obj.refTargetColor = refColor;
			obj.destroyTime = 8000f;
			_guideCircle = DrawManager.Draw(obj);
			if (_guideCircle != null)
			{
				aoes.Add(_guideCircle);
			}
			_guideDrawn = true;
		}
	}

	private void ClearLcGuide()
	{
		Plugin.Instance?.Engine.ClearExternal("mahjong_lc_guide");
		if (_guideCircle != null)
		{
			_guideCircle.Remove();
			aoes.Remove(_guideCircle);
			_guideCircle = null;
		}
	}

	private void TrySendMacro()
	{
		if (!_macroSent && _hasSolution && Plugin.Config.MahjongMacroSend != MahjongMacroMode.Off)
		{
			string startWaymarkSymbol = GetStartWaymarkSymbol();
			if (startWaymarkSymbol != null)
			{
				bool clockwise = _dashStep == 1;
				_macroSent = true;
				SendSpotMacro((Plugin.Config.MahjongMacroSend == MahjongMacroMode.Party) ? "/p " : "/e ", startWaymarkSymbol, clockwise);
			}
		}
	}

	private static void SendSpotMacro(string prefix, string startSym, bool clockwise)
	{
		string[] array = MacroSpots(clockwise, startSym);
		if (array != null)
		{
			ChatSender.Send(prefix + $"Limit Cut ({startSym} start, {(clockwise ? "CW" : "CCW")})");
			for (int i = 0; i < 8; i++)
			{
				ChatSender.Send($"{prefix}{i + 1} -> {array[i]}");
			}
		}
	}

	private unsafe string? GetStartWaymarkSymbol()
	{
		MarkingController* ptr = MarkingController.Instance();
		if (ptr == null)
		{
			return null;
		}
		int num = -1;
		float num2 = float.MaxValue;
		int num3 = 0;
		Span<FieldMarker> fieldMarkers = ptr->FieldMarkers;
		for (int i = 0; i < fieldMarkers.Length; i++)
		{
			ref FieldMarker reference = ref fieldMarkers[i];
			if (num3 >= 8)
			{
				break;
			}
			if (reference.Active)
			{
				float num4 = Vector3.Distance(new Vector3((float)reference.X / 1000f, 0f, (float)reference.Z / 1000f), new Vector3(_firstStartPos.X, 0f, _firstStartPos.Z));
				if (num4 < num2)
				{
					num2 = num4;
					num = num3;
				}
			}
			num3++;
		}
		return num switch
		{
			0 => "A", 
			1 => "B", 
			2 => "C", 
			3 => "D", 
			4 => "1", 
			5 => "2", 
			6 => "3", 
			7 => "4", 
			_ => null, 
		};
	}

	private static string[]? MacroSpots(bool clockwise, string startSym)
	{
		int num = Array.IndexOf(SymOrder, startSym);
		if (num < 0)
		{
			return null;
		}
		string[] array = (clockwise ? CwBase : CcwBase);
		string[] array2 = new string[8];
		for (int i = 0; i < 8; i++)
		{
			int num2 = (clockwise ? Wrap8(i - num) : Wrap8(i + num));
			array2[i] = array[num2];
		}
		return array2;
	}

	private static int Wrap8(int n)
	{
		return (n % 8 + 8) % 8;
	}

	public override void Reset()
	{
		_players.Clear();
		_numbers.Clear();
		_firstPos = default(Vector3);
		_lastPos = default(Vector3);
		_clockwise = false;
		_dashStartCount = 0;
		_firstStartDir = 0;
		_firstStartPos = default(Vector3);
		_firstDashDir = 0;
		_dashStep = 0;
		_hasSolution = false;
		_guideDrawn = false;
		_guideCircle = null;
		_lcHits = 0;
		Plugin.Instance?.Engine.ClearExternal("mahjong_lc_guide");
		_macroSent = false;
		base.Reset();
	}

	private static bool IsClockwise(Vector3 center, Vector3 a, Vector3 b)
	{
		return Vector3.Cross(a - center, b - center).Y < 0f;
	}

	private static int DirFromPos(Vector3 p)
	{
		return Wrap((int)MathF.Round(MathF.Atan2(p.X - Center.X, Center.Z - p.Z) / ((float)Math.PI / 4f)));
	}

	private static Vector3 PosForDir(float dir, float radius)
	{
		float x = dir * ((float)Math.PI / 4f);
		return new Vector3(Center.X + MathF.Sin(x) * radius, 0f, Center.Z - MathF.Cos(x) * radius);
	}

	private static int Wrap(int d)
	{
		return (d % 8 + 8) % 8;
	}
}
