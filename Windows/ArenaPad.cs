using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Replica.Logging;
using Replica.QuickDraws;

namespace Replica.Windows;

internal static class ArenaPad
{
	public enum Shape : byte
	{
		Circle,
		Square
	}

	public const float CenterX = 100f;

	public const float CenterZ = 100f;

	public const float HalfExtent = 30f;

	public const float RaidRadius = 20f;

	private static readonly string[] WaymarkLabels = new string[8] { "A", "B", "C", "D", "1", "2", "3", "4" };

	private static readonly Vector4[] WaymarkColors = new Vector4[8]
	{
		new Vector4(1f, 0.25f, 0.25f, 0.95f),
		new Vector4(0.25f, 0.85f, 0.35f, 0.95f),
		new Vector4(0.3f, 0.55f, 1f, 0.95f),
		new Vector4(1f, 0.85f, 0.2f, 0.95f),
		new Vector4(0.95f, 0.45f, 1f, 0.95f),
		new Vector4(0.35f, 0.95f, 0.95f, 0.95f),
		new Vector4(1f, 0.55f, 0.2f, 0.95f),
		new Vector4(0.75f, 0.75f, 0.8f, 0.95f)
	};

	public static Shape DetectShape(uint territoryId)
	{
		if (territoryId == 0)
		{
			return Shape.Square;
		}
		string text = ZoneLibrary.CategoryOf(territoryId);
		if (text.Contains("Raid", StringComparison.OrdinalIgnoreCase) || text.Contains("Trial", StringComparison.OrdinalIgnoreCase) || text.Contains("Ultimate", StringComparison.OrdinalIgnoreCase) || text.Contains("Chaotic", StringComparison.OrdinalIgnoreCase))
		{
			return Shape.Circle;
		}
		return Shape.Square;
	}

	public static void Draw(string id, Plugin plugin, Func<Vector3> get, Action<Vector3> set, float scale, bool snapGrid, Action<bool> setSnapGrid, Action onDirty)
	{
		ImGui.PushID(id);
		Vector3 vector = get();
		Vector2 data = new Vector2(vector.X, vector.Z);
		ImGui.SetNextItemWidth(200f * scale);
		if (ImGui.InputFloat2("X / Z", ref data))
		{
			set(new Vector3(data.X, 0f, data.Y));
			onDirty();
		}
		ImGui.SameLine();
		if (ImGui.SmallButton("Use my spot"))
		{
			IGameObject gameObject = Plugin.ObjectTable.SearchById(Plugin.PlayerState.EntityId);
			if (gameObject != null)
			{
				set(gameObject.Position);
				onDirty();
			}
		}
		bool v = snapGrid;
		if (ImGui.Checkbox("Snap 1y", ref v))
		{
			setSnapGrid(v);
		}
		ImGui.SameLine();
		if (ImGui.SmallButton("Pop out"))
		{
			ImU8String strId = new ImU8String(12, 1);
			strId.AppendLiteral("##arena_pop_");
			strId.AppendFormatted(id);
			ImGui.OpenPopup(strId);
		}
		Shape shape = DetectShape(Plugin.ClientState.TerritoryType);
		ImGui.TextColored(in Ui.Dimmed, (shape == Shape.Circle) ? $"circle arena  centre {100f:0},{100f:0}  r {20f:0}y" : $"square pad  centre {100f:0},{100f:0}  ±{30f:0}y");
		float size = 150f * scale;
		DrawInteractivePad(get, set, size, shape, plugin, snapGrid, onDirty);
		float num = 420f * scale;
		ImGui.SetNextWindowSize(new Vector2(num + 24f, num + 110f), ImGuiCond.FirstUseEver);
		ImU8String name = new ImU8String(12, 1);
		name.AppendLiteral("##arena_pop_");
		name.AppendFormatted(id);
		if (ImGui.BeginPopupModal(name, ImGuiWindowFlags.NoScrollbar))
		{
			ImGui.TextColored(in Ui.Dimmed, "Click to place — Esc or Close below");
			DrawInteractivePad(get, set, num, shape, plugin, snapGrid, onDirty);
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			if (ImGui.Button("Close", new Vector2(-1f, 36f * scale)))
			{
				ImGui.CloseCurrentPopup();
			}
			ImGui.EndPopup();
		}
		ImGui.PopID();
	}

