using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Replica.Engine.Helper;
using Replica.QuickDraws;

namespace Replica.Windows;

public sealed class ArrowOverlay
{
	private const float Spread = 2.6179938f;

	private readonly Plugin _plugin;

	public ArrowOverlay(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		if (!_plugin.Configuration.QuickDrawsEnabled)
		{
			return;
		}
		ImDrawListPtr backgroundDrawList = ImGui.GetBackgroundDrawList();
		float globalScale = ImGuiHelpers.GlobalScale;
		foreach (QuickDrawEngine.ArrowGeo item in _plugin.Engine.ActiveArrows())
		{
			uint col = ImGui.ColorConvertFloat4ToU32(item.Color);
			float num = MathF.Max(1f, item.Thickness * globalScale);
			Vector3 vector = new Vector3(MathF.Sin(item.Angle), 0f, MathF.Cos(item.Angle));
			Vector3 vector2 = new Vector3(MathF.Sin(item.Angle + 2.6179938f), 0f, MathF.Cos(item.Angle + 2.6179938f));
			Vector3 vector3 = new Vector3(MathF.Sin(item.Angle - 2.6179938f), 0f, MathF.Cos(item.Angle - 2.6179938f));
			Vector3 vector4 = item.Origin + vector * item.Length;
			StrokeWorldLine(backgroundDrawList, item.Origin, vector4, num, col);
			if (item.Chevron)
			{
				float spacing = MathF.Max(0.5f, item.Spacing);
				int num2 = Math.Min(100, (int)(item.Length / spacing));
				for (int i = 1; i <= num2; i++)
				{
					Vector3 vector5 = item.Origin + vector * ((float)i * spacing);
					StrokeWorldLine(backgroundDrawList, vector5, vector5 + vector2 * spacing * 0.5f, num, col);
					StrokeWorldLine(backgroundDrawList, vector5, vector5 + vector3 * spacing * 0.5f, num, col);
				}
				if (PositionHelper.StableWorldToScreen(vector4, out var screen))
				{
					backgroundDrawList.AddCircleFilled(screen, MathF.Max(3f, num * 1.2f), col);
				}
			}
			else
			{
				StrokeWorldLine(backgroundDrawList, vector4, vector4 + vector2 * item.HeadSize, num, col);
				StrokeWorldLine(backgroundDrawList, vector4, vector4 + vector3 * item.HeadSize, num, col);
			}
		}
	}

	private static void StrokeWorldLine(ImDrawListPtr dl, Vector3 start, Vector3 end, float thickness, uint col)
	{
		float dist = Vector3.Distance(start, end);
		if (dist < 0.001f)
		{
			return;
		}
		int num = Math.Clamp((int)MathF.Ceiling(dist * 2f), 2, 100);
		Vector2? vector = null;
		for (int i = 0; i <= num; i++)
		{
			if (PositionHelper.StableWorldToScreen(Vector3.Lerp(start, end, (float)i / (float)num), out var screen))
			{
				if (vector.HasValue)
				{
					dl.AddLine(vector.Value, screen, col, thickness);
				}
				vector = screen;
			}
			else
			{
				vector = null;
			}
		}
	}
}
