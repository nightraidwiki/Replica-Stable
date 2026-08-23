using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using Replica.Logging;

namespace Replica.Windows;

public sealed class MapCanvas
{
	public readonly struct Frame(Vector2 origin, float size, bool hovered, bool active, Vector2 mouse, ImDrawListPtr dl)
	{
		public Vector2 Origin { get; } = origin;

		public float Size { get; } = size;

		public bool Hovered { get; } = hovered;

		public bool Active { get; } = active;

		public Vector2 Mouse { get; } = mouse;

		public ImDrawListPtr Dl { get; } = dl;
	}

	private enum MapCategory : byte
	{
		You,
		Party,
		Enemy,
		Ally,
		Pet,
		Object
	}

	private static readonly Vector4 ColYou = new Vector4(0.35f, 0.85f, 1f, 1f);

	private static readonly Vector4 ColParty = new Vector4(0.4f, 0.85f, 0.5f, 1f);

	private static readonly Vector4 ColEnemy = new Vector4(0.96f, 0.42f, 0.42f, 1f);

	private static readonly Vector4 ColAlly = new Vector4(0.55f, 0.9f, 0.7f, 1f);

	private static readonly uint[] WaymarkIcons = new uint[8] { 61241u, 61242u, 61243u, 61247u, 61244u, 61245u, 61246u, 61248u };

	private readonly Plugin _plugin;

	private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new Dictionary<uint, ISharedImmediateTexture>();

	public float ViewRadius = 30f;

	public float MaxRadius = 200f;

	public float CenterX = 100f;

	public float CenterZ = 100f;

	public bool ShowGameMap;

	public bool ShowWaymarks = true;

	public bool ShowNames = true;

	public bool JobIcons = true;

	private uint _mapId = uint.MaxValue;

	private ISharedImmediateTexture? _mapTex;

	private float _mapScale = 1f;

	private float _mapOffX;

	private float _mapOffZ;

	public MapCanvas(Plugin plugin)
	{
		_plugin = plugin;
	}

	public Vector2 ToScreen(float wx, float wz, Vector2 origin, float size)
	{
		float num = size * 0.5f;
		float num2 = num / ViewRadius;
		return new Vector2(origin.X + num + (wx - CenterX) * num2, origin.Y + num + (wz - CenterZ) * num2);
	}

	public Vector2 ToWorld(Vector2 sp, Vector2 origin, float size)
	{
		float num = size * 0.5f;
		float num2 = ViewRadius / num;
		return new Vector2(CenterX + (sp.X - origin.X - num) * num2, CenterZ + (sp.Y - origin.Y - num) * num2);
	}

	public void RecenterOnPlayer()
	{
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (localPlayer != null)
		{
			CenterX = localPlayer.Position.X;
			CenterZ = localPlayer.Position.Z;
		}
		else
		{
			EnsureMapTexture(CurrentMapId());
			CenterX = 0f - _mapOffX;
			CenterZ = 0f - _mapOffZ;
		}
		ViewRadius = MathF.Min(30f, MaxRadius);
	}

