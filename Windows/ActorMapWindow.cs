using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using Replica.Logging;

namespace Replica.Windows;

public sealed class ActorMapWindow : Window, IDisposable
{
	private enum MapCategory : byte
	{
		You,
		Party,
		Enemy,
		Ally,
		Pet,
		Object
	}

	private readonly struct MapActor(uint id, string name, MapCategory cat, Vector3 pos, float rot, uint dataId, uint baseId, float hpPct, bool casting, uint castId, float castRemain, uint job)
	{
		public readonly uint Id = id;

		public readonly string Name = name;

		public readonly MapCategory Cat = cat;

		public readonly Vector3 Pos = pos;

		public readonly float Rot = rot;

		public readonly uint DataId = dataId;

		public readonly uint BaseId = baseId;

		public readonly float HpPct = hpPct;

		public readonly bool Casting = casting;

		public readonly uint CastId = castId;

		public readonly float CastRemain = castRemain;

		public readonly uint Job = job;
	}

	private const float CenterX = 100f;

	private const float CenterZ = 100f;

	private const float ReplayFade = 6f;

	private static readonly Vector4 ColYou = new Vector4(0.35f, 0.85f, 1f, 1f);

	private static readonly Vector4 ColParty = new Vector4(0.4f, 0.85f, 0.5f, 1f);

	private static readonly Vector4 ColEnemy = new Vector4(0.96f, 0.42f, 0.42f, 1f);

	private static readonly Vector4 ColAlly = new Vector4(0.55f, 0.9f, 0.7f, 1f);

	private static readonly Vector4 ColPet = new Vector4(0.7f, 0.85f, 0.55f, 1f);

	private static readonly Vector4 ColObject = new Vector4(0.66f, 0.66f, 0.7f, 1f);

	private static readonly Vector4 ColId = new Vector4(0.6f, 0.7f, 0.85f, 1f);

	private readonly Plugin _plugin;

	private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new Dictionary<uint, ISharedImmediateTexture>();

	private readonly List<MapActor> _liveActors = new List<MapActor>();

	private float _viewRadius = 40f;

	private float _maxRadius = 200f;

	private float _centerX = 100f;

	private float _centerZ = 100f;

	private bool _dragging;

	private uint _lastTerr = uint.MaxValue;

	private uint _mapId = uint.MaxValue;

	private ISharedImmediateTexture? _mapTex;

	private float _mapScale = 1f;

	private float _mapOffX;

	private float _mapOffZ;

	private bool _replayMode;

	private uint _replayTerr;

	private uint _replayMapId;

	private string _search = "";

	private uint _selectedId;

	private uint _ctxId;

	private MapAoe? _selectedAoe;

	private MapAoe? _hoveredAoe;

	private string _aoeCopyFeedback = "";

	private DateTime _aoeCopyTime = DateTime.MinValue;

	private static readonly uint[] WaymarkIcons = new uint[8] { 61241u, 61242u, 61243u, 61247u, 61244u, 61245u, 61246u, 61248u };

	private int _replayPull;

	private int _cachedPull = -1;

	private bool _playing;

	private double _playT;

	private float _playSpeed = 1f;

	private DateTime _lastFrame = DateTime.Now;

	private DateTime _pullStart;

	private double _pullDuration = 1.0;

	private readonly List<LogEvent> _pullEvents = new List<LogEvent>();

	private readonly List<CombatLogCapture.MapFrame> _pullFrames = new List<CombatLogCapture.MapFrame>();

	private static readonly string[] ShapeNames = new string[4] { "Circle", "Triangle", "Square", "Diamond" };

	private static Configuration Cfg => Plugin.Config;

	public ActorMapWindow(Plugin plugin)
		: base("Replica Live Map###ReplicaMap")
	{
		_plugin = plugin;
		base.SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(560f, 420f),
			MaximumSize = new Vector2(2400f, 2000f)
		};
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