	private static void DrawInteractivePad(Func<Vector3> get, Action<Vector3> set, float size, Shape shape, Plugin plugin, bool snapGrid, Action onDirty)
	{
		Vector2 origin = ImGui.GetCursorScreenPos();
		ImGui.InvisibleButton("##pad", new Vector2(size, size));
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 vector = origin;
		Vector2 vector2 = new Vector2(origin.X + size, origin.Y + size);
		Vector2 mid = new Vector2(origin.X + size * 0.5f, origin.Y + size * 0.5f);
		windowDrawList.AddRectFilled(vector, vector2, ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.11f, 1f)), 4f);
		DrawGrid(windowDrawList, vector, vector2, size);
		DrawArenaOutline(windowDrawList, vector, vector2, mid, size, shape);
		DrawOverlays(ToScreen, plugin);
		Vector2 center = ToScreen(get().X, get().Z);
		windowDrawList.AddCircleFilled(center, 5f, ImGui.ColorConvertFloat4ToU32(Ui.Accent));
		if (ImGui.IsItemActive() && ImGui.IsMouseDown(ImGuiMouseButton.Left))
		{
			Vector2 mousePos = ImGui.GetMousePos();
			float x = 70f + (mousePos.X - origin.X) / size * 60f;
			float x2 = 70f + (mousePos.Y - origin.Y) / size * 60f;
			if (snapGrid)
			{
				x = MathF.Round(x);
				x2 = MathF.Round(x2);
			}
			else
			{
				x = MathF.Round(x, 1);
				x2 = MathF.Round(x2, 1);
			}
			set(new Vector3(x, 0f, x2));
			onDirty();
		}
		Vector2 ToScreen(float wx, float wz)
		{
			return WorldToScreen(wx, wz, origin, size);
		}
	}

	private static void DrawGrid(ImDrawListPtr dl, Vector2 a, Vector2 b, float size)
	{
		uint num = ImGui.ColorConvertFloat4ToU32(new Vector4(0.22f, 0.22f, 0.24f, 0.55f));
		uint num2 = ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.33f, 0.85f));
		float num3 = 70f;
		float num4 = 130f;
		for (float num5 = num3; num5 <= num4 + 0.01f; num5++)
		{
			float num6 = (num5 - num3) / 60f;
			float x = a.X + num6 * size;
			bool flag = MathF.Abs(num5 - 100f) < 0.01f || MathF.Abs(MathF.Round(num5) % 5f) < 0.01f;
			dl.AddLine(new Vector2(x, a.Y), new Vector2(x, b.Y), flag ? num2 : num);
		}
		for (float num7 = num3; num7 <= num4 + 0.01f; num7++)
		{
			float num8 = (num7 - num3) / 60f;
			float y = a.Y + num8 * size;
			bool flag2 = MathF.Abs(num7 - 100f) < 0.01f || MathF.Abs(MathF.Round(num7) % 5f) < 0.01f;
			dl.AddLine(new Vector2(a.X, y), new Vector2(b.X, y), flag2 ? num2 : num);
		}
	}

	private static void DrawArenaOutline(ImDrawListPtr dl, Vector2 a, Vector2 b, Vector2 mid, float size, Shape shape)
	{
		uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0.35f, 0.35f, 0.38f, 1f));
		if (shape == Shape.Circle)
		{
			float radius = 1f / 3f * size;
			dl.AddCircle(mid, radius, col, 48, 1.5f);
		}
		else
		{
			dl.AddRect(a, b, col, 4f);
		}
	}

	private static void DrawOverlays(Func<float, float, Vector2> toScreen, Plugin plugin)
	{
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		IPlayerCharacter localPlayer = Plugin.ObjectTable.LocalPlayer;
		if (localPlayer != null)
		{
			Vector2 center = toScreen(localPlayer.Position.X, localPlayer.Position.Z);
			windowDrawList.AddCircleFilled(center, 4f, ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.85f, 1f, 0.95f)));
		}
		foreach (CombatLogCapture.LiveHeadmarker activeHeadmarker in plugin.Capture.ActiveHeadmarkers)
		{
			IGameObject gameObject = Plugin.ObjectTable.SearchById(activeHeadmarker.ActorId);
			if (gameObject != null)
			{
				Vector2 center2 = toScreen(gameObject.Position.X, gameObject.Position.Z);
				windowDrawList.AddCircle(center2, 6f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.2f, 0.9f)), 10, 1.5f);
				Vector2 pos = new Vector2(center2.X + 7f, center2.Y - 6f);
				uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.9f, 0.5f, 0.9f));
				ImU8String text = new ImU8String(0, 1);
				text.AppendFormatted(activeHeadmarker.IconId, "X");
				windowDrawList.AddText(pos, col, text);
			}
		}
		foreach (CombatLogCapture.LiveTether activeTether in plugin.Capture.ActiveTethers)
		{
			DrawTetherLine(windowDrawList, toScreen, activeTether.From, activeTether.To);
		}
		DrawLiveTethers(windowDrawList, toScreen);
		DrawFieldMarkers(windowDrawList, toScreen);
	}

	private unsafe static void DrawFieldMarkers(ImDrawListPtr dl, Func<float, float, Vector2> toScreen)
	{
		MarkingController* ptr = MarkingController.Instance();
		if (ptr == null)
		{
			return;
		}
		int num = 0;
		Span<FieldMarker> fieldMarkers = ptr->FieldMarkers;
		for (int i = 0; i < fieldMarkers.Length; i++)
		{
			ref FieldMarker reference = ref fieldMarkers[i];
			if (num < WaymarkLabels.Length)
			{
				if (reference.Active)
				{
					Vector2 vector = toScreen((float)reference.X / 1000f, (float)reference.Z / 1000f);
					uint col = ImGui.ColorConvertFloat4ToU32(WaymarkColors[num]);
					float num2 = 5f;
					dl.AddRectFilled(new Vector2(vector.X - num2, vector.Y - num2), new Vector2(vector.X + num2, vector.Y + num2), col, 1f);
					dl.AddRect(new Vector2(vector.X - num2, vector.Y - num2), new Vector2(vector.X + num2, vector.Y + num2), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.85f)), 1f, ImDrawFlags.None, 1f);
					string text = WaymarkLabels[num];
					Vector2 vector2 = ImGui.CalcTextSize(text);
					dl.AddText(new Vector2(vector.X - vector2.X * 0.5f, vector.Y - vector2.Y * 0.5f), ImGui.ColorConvertFloat4ToU32(new Vector4(0.05f, 0.05f, 0.05f, 1f)), text);
				}
				num++;
				continue;
			}
			break;
		}
	}

	private static void DrawTetherLine(ImDrawListPtr dl, Func<float, float, Vector2> toScreen, uint fromId, uint toId)
	{
		IGameObject gameObject = Plugin.ObjectTable.SearchById(fromId);
		IGameObject gameObject2 = Plugin.ObjectTable.SearchById(toId);
		if (gameObject != null && gameObject2 != null)
		{
			Vector2 p = toScreen(gameObject.Position.X, gameObject.Position.Z);
			Vector2 p2 = toScreen(gameObject2.Position.X, gameObject2.Position.Z);
			dl.AddLine(p, p2, ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.45f, 1f, 0.75f)), 1.5f);
		}
	}

	private unsafe static void DrawLiveTethers(ImDrawListPtr dl, Func<float, float, Vector2> toScreen)
	{
		HashSet<(uint, uint)> hashSet = new HashSet<(uint, uint)>();
		foreach (IGameObject item2 in Plugin.ObjectTable)
		{
			if (!(item2 is IBattleChara battleChara))
			{
				continue;
			}
			Character* address = (Character*)battleChara.Address;
			if (address == null)
			{
				continue;
			}
			VfxContainer.Tether tether = address->Vfx.Tethers[0];
			if (tether.Id == 0)
			{
				continue;
			}
			uint num = (uint)(ulong)tether.TargetId;
			if (num != 0 && num != 3758096384u)
			{
				(uint, uint) item = (battleChara.EntityId, num);
				if (hashSet.Add(item))
				{
					DrawTetherLine(dl, toScreen, battleChara.EntityId, num);
				}
			}
		}
	}

	private static Vector2 WorldToScreen(float wx, float wz, Vector2 origin, float size)
	{
		return new Vector2(origin.X + (wx - 70f) / 60f * size, origin.Y + (wz - 70f) / 60f * size);
	}

	public static Func<float, float, Vector2> DrawBackdrop(Vector2 origin, float size, uint territory, Plugin plugin)
	{
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 vector = origin;
		Vector2 vector2 = new Vector2(origin.X + size, origin.Y + size);
		Vector2 mid = new Vector2(origin.X + size * 0.5f, origin.Y + size * 0.5f);
		windowDrawList.AddRectFilled(vector, vector2, ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.11f, 1f)), 4f);
		DrawGrid(windowDrawList, vector, vector2, size);
		DrawArenaOutline(windowDrawList, vector, vector2, mid, size, DetectShape(territory));
		DrawOverlays(ToScreen, plugin);
		return ToScreen;
		Vector2 ToScreen(float wx, float wz)
		{
			return WorldToScreen(wx, wz, origin, size);
		}
	}

	public static Vector3 ScreenToWorld(Vector2 mouse, Vector2 origin, float size, bool snapGrid)
	{
		float x = 70f + (mouse.X - origin.X) / size * 60f;
		float x2 = 70f + (mouse.Y - origin.Y) / size * 60f;
		if (snapGrid)
		{
			x = MathF.Round(x);
			x2 = MathF.Round(x2);
		}
		else
		{
			x = MathF.Round(x, 1);
			x2 = MathF.Round(x2, 1);
		}
		return new Vector3(x, 0f, x2);
	}
}