	public Frame Begin(string id, float size)
	{
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton(id, new Vector2(size, size), ImGuiButtonFlags.MouseButtonMask);
		bool flag = ImGui.IsItemHovered();
		bool flag2 = ImGui.IsItemActive();
		Vector2 mousePos = ImGui.GetMousePos();
		if (flag)
		{
			float mouseWheel = ImGui.GetIO().MouseWheel;
			if (mouseWheel != 0f)
			{
				Vector2 vector = ToWorld(mousePos, cursorScreenPos, size);
				ViewRadius = Math.Clamp(ViewRadius - mouseWheel * ViewRadius * 0.12f, 5f, MaxRadius);
				float num = size * 0.5f;
				float num2 = ViewRadius / num;
				CenterX = vector.X - (mousePos.X - cursorScreenPos.X - num) * num2;
				CenterZ = vector.Y - (mousePos.Y - cursorScreenPos.Y - num) * num2;
			}
		}
		if (flag2 && ImGui.IsMouseDragging(ImGuiMouseButton.Right, 2f))
		{
			Vector2 mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Right, 2f);
			float num3 = ViewRadius / (size * 0.5f);
			CenterX -= mouseDragDelta.X * num3;
			CenterZ -= mouseDragDelta.Y * num3;
			ImGui.ResetMouseDragDelta(ImGuiMouseButton.Right);
		}
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 vector2 = cursorScreenPos;
		Vector2 vector3 = new Vector2(cursorScreenPos.X + size, cursorScreenPos.Y + size);
		windowDrawList.PushClipRect(vector2, vector3, intersectWithCurrentClipRect: true);
		windowDrawList.AddRectFilled(vector2, vector3, ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.11f, 1f)), 4f);
		DrawGrid(windowDrawList, cursorScreenPos, vector2, vector3, size);
		if (ShowGameMap)
		{
			DrawGameMap(windowDrawList, cursorScreenPos, size);
		}
		DrawCardinals(windowDrawList, vector2, vector3);
		if (ShowWaymarks)
		{
			DrawWaymarks(windowDrawList, cursorScreenPos, size);
		}
		return new Frame(cursorScreenPos, size, flag, flag2, mousePos, windowDrawList);
	}

	public void DrawArenaFloor(Frame f, byte shape, float radius, float cx, float cz)
	{
		if (!(radius < 0.5f))
		{
			ImDrawListPtr dl = f.Dl;
			uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0.62f, 0.5f, 0.34f, 0.22f));
			uint col2 = ImGui.ColorConvertFloat4ToU32(new Vector4(0.98f, 0.88f, 0.5f, 0.95f));
			uint col3 = ImGui.ColorConvertFloat4ToU32(new Vector4(0.85f, 0.2f, 0.2f, 0.55f));
			float num = f.Size * 0.5f / ViewRadius;
			Vector2 center = ToScreen(cx, cz, f.Origin, f.Size);
			if (shape == 0)
			{
				float radius2 = radius * num;
				dl.AddCircleFilled(center, radius2, col, 96);
				dl.AddCircle(center, radius2, col3, 96, 4f);
				dl.AddCircle(center, radius2, col2, 96, 2f);
			}
			else
			{
				Vector2 pMin = ToScreen(cx - radius, cz - radius, f.Origin, f.Size);
				Vector2 pMax = ToScreen(cx + radius, cz + radius, f.Origin, f.Size);
				dl.AddRectFilled(pMin, pMax, col, 2f);
				dl.AddRect(pMin, pMax, col3, 2f, ImDrawFlags.None, 4f);
				dl.AddRect(pMin, pMax, col2, 2f, ImDrawFlags.None, 2f);
			}
			dl.AddLine(new Vector2(center.X - 5f, center.Y), new Vector2(center.X + 5f, center.Y), col2, 1f);
			dl.AddLine(new Vector2(center.X, center.Y - 5f), new Vector2(center.X, center.Y + 5f), col2, 1f);
		}
	}

	public void End(Frame f)
	{
		Vector2 origin = f.Origin;
		Vector2 pMax = new Vector2(f.Origin.X + f.Size, f.Origin.Y + f.Size);
		f.Dl.PopClipRect();
		f.Dl.AddRect(origin, pMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.35f, 0.35f, 0.38f, 1f)), 4f);
	}

	public void DrawLiveActors(Frame f)
	{
		DrawLiveAoes(f);
		Vector2 origin = f.Origin;
		float size = f.Size;
		ImDrawListPtr dl = f.Dl;
		uint valueOrDefault = (Plugin.ObjectTable.LocalPlayer?.TargetObject?.EntityId).GetValueOrDefault();
		foreach (IGameObject item in Plugin.ObjectTable)
		{
			if (item == null)
			{
				continue;
			}
			MapCategory mapCategory;
			try
			{
				mapCategory = Classify(item);
			}
			catch
			{
				continue;
			}
			if (mapCategory == MapCategory.Object || mapCategory == MapCategory.Pet)
			{
				continue;
			}
			Vector2 vector = ToScreen(item.Position.X, item.Position.Z, origin, size);
			Vector4 vector2 = CatColor(mapCategory);
			bool flag = false;
			uint id = 0u;
			bool flag2 = false;
			if (item is IBattleChara battleChara)
			{
				if (battleChara.MaxHp != 0)
				{
					flag2 = battleChara.CurrentHp == 0;
				}
				if (battleChara.IsCasting)
				{
					flag = true;
					id = battleChara.CastActionId;
				}
			}
			if (flag2 && mapCategory == MapCategory.Enemy)
			{
				continue;
			}
			bool flag3 = mapCategory == MapCategory.You;
			bool flag4 = item.EntityId == valueOrDefault && mapCategory == MapCategory.Enemy;
			if (mapCategory == MapCategory.Enemy && item.HitboxRadius > 0.1f)
			{
				float num = size * 0.5f / ViewRadius;
				float radius = item.HitboxRadius * num;
				dl.AddCircleFilled(vector, radius, ImGui.ColorConvertFloat4ToU32(new Vector4(0.96f, 0.42f, 0.42f, flag4 ? 0.16f : 0.1f)), 40);
				dl.AddCircle(vector, radius, ImGui.ColorConvertFloat4ToU32(new Vector4(0.96f, 0.42f, 0.42f, flag4 ? 0.85f : 0.5f)), 40, flag4 ? 2f : 1.2f);
			}
			DrawFacing(dl, vector, item.Rotation, vector2);
			bool flag5 = mapCategory <= MapCategory.Party;
			if (flag5 && JobIcons && item is IPlayerCharacter { ClassJob: { RowId: not 0u } } playerCharacter)
			{
				float num2 = (flag3 ? 22f : 18f);
				dl.AddCircleFilled(vector, num2 * 0.5f + 2f, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.55f)), 16);
				DrawIconAt(dl, vector, num2, 62100 + playerCharacter.ClassJob.RowId);
				Vector2 center = vector;
				float radius2 = num2 * 0.5f + 2f;
				Vector4 input;
				if (!flag3)
				{
					Vector4 vector3 = vector2;
					vector3.W = 0.85f;
					input = vector3;
				}
				else
				{
					input = new Vector4(1f, 1f, 1f, 0.95f);
				}
				dl.AddCircle(center, radius2, ImGui.ColorConvertFloat4ToU32(input), 16, flag3 ? 1.8f : 1.2f);
			}
			else
			{
				float r = (flag4 ? 9f : (flag3 ? 8f : 6f));
				DrawDot(dl, vector, r, vector2, flag3 | flag4);
			}
			if (flag4)
			{
				dl.AddCircle(vector, 13f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.3f, 0.95f)), 20, 2f);
			}
			if (flag)
			{
				dl.AddCircle(vector, 11f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.55f, 0.3f, 0.95f)), 18, 1.8f);
				string text = ActionName(id);
				if (!string.IsNullOrEmpty(text))
				{
					dl.AddText(new Vector2(vector.X + 9f, vector.Y + 6f), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.65f, 0.4f, 1f)), text);
				}
			}
			if (ShowNames && !flag3 && !string.IsNullOrEmpty(item.Name.TextValue))
			{
				Vector2 pos = new Vector2(vector.X + 8f, vector.Y - 7f);
				Vector4 vector3 = vector2;
				vector3.W = 0.85f;
				dl.AddText(pos, ImGui.ColorConvertFloat4ToU32(vector3), Shorten(item.Name.TextValue));
			}
		}
		foreach (CombatLogCapture.LiveTether activeTether in _plugin.Capture.ActiveTethers)
		{
			IGameObject gameObject = Plugin.ObjectTable.SearchById(activeTether.From);
			IGameObject gameObject2 = Plugin.ObjectTable.SearchById(activeTether.To);
			if (gameObject != null && gameObject2 != null)
			{
				dl.AddLine(ToScreen(gameObject.Position.X, gameObject.Position.Z, origin, size), ToScreen(gameObject2.Position.X, gameObject2.Position.Z, origin, size), ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.45f, 1f, 0.7f)), 1.5f);
			}
		}
		foreach (CombatLogCapture.LiveHeadmarker activeHeadmarker in _plugin.Capture.ActiveHeadmarkers)
		{
			IGameObject gameObject3 = Plugin.ObjectTable.SearchById(activeHeadmarker.ActorId);
			if (gameObject3 != null)
			{
				Vector2 center2 = ToScreen(gameObject3.Position.X, gameObject3.Position.Z, origin, size);
				dl.AddCircle(center2, 8f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.2f, 0.9f)), 12, 1.6f);
			}
		}
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
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (localPlayer != null)
		{
			float num2 = num * 0.05f;
			if (MathF.Abs(localPlayer.Position.X + _mapOffX) > num + num2 || MathF.Abs(localPlayer.Position.Z + _mapOffZ) > num + num2)
			{
				return false;
			}
		}
		Vector2 pMin = ToScreen(0f - _mapOffX - num, 0f - _mapOffZ - num, origin, size);
		Vector2 pMax = ToScreen(0f - _mapOffX + num, 0f - _mapOffZ + num, origin, size);
		dl.AddImage(dalamudTextureWrap.Handle, pMin, pMax);
		return true;
	}

	private unsafe uint CurrentMapId()
	{
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
		MaxRadius = 200f;
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
				MaxRadius = Math.Clamp(num * 1.25f, 120f, 4000f);
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
		float num4 = num3 / ViewRadius;
		int num5 = (int)(ViewRadius / 5f) + 1;
		for (int i = -num5; i <= num5; i++)
		{
			float num6 = (float)i * 5f;
			if (!(MathF.Abs(num6) > ViewRadius + 0.01f))
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
					DrawIconAt(dl, center, 22f, WaymarkIcons[num]);
				}
				num++;
				continue;
			}
			break;
		}
	}

	private static void DrawFacing(ImDrawListPtr dl, Vector2 sp, float heading, Vector4 col)
	{
		Vector2 vector = new Vector2(MathF.Sin(heading), MathF.Cos(heading));
		Vector2 p = new Vector2(sp.X + vector.X * 16f, sp.Y + vector.Y * 16f);
		dl.AddLine(sp, p, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(col.W, 0.8f))), 4f);
		Vector2 p2 = sp;
		Vector4 input = col;
		input.W = MathF.Min(col.W, 0.95f);
		dl.AddLine(p2, p, ImGui.ColorConvertFloat4ToU32(input), 2f);
	}

	private static void DrawDot(ImDrawListPtr dl, Vector2 c, float r, Vector4 col, bool emphasize)
	{
		float w = col.W;
		uint col2 = ImGui.ColorConvertFloat4ToU32(col);
		uint col3 = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(w, 0.85f)));
		uint col4 = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, MathF.Min(w, emphasize ? 0.95f : 0.6f)));
		dl.AddCircleFilled(c, r + 1.6f, col3, 20);
		dl.AddCircleFilled(c, r, col2, 20);
		dl.AddCircle(c, r, col4, 20, emphasize ? 1.8f : 1f);
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
			_ => new Vector4(0.66f, 0.66f, 0.7f, 1f), 
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

	private static string Shorten(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			if (s.Length > 16)
			{
				return s.Substring(0, 15) + "…";
			}
			return s;
		}
		return "";
	}

	public void DrawLiveAoes(Frame f)
	{
		List<MapAoe>? liveAoes = _plugin.BossModBridge?.GetActiveMapAoes();
		if (liveAoes != null && liveAoes.Count > 0)
		{
			DrawAoes(f, liveAoes);
		}
	}

	public void DrawAoes(Frame f, IReadOnlyList<MapAoe> aoes)
	{
		if (!_plugin.Configuration.MapShowAoes || aoes == null || aoes.Count == 0)
		{
			return;
		}

		Vector2 origin = f.Origin;
		float size = f.Size;
		ImDrawListPtr dl = f.Dl;
		float scale = (size * 0.5f) / ViewRadius;
		float opacity = Math.Clamp(_plugin.Configuration.MapAoeOpacity, 0.05f, 0.95f);

		for (int idx = 0; idx < aoes.Count; idx++)
		{
			MapAoe aoe = aoes[idx];
			DrawSingleMapAoe(dl, origin, size, scale, aoe, opacity);
		}
	}

	private void DrawSingleMapAoe(ImDrawListPtr dl, Vector2 origin, float size, float scale, MapAoe aoe, float baseOpacity)
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

		switch (aoe.Kind)
		{
		case MapAoeKind.Circle:
			{
				float r = MathF.Max(2f, aoe.Param1 * scale);
				dl.AddCircleFilled(center, r, fillCol, 36);
				dl.AddCircle(center, r, outlineCol, 36, 1.6f);
			}
			break;

		case MapAoeKind.SafeSpot:
			{
				float r = MathF.Max(4f, (aoe.Param1 > 0 ? aoe.Param1 : 2f) * scale);
				dl.AddCircleFilled(center, r, fillCol, 32);
				dl.AddCircle(center, r, outlineCol, 32, 2.2f);
				dl.AddCircle(center, r + 2f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.5f)), 32, 1f);
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
					}
					dl.AddLine(ptsInner[0], ptsOuter[0], outlineCol, 1.5f);
					dl.AddLine(ptsInner[segments], ptsOuter[segments], outlineCol, 1.5f);
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
				}
				dl.AddLine(center, arcPts[1], outlineCol, 1.5f);
				dl.AddLine(center, arcPts[segments + 1], outlineCol, 1.5f);
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
				}
			}
			break;
		}
	}
}