	public override void Draw()
	{
		DateTime now = DateTime.Now;
		double totalSeconds = (now - _lastFrame).TotalSeconds;
		_lastFrame = now;
		uint territoryType = Plugin.ClientState.TerritoryType;
		if (territoryType != _lastTerr)
		{
			_lastTerr = territoryType;
			RecenterView();
		}
		DrawTopBar();
		if (_replayMode)
		{
			try
			{
				EnsurePullCache();
				AdvancePlay(totalSeconds);
				DrawReplayControls();
			}
			catch (Exception ex)
			{
				ImU8String text = new ImU8String(14, 1);
				text.AppendLiteral("replay error: ");
				text.AppendFormatted(ex.Message);
				ImGui.TextDisabled(text);
			}
		}
		ImGui.Separator();
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		float x = ImGui.GetStyle().ItemSpacing.X;
		float globalScale = ImGuiHelpers.GlobalScale;
		float num = MathF.Min(240f * globalScale, contentRegionAvail.X * 0.5f);
		float max = MathF.Max(num, contentRegionAvail.X - 220f);
		float num2 = Math.Clamp(contentRegionAvail.X * 0.32f, num, max);
		float x2 = MathF.Max(120f, contentRegionAvail.X - num2 - x);
		float y = MathF.Max(160f, contentRegionAvail.Y);
		if (ImGui.BeginChild("##mappane", new Vector2(x2, y), border: false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
		{
			float num3 = MathF.Max(120f, MathF.Min(x2, y));
			try
			{
				DrawCanvas(num3);
			}
			catch (Exception ex2)
			{
				ImGui.Dummy(new Vector2(num3, num3));
				ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
				Vector2 itemRectMin = ImGui.GetItemRectMin();
				uint col = ImGui.ColorConvertFloat4ToU32(ColEnemy);
				ImU8String text2 = new ImU8String(11, 1);
				text2.AppendLiteral("map error: ");
				text2.AppendFormatted(ex2.Message);
				windowDrawList.AddText(itemRectMin, col, text2);
			}
		}
		ImGui.EndChild();
		ImGui.SameLine();
		if (ImGui.BeginChild("##sidepanel", new Vector2(num2, y), border: true))
		{
			try
			{
				if (_selectedAoe.HasValue)
				{
					DrawAoeInspectorBody(_selectedAoe.Value);
				}
				else if (_replayMode)
				{
					DrawReplayListBody();
				}
				else
				{
					DrawActorListBody();
				}
			}
			catch (Exception ex3)
			{
				ImU8String text3 = new ImU8String(12, 1);
				text3.AppendLiteral("list error: ");
				text3.AppendFormatted(ex3.Message);
				ImGui.TextDisabled(text3);
			}
		}
		ImGui.EndChild();
		DrawActorContextPopup();
	}

	private void DrawTopBar()
	{
		if (ModeButton("Live", !_replayMode))
		{
			_replayMode = false;
		}
		ImGui.SameLine();
		if (ModeButton("Replay", _replayMode))
		{
			if (!_replayMode)
			{
				JumpToLatestPull();
			}
			_replayMode = true;
		}
		ImGui.SameLine();
		ImGui.TextDisabled("|");
		ImGui.SameLine();
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Zoom");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(120f * ImGuiHelpers.GlobalScale);
		ImGui.SliderFloat("##zoom", ref _viewRadius, 5f, _maxRadius, "%.0fy", ImGuiSliderFlags.Logarithmic);
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Scroll the map to zoom toward the cursor · drag to pan.");
		}
		ImGui.SameLine();
		if (ImGui.Button("Recenter"))
		{
			RecenterView();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Reset pan + zoom (centres on you, or the map centre in replay).");
		}
		ImGui.SameLine();
		DrawCaptureScope();
		ImGui.SameLine();
		if (ImGui.Button("View"))
		{
			ImGui.OpenPopup("##viewpop");
		}
		DrawViewPopup();
		ImGui.SameLine();
		if (ImGui.Button("Filters"))
		{
			ImGui.OpenPopup("##filterpop");
		}
		DrawFilterPopup();
		if (_selectedId != 0)
		{
			ImGui.SameLine();
			ImU8String text = new ImU8String(6, 1);
			text.AppendLiteral("sel 0x");
			text.AppendFormatted(_selectedId, "X8");
			ImGui.TextColored(in ColId, text);
			ImGui.SameLine();
			if (ImGui.SmallButton("clear##sel"))
			{
				_selectedId = 0u;
			}
		}
		if (_selectedAoe.HasValue)
		{
			ImGui.SameLine();
			var selAoe = _selectedAoe.Value;
			uint actId = selAoe.ActionId != 0 ? selAoe.ActionId : InferredActionId(selAoe, _playT);
			string aName = ActionName(actId);
			string label = !string.IsNullOrEmpty(aName) ? aName : selAoe.GetShapeName();
			ImGui.TextColored(Ui.Gold, $"aoe [{Shorten(label)}]");
			ImGui.SameLine();
			if (ImGui.SmallButton("clear##aoesel"))
			{
				_selectedAoe = null;
			}
		}
	}

	private void DrawCaptureScope()
	{
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Track");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(110f * ImGuiHelpers.GlobalScale);
		string[] array = new string[4] { "Always", "In combat", "In duty", "Disabled" };
		int num = (int)Cfg.CaptureWhen;
		if (num < 0 || num >= array.Length)
		{
			num = 0;
		}
		if (ImGui.BeginCombo("##capwhen", array[num]))
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (ImGui.Selectable(array[i], i == num))
				{
					Cfg.CaptureWhen = (CaptureMode)i;
					Cfg.Save();
					_plugin.Capture.UpdateHookStates();
					if (Cfg.CaptureWhen == CaptureMode.Disabled)
					{
						_plugin.Capture.TrimPulls();
						_plugin.Capture.SaveToDisk();
					}
				}
			}
			ImGui.EndCombo();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("When the map records actors and events:\nAlways – everywhere, even in town.\nIn combat – only while you're in a fight.\nIn duty – only inside instanced duties.");
		}
	}

	private void DrawViewPopup()
	{
		if (ImGui.BeginPopup("##viewpop"))
		{
			CfgToggle("In-game map", () => Cfg.MapShowGameMap, delegate(bool mapShowGameMap)
			{
				Cfg.MapShowGameMap = mapShowGameMap;
			}, "Use the zone's actual map art as the floor. Falls back to the grid where no map exists.");
			CfgToggle("Waymarks", () => Cfg.MapShowWaymarks, delegate(bool mapShowWaymarks)
			{
				Cfg.MapShowWaymarks = mapShowWaymarks;
			});
			CfgToggle("Names", () => Cfg.MapShowNames, delegate(bool mapShowNames)
			{
				Cfg.MapShowNames = mapShowNames;
			});
			CfgToggle("Hide dead", () => Cfg.MapHideDead, delegate(bool mapHideDead)
			{
				Cfg.MapHideDead = mapHideDead;
			}, "Hide actors at 0 HP. The game keeps despawning actors in the table for a while after a pull.");
			CfgToggle("Job icons", () => Cfg.MapJobIcons, delegate(bool mapJobIcons)
			{
				Cfg.MapJobIcons = mapJobIcons;
			}, "Show each player's job icon instead of a plain marker (live only).");
			CfgToggle("AOEs / Mechanics", () => Cfg.MapShowAoes, delegate(bool mapShowAoes)
			{
				Cfg.MapShowAoes = mapShowAoes;
			}, "Draw BossMod & module AOEs (circles, cones, donuts, rects, safe spots, arrows) on the map.");
			if (Cfg.MapShowAoes)
			{
				float opacity = Cfg.MapAoeOpacity * 100f;
				ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
				if (ImGui.SliderFloat("AOE opacity", ref opacity, 10f, 90f, "%.0f%%"))
				{
					Cfg.MapAoeOpacity = opacity / 100f;
					Cfg.Save();
				}
			}
			ImGui.Separator();
			ImGui.TextDisabled("Sizes & shapes");
			float v = Cfg.MapWaymarkSize;
			ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
			if (ImGui.SliderFloat("Waymark size", ref v, 6f, 48f, "%.0f"))
			{
				Cfg.MapWaymarkSize = v;
				Cfg.Save();
			}
			float v2 = Cfg.MapMarkerScale;
			ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
			if (ImGui.SliderFloat("Actor size", ref v2, 0.5f, 2.5f, "%.2fx"))
			{
				Cfg.MapMarkerScale = v2;
				Cfg.Save();
			}
			ShapeCombo("Player shape", () => Cfg.MapPlayerShape, delegate(int mapPlayerShape)
			{
				Cfg.MapPlayerShape = mapPlayerShape;
			});
			ShapeCombo("Enemy shape", () => Cfg.MapEnemyShape, delegate(int mapEnemyShape)
			{
				Cfg.MapEnemyShape = mapEnemyShape;
			});
			ImGui.EndPopup();
		}
	}

	private void DrawFilterPopup()
	{
		if (ImGui.BeginPopup("##filterpop"))
		{
			ImGui.TextDisabled("Show on map");
			ImGui.Separator();
			CfgToggle("Players (you + party)", () => Cfg.MapShowPlayers, delegate(bool v)
			{
				Cfg.MapShowPlayers = v;
			});
			CfgToggle("Enemies / adds", () => Cfg.MapShowEnemies, delegate(bool v)
			{
				Cfg.MapShowEnemies = v;
			});
			CfgToggle("Allied NPCs", () => Cfg.MapShowAllies, delegate(bool v)
			{
				Cfg.MapShowAllies = v;
			}, "Friendly battle NPCs (escort/duty allies).");
			CfgToggle("Pets / minions", () => Cfg.MapShowPets, delegate(bool v)
			{
				Cfg.MapShowPets = v;
			}, "Carbuncle, fairy, chocobo, minions.");
			CfgToggle("Objects", () => Cfg.MapShowObjects, delegate(bool v)
			{
				Cfg.MapShowObjects = v;
			}, "Exits, aetherytes, event objects, treasure, etc.");
			ImGui.Separator();
			CfgToggle("Hide unnamed clutter", () => Cfg.MapHideUnnamed, delegate(bool v)
			{
				Cfg.MapHideUnnamed = v;
			}, "Drop nameless pets/objects (letterboxes, markers) from the map and list.");
			ImGui.EndPopup();
		}
	}

	private static void ShapeCombo(string label, Func<int> get, Action<int> set)
	{
		int num = Math.Clamp(get(), 0, ShapeNames.Length - 1);
		ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
		if (!ImGui.BeginCombo(label, ShapeNames[num]))
		{
			return;
		}
		for (int i = 0; i < ShapeNames.Length; i++)
		{
			if (ImGui.Selectable(ShapeNames[i], i == num))
			{
				set(i);
				Cfg.Save();
			}
		}
		ImGui.EndCombo();
	}

	private static void CfgToggle(string label, Func<bool> get, Action<bool> set, string? tip = null)
	{
		bool v = get();
		if (ImGui.Checkbox(label, ref v))
		{
			set(v);
			Cfg.Save();
		}
		if (tip != null && ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(tip);
		}
	}

	private static bool ModeButton(string label, bool active)
	{
		if (active)
		{
			ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
		}
		bool result = ImGui.Button(label);
		if (active)
		{
			ImGui.PopStyleColor();
		}
		return result;
	}

	private Vector2 ToScreen(float wx, float wz, Vector2 origin, float size)
	{
		float num = size * 0.5f;
		float num2 = num / _viewRadius;
		return new Vector2(origin.X + num + (wx - _centerX) * num2, origin.Y + num + (wz - _centerZ) * num2);
	}

	private (float wx, float wz) ToWorld(Vector2 sp, Vector2 origin, float size)
	{
		float num = size * 0.5f;
		float num2 = _viewRadius / num;
		return (wx: _centerX + (sp.X - origin.X - num) * num2, wz: _centerZ + (sp.Y - origin.Y - num) * num2);
	}

	private void RecenterView()
	{
		IPlayerCharacter playerCharacter = (_replayMode ? null : Plugin.ObjectTable.LocalPlayer);
		if (playerCharacter != null)
		{
			_centerX = playerCharacter.Position.X;
			_centerZ = playerCharacter.Position.Z;
		}
		else
		{
			EnsureMapTexture(CurrentMapId());
			_centerX = 0f - _mapOffX;
			_centerZ = 0f - _mapOffZ;
		}
		_viewRadius = MathF.Min(40f, _maxRadius);
	}

	private void DrawCanvas(float size)
	{
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton("##mapcanvas", new Vector2(size, size), ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
		bool flag = ImGui.IsItemHovered();
		bool num = ImGui.IsItemActive();
		Vector2 mousePos = ImGui.GetMousePos();
		if (flag)
		{
			float mouseWheel = ImGui.GetIO().MouseWheel;
			if (mouseWheel != 0f)
			{
				(float wx, float wz) tuple = ToWorld(mousePos, cursorScreenPos, size);
				float item = tuple.wx;
				float item2 = tuple.wz;
				_viewRadius = Math.Clamp(_viewRadius - mouseWheel * _viewRadius * 0.12f, 5f, _maxRadius);
				float num2 = size * 0.5f;
				float num3 = _viewRadius / num2;
				_centerX = item - (mousePos.X - cursorScreenPos.X - num2) * num3;
				_centerZ = item2 - (mousePos.Y - cursorScreenPos.Y - num2) * num3;
			}
		}
		if (num && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 4f))
		{
			Vector2 mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 4f);
			float num4 = _viewRadius / (size * 0.5f);
			_centerX -= mouseDragDelta.X * num4;
			_centerZ -= mouseDragDelta.Y * num4;
			ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
			_dragging = true;
		}
		bool clicked = false;
		if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
		{
			clicked = flag && !_dragging;
			_dragging = false;
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 vector = cursorScreenPos;
		Vector2 vector2 = new Vector2(cursorScreenPos.X + size, cursorScreenPos.Y + size);
		windowDrawList.PushClipRect(vector, vector2, intersectWithCurrentClipRect: true);
		windowDrawList.AddRectFilled(vector, vector2, ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.11f, 1f)), 4f);
		DrawGrid(windowDrawList, cursorScreenPos, vector, vector2, size);
		if (Cfg.MapShowGameMap)
		{
			DrawGameMap(windowDrawList, cursorScreenPos, size);
		}
		DrawCardinals(windowDrawList, vector, vector2);
		if (Cfg.MapShowWaymarks && !_replayMode)
		{
			DrawWaymarks(windowDrawList, cursorScreenPos, size);
		}
		if (_replayMode)
		{
			DrawReplayScene(windowDrawList, cursorScreenPos, size, vector, vector2, flag, clicked, mousePos);
		}
		else
		{
			DrawLiveScene(windowDrawList, cursorScreenPos, size, vector, vector2, flag, clicked, mousePos);
		}
		windowDrawList.PopClipRect();
		windowDrawList.AddRect(vector, vector2, ImGui.ColorConvertFloat4ToU32(new Vector4(0.35f, 0.35f, 0.38f, 1f)), 4f);
	}

	private bool DrawGameMap(ImDrawListPtr dl, Vector2 origin, float size)
	{
		EnsureMapTexture(CurrentMapId());
		IDalamudTextureWrap dalamudTextureWrap = _mapTex?.GetWrapOrDefault();
		if (dalamudTextureWrap == null)
		{
			return false;
		}
		float num = 1024f / _mapScale;
		if (!_replayMode)
		{
			IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
			if (localPlayer != null)
			{
				float num2 = num * 0.05f;
				if (MathF.Abs(localPlayer.Position.X + _mapOffX) > num + num2 || MathF.Abs(localPlayer.Position.Z + _mapOffZ) > num + num2)
				{
					return false;
				}
			}
		}
		Vector2 pMin = ToScreen(0f - _mapOffX - num, 0f - _mapOffZ - num, origin, size);
		Vector2 pMax = ToScreen(0f - _mapOffX + num, 0f - _mapOffZ + num, origin, size);
		dl.AddImage(dalamudTextureWrap.Handle, pMin, pMax);
		return true;
	}

	private unsafe uint CurrentMapId()
	{
		if (_replayMode)
		{
			if (_replayMapId != 0)
			{
				return _replayMapId;
			}
			return TerritoryDefaultMap((_replayTerr != 0) ? _replayTerr : Plugin.ClientState.TerritoryType);
		}
		AgentMap* ptr = AgentMap.Instance();
		if (ptr != null && ptr->CurrentMapId != 0)
		{
			return ptr->CurrentMapId;
		}
		return TerritoryDefaultMap(Plugin.ClientState.TerritoryType);
	}

	private static uint TerritoryDefaultMap(uint terr)
	{
		try
		{
			return (Plugin.DataManager.GetExcelSheet<TerritoryType>().GetRowOrDefault(terr)?.Map.ValueNullable?.RowId).GetValueOrDefault();
		}
		catch
		{
			return 0u;
		}
	}

	private void EnsureMapTexture(uint mapId)
	{
		if (mapId == _mapId)
		{
			return;
		}
		_mapId = mapId;
		_mapTex = null;
		_mapScale = 1f;
		_mapOffX = 0f;
		_mapOffZ = 0f;
		_maxRadius = 200f;
		try
		{
			if (mapId == 0)
			{
				return;
			}
			Lumina.Excel.Sheets.Map? rowOrDefault = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Map>().GetRowOrDefault(mapId);
			if (!rowOrDefault.HasValue)
			{
				return;
			}
			Lumina.Excel.Sheets.Map valueOrDefault = rowOrDefault.GetValueOrDefault();
			if (valueOrDefault.RowId == 0)
			{
				return;
			}
			string text = valueOrDefault.Id.ExtractText();
			if (!string.IsNullOrEmpty(text))
			{
				_mapScale = ((valueOrDefault.SizeFactor > 0) ? ((float)(int)valueOrDefault.SizeFactor / 100f) : 1f);
				_mapOffX = valueOrDefault.OffsetX;
				_mapOffZ = valueOrDefault.OffsetY;
				float num = 1024f / _mapScale;
				_maxRadius = Math.Clamp(num * 1.25f, 120f, 4000f);
				string path = $"ui/map/{text}/{text.Replace("/", "")}_m.tex";
				if (Plugin.DataManager.FileExists(path))
				{
					_mapTex = Plugin.TextureProvider.GetFromGame(path);
				}
			}
		}
		catch
		{
			_mapTex = null;
		}
	}

	private void DrawGrid(ImDrawListPtr dl, Vector2 origin, Vector2 a, Vector2 b, float size)
	{
		uint num = ImGui.ColorConvertFloat4ToU32(new Vector4(0.22f, 0.22f, 0.24f, 0.45f));
		uint num2 = ImGui.ColorConvertFloat4ToU32(new Vector4(0.32f, 0.32f, 0.36f, 0.8f));
		float num3 = size * 0.5f;
		float num4 = num3 / _viewRadius;
		int num5 = (int)(_viewRadius / 5f) + 1;
		for (int i = -num5; i <= num5; i++)
		{
			float num6 = (float)i * 5f;
			if (!(MathF.Abs(num6) > _viewRadius + 0.01f))
			{
				uint col = ((i == 0) ? num2 : num);
				float x = origin.X + num3 + num6 * num4;
				dl.AddLine(new Vector2(x, a.Y), new Vector2(x, b.Y), col);
				float y = origin.Y + num3 + num6 * num4;
				dl.AddLine(new Vector2(a.X, y), new Vector2(b.X, y), col);
			}
		}
	}

	private static void DrawCardinals(ImDrawListPtr dl, Vector2 a, Vector2 b)
	{
		uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0.55f, 0.55f, 0.6f, 0.85f));
		float num = (a.X + b.X) * 0.5f;
		float num2 = (a.Y + b.Y) * 0.5f;
		dl.AddText(new Vector2(num - 4f, a.Y + 3f), col, "N");
		dl.AddText(new Vector2(num - 4f, b.Y - 17f), col, "S");
		dl.AddText(new Vector2(b.X - 14f, num2 - 7f), col, "E");
		dl.AddText(new Vector2(a.X + 5f, num2 - 7f), col, "W");
	}

	private unsafe void DrawWaymarks(ImDrawListPtr dl, Vector2 origin, float size)
	{
		MarkingController* ptr = MarkingController.Instance();
		if (ptr == null)
		{
			return;
		}
		int num = 0;
		Span<FFXIVClientStructs.FFXIV.Client.Game.UI.FieldMarker> fieldMarkers = ptr->FieldMarkers;
		for (int i = 0; i < fieldMarkers.Length; i++)
		{
			ref FFXIVClientStructs.FFXIV.Client.Game.UI.FieldMarker reference = ref fieldMarkers[i];
			if (num < WaymarkIcons.Length)
			{
				if (reference.Active)
				{
					Vector2 center = ToScreen((float)reference.X / 1000f, (float)reference.Z / 1000f, origin, size);
					DrawIconAt(dl, center, Math.Clamp(Cfg.MapWaymarkSize, 6f, 48f), WaymarkIcons[num]);
				}
				num++;
				continue;
			}
			break;
		}
	}

	private void DrawLiveScene(ImDrawListPtr dl, Vector2 origin, float size, Vector2 a, Vector2 b, bool hovered, bool clicked, Vector2 mouse)
	{
		RebuildLiveActors();
		uint num = 0u;
		float num2 = 196f;
		foreach (MapActor liveActor in _liveActors)
		{
			MapActor act = liveActor;
			Vector2 vector = ToScreen(act.Pos.X, act.Pos.Z, origin, size);
			Vector4 vector2 = CatColor(act.Cat);
			if (vector.X < a.X || vector.X > b.X || vector.Y < a.Y || vector.Y > b.Y)
			{
				Vector2 c = new Vector2(Math.Clamp(vector.X, a.X + 7f, b.X - 7f), Math.Clamp(vector.Y, a.Y + 7f, b.Y - 7f));
				Vector4 vector3 = vector2;
				vector3.W = 0.85f;
				Vector4 input = vector3;
				DrawDiamond(dl, c, 5f, ImGui.ColorConvertFloat4ToU32(input), filled: false);
				Vector2 pos = new Vector2(c.X + 7f, c.Y - 6f);
				uint col = ImGui.ColorConvertFloat4ToU32(input);
				ImU8String text = new ImU8String(4, 3);
				text.AppendFormatted(Shorten(act.Name));
				text.AppendLiteral(" (");
				text.AppendFormatted(act.Pos.X, "0");
				text.AppendLiteral(",");
				text.AppendFormatted(act.Pos.Z, "0");
				text.AppendLiteral(")");
				dl.AddText(pos, col, text);
				continue;
			}
			DrawFacing(dl, vector, act.Rot, vector2);
			DrawLiveMarker(dl, vector, in act, vector2);
			if (act.Id == _selectedId)
			{
				dl.AddCircle(vector, 10f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f)), 16, 2f);
			}
			else if (SearchHit(act))
			{
				dl.AddCircle(vector, 9f, ImGui.ColorConvertFloat4ToU32(Ui.Gold), 16, 1.6f);
			}
			if (act.Casting)
			{
				dl.AddCircle(vector, 8f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.55f, 0.3f, 0.9f)), 16, 1.5f);
			}
			if (Cfg.MapShowNames && !string.IsNullOrEmpty(act.Name))
			{
				Vector2 pos2 = new Vector2(vector.X + 7f, vector.Y - 6f);
				Vector4 vector3 = vector2;
				vector3.W = 0.85f;
				dl.AddText(pos2, ImGui.ColorConvertFloat4ToU32(vector3), Shorten(act.Name));
			}
			if (hovered)
			{
				float num3 = mouse.X - vector.X;
				float num4 = mouse.Y - vector.Y;
				float num5 = num3 * num3 + num4 * num4;
				if (num5 < num2)
				{
					num2 = num5;
					num = act.Id;
				}
			}
		}

		bool actorClicked = false;
		if (num != 0)
		{
			if (clicked)
			{
				_selectedId = num;
				_selectedAoe = null;
				actorClicked = true;
				ImU8String clipboardText = new ImU8String(0, 1);
				clipboardText.AppendFormatted(num, "X8");
				ImGui.SetClipboardText(clipboardText);
			}
			if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				_ctxId = num;
				ImGui.OpenPopup("##actorctx");
			}
			if (!ImGui.IsPopupOpen("##actorctx"))
			{
				ShowActorTooltip(num);
			}
		}
		else if (clicked && _hoveredAoe == null)
		{
			_selectedId = 0u;
		}

		List<MapAoe>? liveAoes = _plugin.BossModBridge?.GetActiveMapAoes();
		if (liveAoes != null && liveAoes.Count > 0)
		{
			DrawMapAoes(dl, origin, size, liveAoes, hovered, clicked && !actorClicked, mouse, 0, num);
		}

		DrawLiveTethers(dl, origin, size);
		DrawLiveHeadmarkers(dl, origin, size);
	}

	private void RebuildLiveActors()
	{
		_liveActors.Clear();
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			if (item == null)
			{
				continue;
			}
			try
			{
				MapCategory mapCategory = Classify(item);
				if (!CategoryVisible(mapCategory))
				{
					continue;
				}
				string textValue = item.Name.TextValue;
				bool flag = Cfg.MapHideUnnamed && string.IsNullOrEmpty(textValue);
				if (flag)
				{
					bool flag2 = mapCategory - 4 <= MapCategory.Party;
					flag = flag2;
				}
				if (flag)
				{
					continue;
				}
				float hpPct = -1f;
				bool casting = false;
				uint castId = 0u;
				float castRemain = 0f;
				uint baseId = item.BaseId;
				bool flag3 = false;
				uint job = 0u;
				if (item is IBattleChara battleChara)
				{
					baseId = battleChara.BaseId;
					if (battleChara.MaxHp != 0)
					{
						hpPct = (float)battleChara.CurrentHp / (float)battleChara.MaxHp * 100f;
						flag3 = battleChara.CurrentHp == 0;
					}
					if (battleChara.IsCasting)
					{
						casting = true;
						castId = battleChara.CastActionId;
						castRemain = MathF.Max(0f, battleChara.TotalCastTime - battleChara.CurrentCastTime);
					}
				}
				if (item is IPlayerCharacter { ClassJob: var classJob })
				{
					job = classJob.RowId;
				}
				if (!(Cfg.MapHideDead & flag3))
				{
					_liveActors.Add(new MapActor(item.EntityId, textValue, mapCategory, item.Position, item.Rotation, item.BaseId, baseId, hpPct, casting, castId, castRemain, job));
				}
			}
			catch
			{
			}
		}
	}

	private static bool CategoryVisible(MapCategory cat)
	{
		switch (cat)
		{
		case MapCategory.You:
		case MapCategory.Party:
			return Cfg.MapShowPlayers;
		case MapCategory.Enemy:
			return Cfg.MapShowEnemies;
		case MapCategory.Ally:
			return Cfg.MapShowAllies;
		case MapCategory.Pet:
			return Cfg.MapShowPets;
		default:
			return Cfg.MapShowObjects;
		}
	}

	private void DrawLiveMarker(ImDrawListPtr dl, Vector2 sp, in MapActor act, Vector4 col)
	{
		float num = Math.Clamp(Cfg.MapMarkerScale, 0.4f, 3f);
		MapCategory cat = act.Cat;
		bool flag = ((cat <= MapCategory.Party || cat == MapCategory.Ally) ? true : false);
		if (flag && Cfg.MapJobIcons && act.Job != 0)
		{
			float num2 = ((act.Cat == MapCategory.You) ? 22f : 18f) * num;
			float radius = num2 * 0.5f + 2f;
			dl.AddCircleFilled(sp, radius, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.55f)), 16);
			DrawIconAt(dl, sp, num2, 62100 + act.Job);
			uint num3;
			if (act.Cat != MapCategory.You)
			{
				Vector4 input = col;
				input.W = 0.85f;
				num3 = ImGui.ColorConvertFloat4ToU32(input);
			}
			else
			{
				num3 = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f));
			}
			uint col2 = num3;
			dl.AddCircle(sp, radius, col2, 16, (act.Cat == MapCategory.You) ? 1.8f : 1.2f);
			return;
		}
		int shape;
		float num4;
		switch (act.Cat)
		{
		case MapCategory.You:
		case MapCategory.Party:
		case MapCategory.Ally:
			shape = Cfg.MapPlayerShape;
			num4 = ((act.Cat == MapCategory.You) ? 8f : 6f);
			break;
		case MapCategory.Enemy:
			shape = Cfg.MapEnemyShape;
			num4 = 7f;
			break;
		default:
			shape = 0;
			num4 = 5f;
			break;
		}
		DrawMarker(dl, sp, shape, num4 * num, col, act.Cat == MapCategory.You);
	}

	private void DrawLiveTethers(ImDrawListPtr dl, Vector2 origin, float size)
	{
		uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.45f, 1f, 0.7f));
		foreach (CombatLogCapture.LiveTether activeTether in _plugin.Capture.ActiveTethers)
		{
			IGameObject gameObject = Plugin.ObjectTable.SearchById(activeTether.From);
			IGameObject gameObject2 = Plugin.ObjectTable.SearchById(activeTether.To);
			if (gameObject != null && gameObject2 != null)
			{
				dl.AddLine(ToScreen(gameObject.Position.X, gameObject.Position.Z, origin, size), ToScreen(gameObject2.Position.X, gameObject2.Position.Z, origin, size), col, 1.5f);
			}
		}
	}

	private void DrawLiveHeadmarkers(ImDrawListPtr dl, Vector2 origin, float size)
	{
		foreach (CombatLogCapture.LiveHeadmarker activeHeadmarker in _plugin.Capture.ActiveHeadmarkers)
		{
			IGameObject gameObject = Plugin.ObjectTable.SearchById(activeHeadmarker.ActorId);
			if (gameObject != null)
			{
				Vector2 center = ToScreen(gameObject.Position.X, gameObject.Position.Z, origin, size);
				dl.AddCircle(center, 7f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.2f, 0.9f)), 12, 1.6f);
				DrawIconAt(dl, new Vector2(center.X + 13f, center.Y), 16f, activeHeadmarker.IconId);
			}
		}
	}

	private void ShowActorTooltip(uint id)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(id);
		if (gameObject == null)
		{
			return;
		}
		ImGui.BeginTooltip();
		ImGui.TextColored(CatColor(Classify(gameObject)), string.IsNullOrEmpty(gameObject.Name.TextValue) ? "(unnamed)" : gameObject.Name.TextValue);
		ImU8String text = new ImU8String(9, 1);
		text.AppendLiteral("Entity 0x");
		text.AppendFormatted(gameObject.EntityId, "X8");
		ImGui.TextColored(in ColId, text);
		ImU8String text2 = new ImU8String(13, 2);
		text2.AppendLiteral("Kind ");
		text2.AppendFormatted(gameObject.ObjectKind);
		text2.AppendLiteral("   Data ");
		text2.AppendFormatted(gameObject.BaseId);
		ImGui.TextDisabled(text2);
		if (gameObject is IBattleChara battleChara)
		{
			ImU8String text3 = new ImU8String(12, 2);
			text3.AppendLiteral("BaseId ");
			text3.AppendFormatted(battleChara.BaseId);
			text3.AppendLiteral(" (0x");
			text3.AppendFormatted(battleChara.BaseId, "X");
			text3.AppendLiteral(")");
			ImGui.TextDisabled(text3);
			if (battleChara.MaxHp != 0)
			{
				ImU8String text4 = new ImU8String(4, 1);
				text4.AppendLiteral("HP ");
				text4.AppendFormatted((float)battleChara.CurrentHp / (float)battleChara.MaxHp * 100f, "0.#");
				text4.AppendLiteral("%");
				ImGui.TextColored(in ColParty, text4);
			}
			if (battleChara.IsCasting)
			{
				float value = MathF.Max(0f, battleChara.TotalCastTime - battleChara.CurrentCastTime);
				Vector4 col = new Vector4(1f, 0.55f, 0.3f, 1f);
				ImU8String text5 = new ImU8String(12, 2);
				text5.AppendLiteral("casting ");
				text5.AppendFormatted(ActionName(battleChara.CastActionId));
				text5.AppendLiteral(" (");
				text5.AppendFormatted(value, "0.0");
				text5.AppendLiteral("s)");
				ImGui.TextColored(in col, text5);
			}
		}
		ImU8String text6 = new ImU8String(8, 2);
		text6.AppendLiteral("pos (");
		text6.AppendFormatted(gameObject.Position.X, "0.0");
		text6.AppendLiteral(", ");
		text6.AppendFormatted(gameObject.Position.Z, "0.0");
		text6.AppendLiteral(")");
		ImGui.TextDisabled(text6);
		ImGui.TextDisabled("right-click for actions");
		ImGui.EndTooltip();
	}

	private void DrawActorContextPopup()
	{
		if (ImGui.BeginPopup("##actorctx"))
		{
			ActorContextBody(_ctxId);
			ImGui.EndPopup();
		}
	}

	private void ActorContextBody(uint id)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(id);
		if (gameObject == null)
		{
			ImGui.TextDisabled("(no longer present)");
			return;
		}
		string text = (string.IsNullOrEmpty(gameObject.Name.TextValue) ? "(unnamed)" : gameObject.Name.TextValue);
		ImGui.TextColored(CatColor(Classify(gameObject)), text);
		ImGui.Separator();
		if (ImGui.MenuItem("Select / focus"))
		{
			_selectedId = id;
		}
		ImU8String label = new ImU8String(19, 1);
		label.AppendLiteral("Copy entity id   0x");
		label.AppendFormatted(gameObject.EntityId, "X8");
		if (ImGui.MenuItem(label))
		{
			ImU8String clipboardText = new ImU8String(2, 1);
			clipboardText.AppendLiteral("0x");
			clipboardText.AppendFormatted(gameObject.EntityId, "X8");
			ImGui.SetClipboardText(clipboardText);
		}
		ImU8String label2 = new ImU8String(19, 1);
		label2.AppendLiteral("Copy data id     0x");
		label2.AppendFormatted(gameObject.BaseId, "X");
		if (ImGui.MenuItem(label2))
		{
			ImU8String clipboardText2 = new ImU8String(2, 1);
			clipboardText2.AppendLiteral("0x");
			clipboardText2.AppendFormatted(gameObject.BaseId, "X");
			ImGui.SetClipboardText(clipboardText2);
		}
		if (gameObject is IBattleNpc battleNpc)
		{
			ImU8String label3 = new ImU8String(17, 1);
			label3.AppendLiteral("Copy name id     ");
			label3.AppendFormatted(battleNpc.NameId);
			if (ImGui.MenuItem(label3))
			{
				ImGui.SetClipboardText(battleNpc.NameId.ToString());
			}
		}
		if (gameObject is IBattleChara battleChara)
		{
			ImU8String label4 = new ImU8String(19, 1);
			label4.AppendLiteral("Copy base id     0x");
			label4.AppendFormatted(battleChara.BaseId, "X");
			if (ImGui.MenuItem(label4))
			{
				ImU8String clipboardText3 = new ImU8String(2, 1);
				clipboardText3.AppendLiteral("0x");
				clipboardText3.AppendFormatted(battleChara.BaseId, "X");
				ImGui.SetClipboardText(clipboardText3);
			}
			if (battleChara.IsCasting)
			{
				ImU8String label5 = new ImU8String(22, 2);
				label5.AppendLiteral("Copy cast id     0x");
				label5.AppendFormatted(battleChara.CastActionId, "X");
				label5.AppendLiteral(" (");
				label5.AppendFormatted(ActionName(battleChara.CastActionId));
				label5.AppendLiteral(")");
				if (ImGui.MenuItem(label5))
				{
					ImU8String clipboardText4 = new ImU8String(2, 1);
					clipboardText4.AppendLiteral("0x");
					clipboardText4.AppendFormatted(battleChara.CastActionId, "X");
					ImGui.SetClipboardText(clipboardText4);
				}
			}
		}
		ImGui.Separator();
		Vector3 position = gameObject.Position;
		ImU8String label6 = new ImU8String(21, 2);
		label6.AppendLiteral("Copy position    (");
		label6.AppendFormatted(position.X, "0.0");
		label6.AppendLiteral(", ");
		label6.AppendFormatted(position.Z, "0.0");
		label6.AppendLiteral(")");
		if (ImGui.MenuItem(label6))
		{
			ImU8String clipboardText5 = new ImU8String(20, 3);
			clipboardText5.AppendLiteral("new Vector3(");
			clipboardText5.AppendFormatted(position.X, "0.###");
			clipboardText5.AppendLiteral("f, ");
			clipboardText5.AppendFormatted(position.Y, "0.###");
			clipboardText5.AppendLiteral("f, ");
			clipboardText5.AppendFormatted(position.Z, "0.###");
			clipboardText5.AppendLiteral("f)");
			ImGui.SetClipboardText(clipboardText5);
		}
		ImU8String label7 = new ImU8String(17, 1);
		label7.AppendLiteral("Copy heading     ");
		label7.AppendFormatted(gameObject.Rotation, "0.000");
		if (ImGui.MenuItem(label7))
		{
			ImU8String clipboardText6 = new ImU8String(1, 1);
			clipboardText6.AppendFormatted(gameObject.Rotation, "0.#####");
			clipboardText6.AppendLiteral("f");
			ImGui.SetClipboardText(clipboardText6);
		}
		if (ImGui.MenuItem("Copy name"))
		{
			ImGui.SetClipboardText(gameObject.Name.TextValue);
		}
	}

	private void DrawActorListBody()
	{
		ImGui.SetNextItemWidth(-1f);
		ImGui.InputTextWithHint("##mapsearch", "filter name / id (hex or dec)…", ref _search, 64);
		ImU8String text = new ImU8String(7, 1);
		text.AppendFormatted(_liveActors.Count);
		text.AppendLiteral(" actors");
		ImGui.TextDisabled(text);
		ImGui.Separator();
		DrawActorGroup("Enemies", MapCategory.Enemy);
		DrawActorGroup("You / Party", MapCategory.You, MapCategory.Party);
		DrawActorGroup("Allies / Pets", MapCategory.Ally, MapCategory.Pet);
		DrawActorGroup("Objects", MapCategory.Object);
	}

	private void DrawActorGroup(string label, params MapCategory[] cats)
	{
		bool flag = false;
		foreach (MapActor liveActor in _liveActors)
		{
			if (Array.IndexOf(cats, liveActor.Cat) >= 0 && (SearchHit(liveActor) || string.IsNullOrEmpty(_search)))
			{
				if (!flag)
				{
					ImGui.TextDisabled(label);
					flag = true;
				}
				DrawActorRow(liveActor);
			}
		}
		if (flag)
		{
			ImGui.Spacing();
		}
	}

	private void DrawActorRow(MapActor act)
	{
		bool flag = act.Id == _selectedId;
		string value = (string.IsNullOrEmpty(act.Name) ? "(unnamed)" : act.Name);
		ImGui.PushStyleColor(ImGuiCol.Text, CatColor(act.Cat));
		ImU8String label = new ImU8String(3, 2);
		label.AppendFormatted(value);
		label.AppendLiteral("##a");
		label.AppendFormatted(act.Id);
		if (ImGui.Selectable(label, flag))
		{
			_selectedId = ((!flag) ? act.Id : 0u);
			if (_selectedId != 0)
			{
				ImU8String clipboardText = new ImU8String(0, 1);
				clipboardText.AppendFormatted(act.Id, "X8");
				ImGui.SetClipboardText(clipboardText);
			}
		}
		ImGui.PopStyleColor();
		ImU8String strId = new ImU8String(5, 1);
		strId.AppendLiteral("##ctx");
		strId.AppendFormatted(act.Id);
		if (ImGui.BeginPopupContextItem(strId))
		{
			ActorContextBody(act.Id);
			ImGui.EndPopup();
		}
		if (ImGui.IsItemHovered())
		{
			ImU8String tooltip = new ImU8String(56, 1);
			tooltip.AppendLiteral("0x");
			tooltip.AppendFormatted(act.Id, "X8");
			tooltip.AppendLiteral("\nleft-click selects + copies · right-click for actions");
			ImGui.SetTooltip(tooltip);
		}
		ImGui.SameLine();
		ImU8String text = new ImU8String(2, 1);
		text.AppendLiteral("0x");
		text.AppendFormatted(act.Id, "X8");
		ImGui.TextColored(in ColId, text);
		if (act.HpPct >= 0f)
		{
			ImGui.SameLine();
			ImU8String text2 = new ImU8String(1, 1);
			text2.AppendFormatted(act.HpPct, "0");
			text2.AppendLiteral("%");
			ImGui.TextDisabled(text2);
		}
		if (act.Casting)
		{
			ImGui.SameLine();
			Vector4 col = new Vector4(1f, 0.55f, 0.3f, 1f);
			ImU8String text3 = new ImU8String(2, 1);
			text3.AppendLiteral("» ");
			text3.AppendFormatted(ActionName(act.CastId));
			ImGui.TextColored(in col, text3);
		}
	}

	private bool SearchHit(MapActor act)
	{
		if (string.IsNullOrEmpty(_search))
		{
			return false;
		}
		if (act.Name.Contains(_search, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (!IdMatches(act.Id, _search) && !IdMatches(act.DataId, _search))
		{
			return IdMatches(act.BaseId, _search);
		}
		return true;
	}

	private void EnsurePullCache()
	{
		if (_replayPull == 0)
		{
			IReadOnlyList<CombatLogCapture.PullInfo> pulls = _plugin.Capture.Pulls;
			if (pulls.Count > 0)
			{
				_replayPull = pulls[pulls.Count - 1].Index;
			}
		}
		if (_replayPull == _cachedPull)
		{
			return;
		}
		_cachedPull = _replayPull;
		_playT = 0.0;
		_playing = false;
		_pullEvents.Clear();
		_pullFrames.Clear();
		CombatLogCapture.PullInfo pullInfo = FindPull(_replayPull);
		if (pullInfo == null)
		{
			_pullStart = DateTime.Now;
			_pullDuration = 1.0;
			return;
		}
		if ((pullInfo.Territory != 0 && pullInfo.Territory != _replayTerr) || pullInfo.MapId != _replayMapId)
		{
			_replayTerr = pullInfo.Territory;
			_replayMapId = pullInfo.MapId;
			EnsureMapTexture(CurrentMapId());
			RecenterView();
		}
		_pullStart = pullInfo.Start;
		foreach (LogEvent @event in _plugin.Capture.Events)
		{
			if (@event.Pull == _replayPull)
			{
				_pullEvents.Add(@event);
			}
		}
		_pullEvents.Sort((LogEvent x, LogEvent y) => x.Seq.CompareTo(y.Seq));
		foreach (CombatLogCapture.MapFrame frame in _plugin.Capture.Frames)
		{
			if (frame.Pull == _replayPull)
			{
				_pullFrames.Add(frame);
			}
		}
		_pullFrames.Sort((CombatLogCapture.MapFrame x, CombatLogCapture.MapFrame y) => x.T.CompareTo(y.T));
		double num;
		if (_pullFrames.Count <= 0)
		{
			num = 0.0;
		}
		else
		{
			List<CombatLogCapture.MapFrame> pullFrames = _pullFrames;
			num = pullFrames[pullFrames.Count - 1].T;
		}
		double val = num;
		double num2;
		if (_pullEvents.Count <= 0)
		{
			num2 = 0.0;
		}
		else
		{
			List<LogEvent> pullEvents = _pullEvents;
			num2 = (pullEvents[pullEvents.Count - 1].Time - _pullStart).TotalSeconds;
		}
		double val2 = num2;
		double totalSeconds = (((pullInfo.End == DateTime.MinValue) ? pullInfo.Start : pullInfo.End) - _pullStart).TotalSeconds;
		_pullDuration = Math.Max(1.0, Math.Max(totalSeconds, Math.Max(val, val2)));
	}

	private void JumpToLatestPull()
	{
		IReadOnlyList<CombatLogCapture.PullInfo> pulls = _plugin.Capture.Pulls;
		if (pulls.Count != 0)
		{
			_replayPull = pulls[pulls.Count - 1].Index;
			_cachedPull = -1;
		}
	}

	private CombatLogCapture.PullInfo? FindPull(int index)
	{
		foreach (CombatLogCapture.PullInfo pull in _plugin.Capture.Pulls)
		{
			if (pull.Index == index)
			{
				return pull;
			}
		}
		return null;
	}

	private void AdvancePlay(double dt)
	{
		if (_playing)
		{
			_playT += dt * (double)_playSpeed;
			if (_playT >= _pullDuration)
			{
				_playT = _pullDuration;
				_playing = false;
			}
		}
	}

	private void DrawReplayControls()
	{
		IReadOnlyList<CombatLogCapture.PullInfo> pulls = _plugin.Capture.Pulls;
		if (pulls.Count == 0)
		{
			ImGui.TextDisabled("No pulls captured yet — start a fight (or set Track to Always).");
			return;
		}
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Pull");
		ImGui.SameLine();
		float comboWidth = Math.Clamp(ImGui.GetContentRegionAvail().X * 0.45f, 340f * ImGuiHelpers.GlobalScale, 520f * ImGuiHelpers.GlobalScale);
		ImGui.SetNextItemWidth(comboWidth);
		if (ImGui.BeginCombo("##pullsel", PullLabel(FindPull(_replayPull))))
		{
			for (int num = pulls.Count - 1; num >= 0; num--)
			{
				CombatLogCapture.PullInfo pullInfo = pulls[num];
				if (ImGui.Selectable(PullLabel(pullInfo), pullInfo.Index == _replayPull))
				{
					_replayPull = pullInfo.Index;
				}
			}
			ImGui.EndCombo();
		}
		ImGui.SameLine();
		if (ImGui.Button("Latest"))
		{
			JumpToLatestPull();
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Jump to the most recent pull (your last wipe / clear).");
		}
		ImGui.SameLine();
		if (ImGui.Button(_playing ? "Pause" : "Play"))
		{
			_playing = !_playing;
		}
		ImGui.SameLine();
		if (ImGui.Button("Reset"))
		{
			_playT = 0.0;
			_playing = false;
		}
		ImGui.SameLine();
		if (ImGui.Button("Export"))
		{
			ExportReplayPull(_replayPull);
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Export this replay pull (frames + events) to a JSON file.");
		}
		ImGui.SameLine();
		ImGui.SetNextItemWidth(100f * ImGuiHelpers.GlobalScale);
		ImGui.SliderFloat("##speed", ref _playSpeed, 0.25f, 4f, "%.2fx");
		float v = (float)_playT;
		ImGui.SetNextItemWidth(-1f);
		if (ImGui.SliderFloat("##timeline", ref v, 0f, (float)_pullDuration, FormatTime(v)))
		{
			_playT = v;
			_playing = false;
		}
		DrawNearReadout();
	}

	private void DrawNearReadout()
	{
		ImU8String text = new ImU8String(7, 2);
		text.AppendLiteral("t = ");
		text.AppendFormatted(FormatTime((float)_playT));
		text.AppendLiteral(" / ");
		text.AppendFormatted(FormatTime((float)_pullDuration));
		ImGui.TextDisabled(text);
		if (_pullFrames.Count == 0)
		{
			ImGui.SameLine();
			ImGui.TextColored(in Ui.Gold, "· no movement recorded (only event markers)");
		}
		int num = 0;
		int num2 = _pullEvents.Count - 1;
		while (num2 >= 0 && num < 3)
		{
			LogEvent logEvent = _pullEvents[num2];
			if (!(Offset(logEvent) > _playT))
			{
				var (value, col) = KindTag(logEvent.Kind);
				ImGui.SameLine();
				ImU8String text2 = new ImU8String(3, 2);
				text2.AppendLiteral("· ");
				text2.AppendFormatted(value);
				text2.AppendLiteral(" ");
				text2.AppendFormatted(Shorten(string.IsNullOrEmpty(logEvent.Name) ? logEvent.SourceName : logEvent.Name));
				ImGui.TextColored(in col, text2);
				num++;
			}
			num2--;
		}
	}

	private CombatLogCapture.MapFrame? FrameAt(double t)
	{
		if (_pullFrames.Count == 0)
		{
			return null;
		}
		int num = 0;
		int num2 = _pullFrames.Count - 1;
		int index = 0;
		while (num <= num2)
		{
			int num3 = (num + num2) / 2;
			if (_pullFrames[num3].T <= t)
			{
				index = num3;
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return _pullFrames[index];
	}

	private void DrawReplayTrail(ImDrawListPtr dl, Vector2 origin, float size)
	{
		if (_selectedId == 0)
		{
			return;
		}
		Vector2? vector = null;
		foreach (CombatLogCapture.MapFrame pullFrame in _pullFrames)
		{
			if (pullFrame.T > _playT)
			{
				break;
			}
			if (_playT - pullFrame.T > 14.0)
			{
				continue;
			}
			CombatLogCapture.ActorSample[] actors = pullFrame.Actors;
			for (int i = 0; i < actors.Length; i++)
			{
				CombatLogCapture.ActorSample actorSample = actors[i];
				if (actorSample.Id == _selectedId)
				{
					Vector2 vector2 = ToScreen(actorSample.X, actorSample.Z, origin, size);
					if (vector.HasValue)
					{
						dl.AddLine(vector.Value, vector2, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.45f)), 1.6f);
					}
					vector = vector2;
					break;
				}
			}
		}
	}

	private void DrawReplayActors(ImDrawListPtr dl, Vector2 origin, float size, bool hovered, bool clicked, Vector2 mousePos, out uint hoveredActorId, out bool actorClicked)
	{
		hoveredActorId = 0u;
		actorClicked = false;
		CombatLogCapture.MapFrame mapFrame = FrameAt(_playT);
		if (mapFrame == null)
		{
			return;
		}
		DrawReplayTrail(dl, origin, size);
		Vector2 vector = origin;
		Vector2 vector2 = new Vector2(origin.X + size, origin.Y + size);
		float num2 = 196f;
		CombatLogCapture.ActorSample s = default(CombatLogCapture.ActorSample);
		CombatLogCapture.ActorSample[] actors = mapFrame.Actors;
		for (int i = 0; i < actors.Length; i++)
		{
			CombatLogCapture.ActorSample actorSample = actors[i];
			Vector2 vector3 = ToScreen(actorSample.X, actorSample.Z, origin, size);
			Vector4 vector4 = KindColor(actorSample.Kind);
			if (vector3.X < vector.X || vector3.X > vector2.X || vector3.Y < vector.Y || vector3.Y > vector2.Y)
			{
				Vector2 c = new Vector2(Math.Clamp(vector3.X, vector.X + 7f, vector2.X - 7f), Math.Clamp(vector3.Y, vector.Y + 7f, vector2.Y - 7f));
				ImDrawListPtr dl2 = dl;
				Vector4 input = vector4;
				input.W = 0.8f;
				DrawDiamond(dl2, c, 5f, ImGui.ColorConvertFloat4ToU32(input), filled: false);
				continue;
			}
			DrawFacing(dl, vector3, actorSample.Rot, vector4);
			float num3 = Math.Clamp(Cfg.MapMarkerScale, 0.4f, 3f);
			int shape;
			float num4;
			switch (actorSample.Kind)
			{
			case ActorKind.You:
				shape = Cfg.MapPlayerShape;
				num4 = 8f;
				break;
			case ActorKind.Party:
				shape = Cfg.MapPlayerShape;
				num4 = 6f;
				break;
			case ActorKind.Enemy:
				shape = Cfg.MapEnemyShape;
				num4 = 7f;
				break;
			default:
				shape = 0;
				num4 = 5f;
				break;
			}
			DrawMarker(dl, vector3, shape, num4 * num3, vector4, actorSample.Kind == ActorKind.You);
			if (actorSample.Id == _selectedId)
			{
				dl.AddCircle(vector3, 10f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f)), 16, 2f);
			}
			if (actorSample.CastId != 0)
			{
				dl.AddCircle(vector3, 8f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.55f, 0.3f, 0.9f)), 16, 1.5f);
			}
			if (Cfg.MapShowNames)
			{
				string text = _plugin.Capture.FrameActorName(actorSample.Id);
				if (!string.IsNullOrEmpty(text))
				{
					Vector2 pos = new Vector2(vector3.X + 7f, vector3.Y - 6f);
					Vector4 input = vector4;
					input.W = 0.85f;
					dl.AddText(pos, ImGui.ColorConvertFloat4ToU32(input), Shorten(text));
				}
			}
			if (hovered)
			{
				float num5 = mousePos.X - vector3.X;
				float num6 = mousePos.Y - vector3.Y;
				float num7 = num5 * num5 + num6 * num6;
				if (num7 < num2)
				{
					num2 = num7;
					hoveredActorId = actorSample.Id;
					s = actorSample;
				}
			}
		}
		if (hoveredActorId != 0)
		{
			if (clicked)
			{
				_selectedId = hoveredActorId;
				_selectedAoe = null;
				actorClicked = true;
				ImU8String clipboardText = new ImU8String(0, 1);
				clipboardText.AppendFormatted(hoveredActorId, "X8");
				ImGui.SetClipboardText(clipboardText);
			}
			ShowSampleTooltip(s);
		}
	}

	private void ShowSampleTooltip(CombatLogCapture.ActorSample s)
	{
		ImGui.BeginTooltip();
		string text = _plugin.Capture.FrameActorName(s.Id);
		ImGui.TextColored(KindColor(s.Kind), string.IsNullOrEmpty(text) ? "(unknown)" : text);
		ImU8String text2 = new ImU8String(9, 1);
		text2.AppendLiteral("Entity 0x");
		text2.AppendFormatted(s.Id, "X8");
		ImGui.TextColored(in ColId, text2);
		if (s.HpPct >= 0f)
		{
			ImU8String text3 = new ImU8String(4, 1);
			text3.AppendLiteral("HP ");
			text3.AppendFormatted(s.HpPct, "0.#");
			text3.AppendLiteral("%");
			ImGui.TextColored(in ColParty, text3);
		}
		if (s.CastId != 0)
		{
			Vector4 col = new Vector4(1f, 0.55f, 0.3f, 1f);
			ImU8String text4 = new ImU8String(8, 1);
			text4.AppendLiteral("casting ");
			text4.AppendFormatted(ActionName(s.CastId));
			ImGui.TextColored(in col, text4);
		}
		ImU8String text5 = new ImU8String(8, 2);
		text5.AppendLiteral("pos (");
		text5.AppendFormatted(s.X, "0.0");
		text5.AppendLiteral(", ");
		text5.AppendFormatted(s.Z, "0.0");
		text5.AppendLiteral(")");
		ImGui.TextDisabled(text5);
		ImGui.EndTooltip();
	}

	private void DrawReplayScene(ImDrawListPtr dl, Vector2 origin, float size, Vector2 a, Vector2 b, bool hovered, bool clicked, Vector2 mouse)
	{
		CombatLogCapture.MapFrame mapFrame = FrameAt(_playT);
		DrawReplayActors(dl, origin, size, hovered, clicked, mouse, out uint hoveredActorId, out bool actorClicked);

		if (mapFrame != null && mapFrame.Aoes != null && mapFrame.Aoes.Length > 0)
		{
			DrawMapAoes(dl, origin, size, mapFrame.Aoes, hovered, clicked && !actorClicked, mouse, _playT, hoveredActorId);
		}
		else if (clicked && !actorClicked && _hoveredAoe == null && hoveredActorId == 0)
		{
			_selectedId = 0u;
		}
		foreach (LogEvent pullEvent in _pullEvents)
		{
			double num = Offset(pullEvent);
			if (num > _playT)
			{
				break;
			}
			double num2 = _playT - num;
			switch (pullEvent.Kind)
			{
			case Replica.Logging.LogKind.CastStart:
			case Replica.Logging.LogKind.CastFinish:
			case Replica.Logging.LogKind.Ability:
			case Replica.Logging.LogKind.AbilityExtra:
				if ((pullEvent.X != 0f || pullEvent.Y != 0f) && !(num2 > 6.0))
				{
					float w2 = (float)Math.Clamp(1.0 - num2 / 6.0, 0.18, 1.0);
					Vector4 vector = new Vector4(1f, 0.55f, 0.3f, w2);
					Vector2 vector2 = ToScreen(pullEvent.X, pullEvent.Y, origin, size);
					dl.AddCircleFilled(vector2, 4f, ImGui.ColorConvertFloat4ToU32(vector));
					DrawFacing(dl, vector2, pullEvent.Heading, vector);
					if (num2 < 3.5)
					{
						Vector2 pos = new Vector2(vector2.X + 7f, vector2.Y - 6f);
						Vector4 colEnemy = vector;
						colEnemy.W = w2;
						dl.AddText(pos, ImGui.ColorConvertFloat4ToU32(colEnemy), Shorten(pullEvent.Name));
					}
				}
				break;
			case Replica.Logging.LogKind.Added:
				if ((pullEvent.X != 0f || pullEvent.Y != 0f) && !(num2 > 12.0))
				{
					float w = (float)Math.Clamp(1.0 - num2 / 12.0, 0.2, 0.9);
					Vector2 c = ToScreen(pullEvent.X, pullEvent.Y, origin, size);
					Vector4 colEnemy = ColEnemy;
					colEnemy.W = w;
					Vector4 input = colEnemy;
					DrawDiamond(dl, c, 4f, ImGui.ColorConvertFloat4ToU32(input), filled: false);
				}
				break;
			case Replica.Logging.LogKind.Headmarker:
				if (!(num2 > 6.0))
				{
					IGameObject gameObject = ((pullEvent.SourceId != 0) ? Plugin.ObjectTable.SearchById(pullEvent.SourceId) : null);
					if (gameObject != null)
					{
						Vector2 center = ToScreen(gameObject.Position.X, gameObject.Position.Z, origin, size);
						dl.AddCircle(center, 7f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.2f, 0.9f)), 12, 1.6f);
						DrawIconAt(dl, new Vector2(center.X + 13f, center.Y), 16f, pullEvent.DataId);
					}
				}
				break;
			}
		}
	}

	private void DrawReplayListBody()
	{
		ImU8String text = new ImU8String(33, 2);
		text.AppendFormatted(_pullEvents.Count);
		text.AppendLiteral(" events · ");
		text.AppendFormatted(_pullFrames.Count);
		text.AppendLiteral(" frames · click to seek");
		ImGui.TextDisabled(text);
		if (_pullFrames.Count == 0)
		{
			ImGui.TextColored(in Ui.Gold, "no movement recorded for this pull");
		}
		ImGui.Separator();
		if (!ImGui.BeginTable("##rev", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
		{
			return;
		}
		ImGui.TableSetupColumn("t", ImGuiTableColumnFlags.WidthFixed, 50f * ImGuiHelpers.GlobalScale);
		ImGui.TableSetupColumn("event", ImGuiTableColumnFlags.WidthStretch);
		float textLineHeightWithSpacing = ImGui.GetTextLineHeightWithSpacing();
		ImGuiListClipper imGuiListClipper = default(ImGuiListClipper);
		imGuiListClipper.Begin(_pullEvents.Count, textLineHeightWithSpacing);
		while (imGuiListClipper.Step())
		{
			for (int i = imGuiListClipper.DisplayStart; i < imGuiListClipper.DisplayEnd; i++)
			{
				LogEvent logEvent = _pullEvents[i];
				double num = Offset(logEvent);
				bool num2 = num <= _playT;
				ImGui.TableNextRow();
				ImGui.TableNextColumn();
				(string, Vector4) tuple = KindTag(logEvent.Kind);
				string item = tuple.Item1;
				Vector4 item2 = tuple.Item2;
				ImU8String label = new ImU8String(5, 2);
				label.AppendFormatted(FormatTime((float)num));
				label.AppendLiteral("##rev");
				label.AppendFormatted(logEvent.Seq);
				if (ImGui.Selectable(label, selected: false, ImGuiSelectableFlags.SpanAllColumns))
				{
					_playT = Math.Clamp(num, 0.0, _pullDuration);
					_playing = false;
				}
				ImGui.TableNextColumn();
				Vector4 vector2;
				Vector4 vector;
				if (!num2)
				{
					vector = item2;
					vector.W = 0.4f;
					vector2 = vector;
				}
				else
				{
					vector2 = item2;
				}
				vector = vector2;
				ImGui.TextColored(in vector, item);
				ImGui.SameLine();
				string text2 = (string.IsNullOrEmpty(logEvent.Name) ? logEvent.SourceName : logEvent.Name);
				if (num2)
				{
					ImGui.Text(text2);
				}
				else
				{
					ImGui.TextDisabled(text2);
				}
			}
		}
		imGuiListClipper.End();
		ImGui.EndTable();
	}

	private double Offset(LogEvent e)
	{
		return (e.Time - _pullStart).TotalSeconds;
	}

	private void DrawFacing(ImDrawListPtr dl, Vector2 sp, float heading, Vector4 col)
	{
		Vector2 vector = new Vector2(MathF.Sin(heading), MathF.Cos(heading));
		Vector2 p = new Vector2(sp.X + vector.X * 16f, sp.Y + vector.Y * 16f);
		dl.AddLine(sp, p, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(col.W, 0.8f))), 4f);
		Vector2 p2 = sp;
		Vector4 input = col;
		input.W = MathF.Min(col.W, 0.95f);
		dl.AddLine(p2, p, ImGui.ColorConvertFloat4ToU32(input), 2f);
	}

	private static void DrawDot(ImDrawListPtr dl, Vector2 c, float r, Vector4 col)
	{
		DrawMarker(dl, c, 0, r, col, emphasize: false);
	}

	private static void DrawMarker(ImDrawListPtr dl, Vector2 c, int shape, float r, Vector4 col, bool emphasize)
	{
		float w = col.W;
		uint col2 = ImGui.ColorConvertFloat4ToU32(col);
		uint col3 = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(w, 0.85f)));
		uint col4 = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, MathF.Min(w, emphasize ? 0.95f : 0.6f)));
		float num = (emphasize ? 1.8f : 1f);
		float num2 = r + 1.6f;
		switch (shape)
		{
		case 1:
			Tri(dl, c, num2, col3, filled: true, 0f);
			Tri(dl, c, r, col2, filled: true, 0f);
			Tri(dl, c, r, col4, filled: false, num);
			break;
		case 2:
			dl.AddRectFilled(new Vector2(c.X - num2, c.Y - num2), new Vector2(c.X + num2, c.Y + num2), col3, 1.5f);
			dl.AddRectFilled(new Vector2(c.X - r, c.Y - r), new Vector2(c.X + r, c.Y + r), col2, 1.5f);
			dl.AddRect(new Vector2(c.X - r, c.Y - r), new Vector2(c.X + r, c.Y + r), col4, 1.5f, ImDrawFlags.None, num);
			break;
		case 3:
			DrawDiamond(dl, c, r, col2, filled: true);
			dl.AddLine(new Vector2(c.X, c.Y - r), new Vector2(c.X + r, c.Y), col4, num);
			dl.AddLine(new Vector2(c.X + r, c.Y), new Vector2(c.X, c.Y + r), col4, num);
			dl.AddLine(new Vector2(c.X, c.Y + r), new Vector2(c.X - r, c.Y), col4, num);
			dl.AddLine(new Vector2(c.X - r, c.Y), new Vector2(c.X, c.Y - r), col4, num);
			break;
		default:
			dl.AddCircleFilled(c, num2, col3, 20);
			dl.AddCircleFilled(c, r, col2, 20);
			dl.AddCircle(c, r, col4, 20, num);
			break;
		}
	}

	private static void Tri(ImDrawListPtr dl, Vector2 c, float r, uint col, bool filled, float thick)
	{
		Vector2 p = new Vector2(c.X, c.Y - r);
		Vector2 p2 = new Vector2(c.X + r * 0.92f, c.Y + r * 0.72f);
		Vector2 p3 = new Vector2(c.X - r * 0.92f, c.Y + r * 0.72f);
		if (filled)
		{
			dl.AddTriangleFilled(p, p2, p3, col);
		}
		else
		{
			dl.AddTriangle(p, p2, p3, col, thick);
		}
	}

	private static void DrawDiamond(ImDrawListPtr dl, Vector2 c, float r, uint col, bool filled)
	{
		Vector2 vector = new Vector2(c.X, c.Y - r);
		Vector2 vector2 = new Vector2(c.X + r, c.Y);
		Vector2 vector3 = new Vector2(c.X, c.Y + r);
		Vector2 vector4 = new Vector2(c.X - r, c.Y);
		if (filled)
		{
			uint col2 = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.85f));
			float num = r + 1.6f;
			dl.AddTriangleFilled(new Vector2(c.X, c.Y - num), new Vector2(c.X + num, c.Y), new Vector2(c.X, c.Y + num), col2);
			dl.AddTriangleFilled(new Vector2(c.X, c.Y - num), new Vector2(c.X, c.Y + num), new Vector2(c.X - num, c.Y), col2);
			dl.AddTriangleFilled(vector, vector2, vector3, col);
			dl.AddTriangleFilled(vector, vector3, vector4, col);
		}
		else
		{
			dl.AddLine(vector, vector2, col, 1.5f);
			dl.AddLine(vector2, vector3, col, 1.5f);
			dl.AddLine(vector3, vector4, col, 1.5f);
			dl.AddLine(vector4, vector, col, 1.5f);
		}
	}

	private void DrawIconAt(ImDrawListPtr dl, Vector2 center, float size, uint iconId)
	{
		if (iconId == 0)
		{
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
			float num = size * 0.5f;
			dl.AddImage(dalamudTextureWrap.Handle, new Vector2(center.X - num, center.Y - num), new Vector2(center.X + num, center.Y + num));
		}
	}

	private static MapCategory Classify(IGameObject o)
	{
		if (o.EntityId == Plugin.PlayerState.EntityId)
		{
			return MapCategory.You;
		}
		switch (o.ObjectKind)
		{
		case ObjectKind.Pc:
			return MapCategory.Party;
		case ObjectKind.BattleNpc:
			if (o is IBattleNpc battleNpc)
			{
				byte battleNpcKind = (byte)battleNpc.BattleNpcKind;
				if ((uint)(battleNpcKind - 2) <= 1u)
				{
					return MapCategory.Pet;
				}
				if (battleNpcKind == 9)
				{
					return MapCategory.Ally;
				}
			}
			return MapCategory.Enemy;
		case ObjectKind.Companion:
			return MapCategory.Pet;
		default:
			return MapCategory.Object;
		}
	}

	private static Vector4 CatColor(MapCategory cat)
	{
		return cat switch
		{
			MapCategory.You => ColYou, 
			MapCategory.Party => ColParty, 
			MapCategory.Enemy => ColEnemy, 
			MapCategory.Ally => ColAlly, 
			MapCategory.Pet => ColPet, 
			_ => ColObject, 
		};
	}

	private static Vector4 KindColor(ActorKind kind)
	{
		return kind switch
		{
			ActorKind.You => ColYou, 
			ActorKind.Party => ColParty, 
			ActorKind.Enemy => ColEnemy, 
			_ => ColObject, 
		};
	}

	private static (string, Vector4) KindTag(Replica.Logging.LogKind kind)
	{
		return kind switch
		{
			Replica.Logging.LogKind.CastStart => ("startcast", new Vector4(1f, 0.55f, 0.3f, 1f)), 
			Replica.Logging.LogKind.CastFinish => ("endcast", new Vector4(1f, 0.55f, 0.3f, 1f)), 
			Replica.Logging.LogKind.Ability => ("use", new Vector4(0.95f, 0.8f, 0.45f, 1f)), 
			Replica.Logging.LogKind.StatusGain => ("gain", new Vector4(0.55f, 0.85f, 1f, 1f)), 
			Replica.Logging.LogKind.StatusLose => ("lose", new Vector4(0.55f, 0.55f, 0.6f, 1f)), 
			Replica.Logging.LogKind.Death => ("death", ColEnemy), 
			Replica.Logging.LogKind.Headmarker => ("marker", new Vector4(0.85f, 0.55f, 1f, 1f)), 
			Replica.Logging.LogKind.Tether => ("tether", new Vector4(0.85f, 0.55f, 1f, 1f)), 
			Replica.Logging.LogKind.Added => ("add", ColEnemy), 
			Replica.Logging.LogKind.MapEffect => ("mapfx", new Vector4(0.45f, 0.9f, 0.8f, 1f)), 
			_ => (kind.ToString().ToLowerInvariant(), ColObject), 
		};
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

	private static string PullLabel(CombatLogCapture.PullInfo? p)
	{
		if (p != null)
		{
			return p.GetFullDisplayLabel();
		}
		return "(none)";
	}

	private void ExportReplayPull(int pullIndex)
	{
		try
		{
			CombatLogCapture.PullInfo? pull = FindPull(pullIndex);
			if (pull == null) return;

			string pluginConfigDirectory = Plugin.PluginInterface.GetPluginConfigDirectory();
			Directory.CreateDirectory(pluginConfigDirectory);

			string slug = pull.GetFileSlug();
			string fileName = $"replica-replay-{slug}-{DateTime.Now:yyyyMMdd-HHmmss}.json";
			string filePath = Path.Combine(pluginConfigDirectory, fileName);

			EnsurePullCache();

			var payload = new
			{
				pull = pull.Index,
				zone = pull.GetEffectiveZoneName(),
				boss = pull.GetEffectiveBossName(),
				duration = pull.Duration(),
				start = pull.Start.ToString("o"),
				end = pull.End.ToString("o"),
				territory = pull.Territory,
				mapId = pull.MapId,
				eventsCount = _pullEvents.Count,
				framesCount = _pullFrames.Count,
				events = _pullEvents,
				frames = _pullFrames
			};

			string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(filePath, json);

			Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + filePath + "\"")
			{
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			Plugin.Log?.Error("[Replica] Replay export failed: " + ex.Message);
		}
	}

	private static string FormatTime(float seconds)
	{
		if (seconds < 0f)
		{
			seconds = 0f;
		}
		int num = (int)(seconds / 60f);
		float value = seconds - (float)num * 60f;
		return $"{num}:{value:00.0}";
	}

	private static string Shorten(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			if (s.Length > 18)
			{
				return s.Substring(0, 17) + "…";
			}
			return s;
		}
		return "";
	}

	private static bool IdMatches(uint id, string search)
	{
		if (id == 0)
		{
			return false;
		}
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
		if (id.ToString("X8").Contains(text, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return id.ToString().Contains(text, StringComparison.Ordinal);
	}

	private void DrawMapAoes(ImDrawListPtr dl, Vector2 origin, float size, IReadOnlyList<MapAoe> aoes, bool hovered = false, bool clicked = false, Vector2 mouse = default, double pullT = 0, uint hoveredActorId = 0)
	{
		if (!Cfg.MapShowAoes || aoes == null || aoes.Count == 0)
		{
			return;
		}

		float scale = (size * 0.5f) / _viewRadius;
		float opacity = Math.Clamp(Cfg.MapAoeOpacity, 0.05f, 0.95f);
		(float wx, float wz) worldMouse = ToWorld(mouse, origin, size);

		MapAoe? mouseOverAoe = null;
		if (hovered)
		{
			for (int idx = aoes.Count - 1; idx >= 0; idx--)
			{
				if (aoes[idx].ContainsPoint(worldMouse.wx, worldMouse.wz))
				{
					mouseOverAoe = aoes[idx];
					break;
				}
			}
		}
		_hoveredAoe = mouseOverAoe;

		for (int idx = 0; idx < aoes.Count; idx++)
		{
			MapAoe aoe = aoes[idx];
			bool isSel = _selectedAoe.HasValue && IsSameAoe(aoe, _selectedAoe.Value);
			bool isHov = _hoveredAoe.HasValue && IsSameAoe(aoe, _hoveredAoe.Value);
			DrawSingleMapAoe(dl, origin, size, scale, aoe, opacity, isSel, isHov);
		}

		if (mouseOverAoe.HasValue)
		{
			if (clicked && hoveredActorId == 0)
			{
				_selectedAoe = mouseOverAoe.Value;
				_selectedId = 0u;
			}
			if (hoveredActorId == 0)
			{
				ShowAoeTooltip(mouseOverAoe.Value, pullT);
			}
		}
	}

	private void DrawSingleMapAoe(ImDrawListPtr dl, Vector2 origin, float size, float scale, MapAoe aoe, float baseOpacity, bool isSelected = false, bool isHovered = false)
	{
		Vector2 center = ToScreen(aoe.X, aoe.Z, origin, size);
		bool isSafe = aoe.IsSafe;

		Vector4 baseFill = isSafe
			? new Vector4(0.2f, 0.85f, 0.6f, baseOpacity)
			: new Vector4(0.96f, 0.45f, 0.2f, baseOpacity);

		Vector4 baseOutline = isSafe
			? new Vector4(0.3f, 0.95f, 0.75f, MathF.Min(1f, baseOpacity * 2.2f + 0.2f))
			: new Vector4(1f, 0.55f, 0.3f, MathF.Min(1f, baseOpacity * 2.2f + 0.2f));

		uint fillCol = ImGui.ColorConvertFloat4ToU32(baseFill);
		uint outlineCol = ImGui.ColorConvertFloat4ToU32(baseOutline);

		uint highlightCol = 0;
		float highlightThick = 0f;
		if (isSelected)
		{
			float pulse = (MathF.Sin((float)ImGui.GetTime() * 6f) + 1f) * 0.5f;
			Vector4 selColor = Vector4.Lerp(new Vector4(1f, 0.85f, 0.1f, 1f), new Vector4(0.1f, 0.95f, 1f, 1f), pulse);
			highlightCol = ImGui.ColorConvertFloat4ToU32(selColor);
			highlightThick = 3.2f;
		}
		else if (isHovered)
		{
			highlightCol = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.9f));
			highlightThick = 2.0f;
		}

		switch (aoe.Kind)
		{
		case MapAoeKind.Circle:
			{
				float r = MathF.Max(2f, aoe.Param1 * scale);
				dl.AddCircleFilled(center, r, fillCol, 36);
				dl.AddCircle(center, r, outlineCol, 36, 1.6f);
				if (highlightThick > 0f)
				{
					dl.AddCircle(center, r + 1f, highlightCol, 36, highlightThick);
				}
			}
			break;

		case MapAoeKind.SafeSpot:
			{
				float r = MathF.Max(4f, (aoe.Param1 > 0 ? aoe.Param1 : 2f) * scale);
				dl.AddCircleFilled(center, r, fillCol, 32);
				dl.AddCircle(center, r, outlineCol, 32, 2.2f);
				dl.AddCircle(center, r + 2f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.5f)), 32, 1f);
				if (highlightThick > 0f)
				{
					dl.AddCircle(center, r + 3f, highlightCol, 32, highlightThick);
				}
			}
			break;

		case MapAoeKind.Donut:
			{
				float rOuter = MathF.Max(3f, aoe.Param1 * scale);
				float rInner = MathF.Max(1f, aoe.Param2 * scale);
				float ha = aoe.Param3;

				if (ha <= 0.01f || ha >= MathF.PI - 0.01f)
				{
					// Full Donut
					dl.AddCircle(center, rOuter, outlineCol, 36, 1.6f);
					dl.AddCircle(center, rInner, outlineCol, 36, 1.4f);
					if (highlightThick > 0f)
					{
						dl.AddCircle(center, rOuter + 1f, highlightCol, 36, highlightThick);
						dl.AddCircle(center, rInner - 1f, highlightCol, 36, highlightThick * 0.8f);
					}

					int segments = 36;
					for (int i = 0; i < segments; i++)
					{
						float a1 = (float)(i * 2.0 * Math.PI / segments);
						float a2 = (float)((i + 1) * 2.0 * Math.PI / segments);
						Vector2 p1 = center + new Vector2(MathF.Sin(a1), MathF.Cos(a1)) * rInner;
						Vector2 p2 = center + new Vector2(MathF.Sin(a2), MathF.Cos(a2)) * rInner;
						Vector2 p3 = center + new Vector2(MathF.Sin(a2), MathF.Cos(a2)) * rOuter;
						Vector2 p4 = center + new Vector2(MathF.Sin(a1), MathF.Cos(a1)) * rOuter;
						dl.AddQuadFilled(p1, p2, p3, p4, fillCol);
					}
				}
				else
				{
					// Donut Sector
					float startAngle = aoe.Rot - ha;
					float endAngle = aoe.Rot + ha;
					int segments = Math.Clamp((int)(ha * 24), 8, 36);

					Span<Vector2> ptsInner = stackalloc Vector2[segments + 1];
					Span<Vector2> ptsOuter = stackalloc Vector2[segments + 1];

					for (int i = 0; i <= segments; i++)
					{
						float a = startAngle + (float)i / segments * (endAngle - startAngle);
						Vector2 dir = new Vector2(MathF.Sin(a), MathF.Cos(a));
						ptsInner[i] = center + dir * rInner;
						ptsOuter[i] = center + dir * rOuter;
					}

					for (int i = 0; i < segments; i++)
					{
						dl.AddQuadFilled(ptsInner[i], ptsInner[i + 1], ptsOuter[i + 1], ptsOuter[i], fillCol);
					}

					for (int i = 0; i < segments; i++)
					{
						dl.AddLine(ptsOuter[i], ptsOuter[i + 1], outlineCol, 1.6f);
						dl.AddLine(ptsInner[i], ptsInner[i + 1], outlineCol, 1.4f);
						if (highlightThick > 0f)
						{
							dl.AddLine(ptsOuter[i], ptsOuter[i + 1], highlightCol, highlightThick);
						}
					}
					dl.AddLine(ptsInner[0], ptsOuter[0], outlineCol, 1.5f);
					dl.AddLine(ptsInner[segments], ptsOuter[segments], outlineCol, 1.5f);
					if (highlightThick > 0f)
					{
						dl.AddLine(ptsInner[0], ptsOuter[0], highlightCol, highlightThick);
						dl.AddLine(ptsInner[segments], ptsOuter[segments], highlightCol, highlightThick);
					}
				}
			}
			break;

		case MapAoeKind.Cone:
			{
				float r = MathF.Max(2f, aoe.Param1 * scale);
				float ha = aoe.Param2;
				if (ha <= 0.01f) ha = 0.785f;

				float startAngle = aoe.Rot - ha;
				float endAngle = aoe.Rot + ha;
				int segments = Math.Clamp((int)(ha * 24), 8, 36);

				Span<Vector2> arcPts = stackalloc Vector2[segments + 2];
				arcPts[0] = center;

				for (int i = 0; i <= segments; i++)
				{
					float a = startAngle + (float)i / segments * (endAngle - startAngle);
					arcPts[i + 1] = center + new Vector2(MathF.Sin(a), MathF.Cos(a)) * r;
				}

				for (int i = 1; i <= segments; i++)
				{
					dl.AddTriangleFilled(center, arcPts[i], arcPts[i + 1], fillCol);
					dl.AddLine(arcPts[i], arcPts[i + 1], outlineCol, 1.6f);
					if (highlightThick > 0f)
					{
						dl.AddLine(arcPts[i], arcPts[i + 1], highlightCol, highlightThick);
					}
				}
				dl.AddLine(center, arcPts[1], outlineCol, 1.5f);
				dl.AddLine(center, arcPts[segments + 1], outlineCol, 1.5f);
				if (highlightThick > 0f)
				{
					dl.AddLine(center, arcPts[1], highlightCol, highlightThick);
					dl.AddLine(center, arcPts[segments + 1], highlightCol, highlightThick);
				}
			}
			break;

		case MapAoeKind.Rect:
			{
				float lf = aoe.Param1 * scale;
				float lb = aoe.Param2 * scale;
				float hw = MathF.Max(1.5f, aoe.Param3 * scale);
				float rot = aoe.Rot;

				Vector2 fwd = new Vector2(MathF.Sin(rot), MathF.Cos(rot));
				Vector2 right = new Vector2(MathF.Cos(rot), -MathF.Sin(rot));

				Vector2 c1 = center + fwd * lf + right * hw;
				Vector2 c2 = center + fwd * lf - right * hw;
				Vector2 c3 = center - fwd * lb - right * hw;
				Vector2 c4 = center - fwd * lb + right * hw;

				dl.AddQuadFilled(c1, c2, c3, c4, fillCol);
				dl.AddQuad(c1, c2, c3, c4, outlineCol, 1.6f);
				if (highlightThick > 0f)
				{
					dl.AddQuad(c1, c2, c3, c4, highlightCol, highlightThick);
				}
			}
			break;

		case MapAoeKind.Cross:
			{
				float len = aoe.Param1 * scale;
				float hw = MathF.Max(1.5f, aoe.Param2 * scale);
				float rot = aoe.Rot;

				for (int arm = 0; arm < 2; arm++)
				{
					float armRot = rot + (arm == 1 ? MathF.PI * 0.5f : 0f);
					Vector2 fwd = new Vector2(MathF.Sin(armRot), MathF.Cos(armRot));
					Vector2 right = new Vector2(MathF.Cos(armRot), -MathF.Sin(armRot));

					Vector2 c1 = center + fwd * len + right * hw;
					Vector2 c2 = center + fwd * len - right * hw;
					Vector2 c3 = center - fwd * len - right * hw;
					Vector2 c4 = center - fwd * len + right * hw;

					dl.AddQuadFilled(c1, c2, c3, c4, fillCol);
					dl.AddQuad(c1, c2, c3, c4, outlineCol, 1.4f);
					if (highlightThick > 0f)
					{
						dl.AddQuad(c1, c2, c3, c4, highlightCol, highlightThick);
					}
				}
			}
			break;

		case MapAoeKind.MovementArrow:
			{
				Vector2 target = ToScreen(aoe.Param2, aoe.Param3, origin, size);
				Vector2 diff = target - center;
				float dist = diff.Length();
				if (dist > 3f)
				{
					Vector2 dir = diff / dist;
					Vector2 normal = new Vector2(-dir.Y, dir.X);
					dl.AddLine(center, target, outlineCol, 2.5f);

					float arrowSize = MathF.Min(12f, dist * 0.35f);
					Vector2 arrowP1 = target - dir * arrowSize + normal * (arrowSize * 0.5f);
					Vector2 arrowP2 = target - dir * arrowSize - normal * (arrowSize * 0.5f);
					dl.AddTriangleFilled(target, arrowP1, arrowP2, outlineCol);

					if (highlightThick > 0f)
					{
						dl.AddLine(center, target, highlightCol, highlightThick + 1f);
						dl.AddTriangle(target, arrowP1, arrowP2, highlightCol, highlightThick);
					}
				}
			}
			break;
		}
	}

	private static bool IsSameAoe(MapAoe a, MapAoe b)
	{
		return a.Kind == b.Kind &&
		       MathF.Abs(a.X - b.X) < 0.15f &&
		       MathF.Abs(a.Z - b.Z) < 0.15f &&
		       MathF.Abs(a.Param1 - b.Param1) < 0.15f &&
		       MathF.Abs(a.Param2 - b.Param2) < 0.15f &&
		       a.ActionId == b.ActionId &&
		       a.SourceId == b.SourceId;
	}

	private void ShowAoeTooltip(MapAoe aoe, double pullT)
	{
		ImGui.BeginTooltip();
		uint actId = aoe.ActionId != 0 ? aoe.ActionId : InferredActionId(aoe, pullT);
		string spellName = ActionName(actId);

		if (aoe.IsSafe)
			ImGui.TextColored(new Vector4(0.2f, 0.95f, 0.6f, 1f), $"🛡 [Safe Zone] {aoe.GetShapeName()}");
		else
			ImGui.TextColored(new Vector4(1f, 0.55f, 0.2f, 1f), $"⚠ [AoE] {aoe.GetShapeName()}");

		if (!string.IsNullOrEmpty(spellName))
		{
			ImGui.TextColored(Ui.Gold, spellName);
		}
		if (actId != 0)
		{
			ImGui.TextDisabled($"Action ID: #{actId} (0x{actId:X})");
		}

		uint srcId = aoe.SourceId != 0 ? aoe.SourceId : InferredSourceId(aoe, pullT);
		if (srcId != 0)
		{
			string caster = _plugin.Capture.FrameActorName(srcId);
			if (string.IsNullOrEmpty(caster))
				caster = Plugin.ObjectTable.SearchById(srcId)?.Name.TextValue ?? "";
			if (!string.IsNullOrEmpty(caster))
				ImGui.TextColored(ColEnemy, $"Caster: {caster} (0x{srcId:X8})");
			else
				ImGui.TextDisabled($"Caster: 0x{srcId:X8}");
		}

		if (aoe.TargetId != 0)
		{
			string tgt = _plugin.Capture.FrameActorName(aoe.TargetId);
			if (!string.IsNullOrEmpty(tgt))
				ImGui.TextColored(ColParty, $"Target: {tgt} (0x{aoe.TargetId:X8})");
		}

		ImGui.TextDisabled(aoe.GetShapeDescription());
		ImGui.TextDisabled($"Pos: ({aoe.X:F1}, {aoe.Z:F1}) · Rot: {((aoe.Rot * 180f / MathF.PI + 360f) % 360f):F0}°");
		ImGui.Separator();
		ImGui.TextColored(new Vector4(0.3f, 0.9f, 1f, 1f), "💡 Left click to inspect, copy IDs & create script");
		ImGui.EndTooltip();
	}

	private void DrawAoeInspectorBody(MapAoe aoe)
	{
		uint actId = aoe.ActionId != 0 ? aoe.ActionId : InferredActionId(aoe, _playT);
		string spellName = ActionName(actId);
		uint srcId = aoe.SourceId != 0 ? aoe.SourceId : InferredSourceId(aoe, _playT);
		string casterName = srcId != 0 ? _plugin.Capture.FrameActorName(srcId) : "";
		if (string.IsNullOrEmpty(casterName) && srcId != 0)
			casterName = Plugin.ObjectTable.SearchById(srcId)?.Name.TextValue ?? "";

		ImGui.TextColored(Ui.Gold, "✦ AoE & Draw Analyzer");
		ImGui.SameLine();
		if (ImGui.SmallButton("✕ Close##aoecls"))
		{
			_selectedAoe = null;
			return;
		}
		ImGui.Separator();

		if (aoe.IsSafe)
			ImGui.TextColored(new Vector4(0.2f, 0.95f, 0.6f, 1f), "🛡 SAFE ZONE / SAFE SPOT");
		else
			ImGui.TextColored(new Vector4(1f, 0.45f, 0.2f, 1f), "⚠ DANGER / DAMAGE AOE");

		if (!string.IsNullOrEmpty(spellName))
		{
			ImGui.TextColored(Ui.Gold, spellName);
		}
		else
		{
			ImGui.TextColored(Ui.Gold, aoe.GetShapeName());
		}

		if (actId != 0)
		{
			ImGui.TextDisabled($"Action ID: #{actId} (0x{actId:X})");
			ImGui.SameLine();
			if (ImGui.SmallButton("📋 Copy##actid"))
			{
				ImGui.SetClipboardText(actId.ToString());
				SetAoeFeedback($"Copied Action ID #{actId}");
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("📋 Hex##acthex"))
			{
				ImGui.SetClipboardText($"0x{actId:X}");
				SetAoeFeedback($"Copied Hex 0x{actId:X}");
			}
		}
		else
		{
			ImGui.TextDisabled("Action ID: (Unknown / Generic Draw)");
		}

		if (srcId != 0)
		{
			string displayCaster = !string.IsNullOrEmpty(casterName) ? casterName : $"(Entity 0x{srcId:X8})";
			ImGui.TextColored(ColEnemy, $"Caster: {displayCaster}");
			ImGui.TextDisabled($"Entity ID: 0x{srcId:X8} (#{srcId})");
			ImGui.SameLine();
			if (ImGui.SmallButton("📋 Copy##cid"))
			{
				ImGui.SetClipboardText($"0x{srcId:X8}");
				SetAoeFeedback($"Copied Caster ID 0x{srcId:X8}");
			}
		}
		else
		{
			ImGui.TextDisabled("Caster: (Environment / Unknown)");
		}

		if (aoe.TargetId != 0)
		{
			string targetName = _plugin.Capture.FrameActorName(aoe.TargetId);
			if (string.IsNullOrEmpty(targetName))
				targetName = Plugin.ObjectTable.SearchById(aoe.TargetId)?.Name.TextValue ?? "";
			string displayTgt = !string.IsNullOrEmpty(targetName) ? targetName : $"0x{aoe.TargetId:X8}";
			ImGui.TextColored(ColParty, $"Target: {displayTgt}");
		}

		if (!string.IsNullOrEmpty(_aoeCopyFeedback) && (DateTime.Now - _aoeCopyTime).TotalSeconds < 3.0)
		{
			ImGui.TextColored(new Vector4(0.2f, 1f, 0.4f, 1f), $"✓ {_aoeCopyFeedback}");
		}

		ImGui.Separator();
		ImGui.TextColored(Ui.Gold, "📐 Shape & Geometry");
		ImGui.BulletText($"Type: {aoe.GetShapeName()}");
		ImGui.BulletText(aoe.GetShapeDescription());
		ImGui.BulletText($"Position: ({aoe.X:F2}, {aoe.Z:F2})");
		ImGui.BulletText($"Rotation: {aoe.Rot:F2} rad ({(aoe.Rot * 180f / MathF.PI + 360f) % 360f:F1}°)");

		if (_pullEvents.Count > 0 && _replayMode)
		{
			ImGui.Separator();
			ImGui.TextColored(Ui.Gold, $"⏱ Timeline Context (T ≈ {_playT:F1}s)");
			
			int eventCount = 0;
			foreach (var ev in _pullEvents)
			{
				double evT = Offset(ev);
				double diff = evT - _playT;
				if (diff >= -5.0 && diff <= 3.0)
				{
					string sign = diff >= 0 ? "+" : "";
					string evName = !string.IsNullOrEmpty(ev.Name) ? ev.Name : ev.Kind.ToString();
					ImGui.TextDisabled($"[{sign}{diff:F1}s]");
					ImGui.SameLine();
					(string tag, Vector4 col) = KindTag(ev.Kind);
					ImGui.TextColored(col, tag);
					ImGui.SameLine();
					ImGui.Text(Shorten(evName));
					if (++eventCount >= 4) break;
				}
			}
			if (eventCount == 0)
			{
				ImGui.TextDisabled("No combat events in ±4s window");
			}
		}

		ImGui.Separator();
		ImGui.TextColored(Ui.Gold, "⚡ Script & Automation Generator");

		if (ImGui.Button("📋 Copy C# Module Trigger", new Vector2(-1, 0)))
		{
			string csharpCode = GenerateCSharpModuleSnippet(aoe, spellName, actId);
			ImGui.SetClipboardText(csharpCode);
			SetAoeFeedback("Copied C# Module code to clipboard!");
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Copy ready-to-paste C# trigger and draw code for Replica combat modules.");
		}

		if (ImGui.Button("📋 Copy Script Bridge Trigger", new Vector2(-1, 0)))
		{
			string jsCode = GenerateJsBridgeSnippet(aoe, spellName, actId);
			ImGui.SetClipboardText(jsCode);
			SetAoeFeedback("Copied Script trigger to clipboard!");
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Copy JavaScript/Lua EventBridge trigger template.");
		}

		if (ImGui.Button("➕ Create QuickDraw from AoE", new Vector2(-1, 0)))
		{
			_plugin.OpenQuickDrawForMapAoe(aoe, spellName, _replayTerr != 0 ? _replayTerr : Plugin.ClientState.TerritoryType);
			SetAoeFeedback("Opened QuickDraw Editor!");
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Open QuickDraw Editor pre-configured with this shape, dimensions and action.");
		}
	}

	private void SetAoeFeedback(string msg)
	{
		_aoeCopyFeedback = msg;
		_aoeCopyTime = DateTime.Now;
	}

	private uint InferredActionId(MapAoe aoe, double pullT)
	{
		if (aoe.ActionId != 0) return aoe.ActionId;

		LogEvent? bestEvent = null;
		double minTimeDist = 999.0;
		float bestSpatialDist = 999.0f;

		foreach (var ev in _pullEvents)
		{
			if (!ev.IsCast && ev.Kind != Replica.Logging.LogKind.Ability && ev.Kind != Replica.Logging.LogKind.CastStart && ev.Kind != Replica.Logging.LogKind.CastFinish)
				continue;
			if (ev.DataId == 0) continue;

			double evT = Offset(ev);
			double timeDist = Math.Abs(evT - pullT);
			if (timeDist > 6.0) continue;

			float dx = ev.X - aoe.X;
			float dz = ev.Y - aoe.Z;
			float sDist = MathF.Sqrt(dx * dx + dz * dz);

			if (bestEvent == null || timeDist < minTimeDist || (timeDist < 2.0 && sDist < bestSpatialDist))
			{
				bestEvent = ev;
				minTimeDist = timeDist;
				bestSpatialDist = sDist;
			}
		}

		if (bestEvent != null)
			return bestEvent.DataId;

		if (!_replayMode)
		{
			foreach (var act in _liveActors)
			{
				if (act.Casting && act.CastId != 0)
				{
					float dx = act.Pos.X - aoe.X;
					float dz = act.Pos.Z - aoe.Z;
					if (dx * dx + dz * dz < 25f * 25f)
						return act.CastId;
				}
			}
		}

		return 0;
	}

	private uint InferredSourceId(MapAoe aoe, double pullT)
	{
		if (aoe.SourceId != 0) return aoe.SourceId;

		foreach (var ev in _pullEvents)
		{
			if (ev.SourceId == 0) continue;
			double evT = Offset(ev);
			if (Math.Abs(evT - pullT) <= 4.0)
			{
				float dx = ev.X - aoe.X;
				float dz = ev.Y - aoe.Z;
				if (dx * dx + dz * dz < 25f * 25f)
					return ev.SourceId;
			}
		}

		if (!_replayMode)
		{
			foreach (var act in _liveActors)
			{
				if (act.Casting)
				{
					float dx = act.Pos.X - aoe.X;
					float dz = act.Pos.Z - aoe.Z;
					if (dx * dx + dz * dz < 25f * 25f)
						return act.Id;
				}
			}
		}

		return 0;
	}

	private static string GenerateCSharpModuleSnippet(MapAoe aoe, string spellName, uint actionId)
	{
		string shapeCode = aoe.Kind switch
		{
			MapAoeKind.Circle => $"DrawCircle(caster.Position, {MathF.Max(1f, aoe.Param1):F1}f, ColorDanger);",
			MapAoeKind.Donut => $"DrawDonut(caster.Position, {MathF.Max(0f, aoe.Param2):F1}f, {MathF.Max(1f, aoe.Param1):F1}f, ColorDanger);",
			MapAoeKind.Cone => $"DrawFan(caster.Position, {MathF.Max(1f, aoe.Param1):F1}f, caster.Rotation, {(aoe.Param2 > 0.01f ? aoe.Param2 * 2f : 1.57f):F2}f, ColorDanger);",
			MapAoeKind.Rect => $"DrawRect(caster.Position, caster.Rotation, {MathF.Max(1f, aoe.Param3 * 2f):F1}f, {MathF.Max(1f, aoe.Param1 + aoe.Param2):F1}f, ColorDanger);",
			_ => $"DrawCircle(caster.Position, 5f, ColorDanger);"
		};

		return $@"// Generated by Replica LiveMap AoE Analyzer
// Spell: {(string.IsNullOrEmpty(spellName) ? "Action" : spellName)} (#{actionId})
public override HashSet<uint> ActionID => new() {{ {actionId} }};

public override void OnCastStart(IBattleChara caster, CastInfo info)
{{
    if (info.ActionId == {actionId})
    {{
        // Shape: {aoe.GetShapeName()} ({aoe.GetShapeDescription()})
        {shapeCode}
    }}
}}";
	}

	private static string GenerateJsBridgeSnippet(MapAoe aoe, string spellName, uint actionId)
	{
		return $@"// Generated by Replica LiveMap AoE Analyzer
// Trigger on spell {(string.IsNullOrEmpty(spellName) ? "Action" : spellName)} (#{actionId})
on.cast({actionId}, (event) => {{
    log(`Cast started: ${{event.actionName}} (${{event.actionId}}) from ${{event.sourceName}}`);
    // Shape: {aoe.GetShapeName()} ({aoe.GetShapeDescription()})
    // Position: (${aoe.X:F1}, ${aoe.Z:F1})
}});";
	}
}
