using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;

namespace Replica.Windows;

public sealed class LabelOverlay
{
	private readonly Plugin _plugin;

	private static readonly (float X, float Y)[] Dirs = new(float, float)[8]
	{
		(-1f, 0f),
		(1f, 0f),
		(0f, -1f),
		(0f, 1f),
		(-0.7f, -0.7f),
		(0.7f, -0.7f),
		(-0.7f, 0.7f),
		(0.7f, 0.7f)
	};

	public LabelOverlay(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		if (!_plugin.Configuration.QuickDrawsEnabled)
		{
			return;
		}
		IFontHandle labelFont = _plugin.LabelFont;
		using ((labelFont != null && labelFont.Available) ? labelFont.Push() : null)
		{
			ImDrawListPtr backgroundDrawList = ImGui.GetBackgroundDrawList();
			ImFontPtr font = ImGui.GetFont();
			float fontSize = ImGui.GetFontSize();
			float num = 24f * ImGuiHelpers.GlobalScale;
			uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.95f));
			_plugin.Engine.RefreshLabelScreens();
			foreach (var (vector, text, input, y) in _plugin.Engine.ActiveLabelScreens())
			{
				if (!string.IsNullOrEmpty(text))
				{
					float num2 = num * MathF.Max(0.1f, y);
					float num3 = num2 / fontSize;
					Vector2 vector2 = ImGui.CalcTextSize(text) * num3;
					Vector2 vector3 = new Vector2(vector.X - vector2.X * 0.5f, vector.Y - vector2.Y * 0.5f);
					uint col2 = ImGui.ColorConvertFloat4ToU32(input);
					float num4 = MathF.Max(1.5f, num2 * 0.06f);
					(float, float)[] dirs = Dirs;
					for (int i = 0; i < dirs.Length; i++)
					{
						var (num5, num6) = dirs[i];
						backgroundDrawList.AddText(font, num2, vector3 + new Vector2(num5 * num4, num6 * num4), col, text);
					}
					backgroundDrawList.AddText(font, num2, vector3, col2, text);
				}
			}
		}
	}
}
