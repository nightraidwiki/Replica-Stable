using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Replica.Engine.Helper;
using Replica.Logging;

namespace Replica.Engine.Bridge.BossMod.Overlay;

public sealed class BossModOverlayRenderer
{
    private readonly List<OverlayArrow> _cachedArrows = new(16);
    private readonly List<OverlaySafeSpot> _cachedSafeSpots = new(16);
    private readonly List<OverlayKnockback> _cachedKnockbacks = new(16);
    private readonly List<OverlayTether> _cachedTethers = new(16);
    private readonly List<OverlayGaze> _cachedGazes = new(16);
    private readonly List<OverlayReturnSpot> _cachedReturnSpots = new(16);
    private readonly List<OverlayBannerHint> _cachedBanners = new(8);

    public void CopyToMapAoes(List<MapAoe> output)
    {
        lock (_cachedArrows)
        {
            foreach (var a in _cachedArrows)
            {
                output.Add(new MapAoe(MapAoeKind.MovementArrow, true, a.Start.X, a.Start.Z, 0f, 0f, a.End.X, a.End.Z));
            }
        }
        lock (_cachedSafeSpots)
        {
            foreach (var s in _cachedSafeSpots)
            {
                output.Add(new MapAoe(MapAoeKind.SafeSpot, true, s.Center.X, s.Center.Z, 0f, s.Radius));
            }
        }
        lock (_cachedKnockbacks)
        {
            foreach (var k in _cachedKnockbacks)
            {
                output.Add(new MapAoe(MapAoeKind.MovementArrow, false, k.Start.X, k.Start.Z, 0f, 0f, k.End.X, k.End.Z));
            }
        }
    }

    public void ClearFrame()
    {
        lock (_cachedSafeSpots) { _cachedSafeSpots.Clear(); }
        lock (_cachedTethers) { _cachedTethers.Clear(); }
        lock (_cachedGazes) { _cachedGazes.Clear(); }
        lock (_cachedReturnSpots) { _cachedReturnSpots.Clear(); }
    }

    public void ClearAll()
    {
        lock (_cachedArrows) { _cachedArrows.Clear(); }
        lock (_cachedSafeSpots) { _cachedSafeSpots.Clear(); }
        lock (_cachedKnockbacks) { _cachedKnockbacks.Clear(); }
        lock (_cachedTethers) { _cachedTethers.Clear(); }
        lock (_cachedGazes) { _cachedGazes.Clear(); }
        lock (_cachedReturnSpots) { _cachedReturnSpots.Clear(); }
        lock (_cachedBanners) { _cachedBanners.Clear(); }
    }

    public void SetBanners(List<OverlayBannerHint> banners)
    {
        lock (_cachedBanners)
        {
            _cachedBanners.Clear();
            _cachedBanners.AddRange(banners);
        }
    }

    public void ClearBanners()
    {
        lock (_cachedBanners)
        {
            _cachedBanners.Clear();
        }
    }

    public void AddArrow(OverlayArrow arrow)
    {
        lock (_cachedArrows)
        {
            _cachedArrows.Add(arrow);
        }
    }

    public void SetArrows(List<OverlayArrow> arrows)
    {
        lock (_cachedArrows)
        {
            _cachedArrows.Clear();
            _cachedArrows.AddRange(arrows);
        }
    }

    public void ClearArrows()
    {
        lock (_cachedArrows)
        {
            _cachedArrows.Clear();
        }
    }

    public void AddSafeSpot(OverlaySafeSpot spot)
    {
        lock (_cachedSafeSpots)
        {
            _cachedSafeSpots.Add(spot);
        }
    }

    public void AddKnockback(OverlayKnockback knockback)
    {
        lock (_cachedKnockbacks)
        {
            _cachedKnockbacks.Add(knockback);
        }
    }

    public void ClearKnockbacks()
    {
        lock (_cachedKnockbacks)
        {
            _cachedKnockbacks.Clear();
        }
    }

    public void AddTether(OverlayTether tether)
    {
        lock (_cachedTethers)
        {
            _cachedTethers.Add(tether);
        }
    }

    public void AddGaze(OverlayGaze gaze)
    {
        lock (_cachedGazes)
        {
            _cachedGazes.Add(gaze);
        }
    }

    public void AddReturnSpot(OverlayReturnSpot spot)
    {
        lock (_cachedReturnSpots)
        {
            _cachedReturnSpots.Add(spot);
        }
    }

    public void Draw(Configuration configuration)
    {
        if (!configuration.BossModMirrorEnabled)
            return;

        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
        float scale = ImGuiHelpers.GlobalScale;
        float arrowThickness = MathF.Max(1.5f, configuration.BossModArrowThickness * scale);

        // 1. Safe spot movement arrows and ground rings
        if (configuration.BossModMirrorMovementArrows || configuration.BossModMirrorSafeZones)
        {
            if (configuration.BossModMirrorMovementArrows)
            {
                lock (_cachedArrows)
                {
                    var span = CollectionsMarshal.AsSpan(_cachedArrows);
                    for (int i = 0; i < span.Length; i++)
                    {
                        ref readonly var item = ref span[i];
                        Draw3DArrow(drawList, item.Start, item.End, arrowThickness, item.Color, true);
                    }
                }
            }

            if (configuration.BossModMirrorSafeZones)
            {
                lock (_cachedArrows)
                {
                    var span = CollectionsMarshal.AsSpan(_cachedArrows);
                    for (int i = 0; i < span.Length; i++)
                    {
                        ref readonly var item = ref span[i];
                        Draw3DSafeSpotGroundRing(drawList, item.End, 1.8f, item.Color);
                    }
                }

                lock (_cachedSafeSpots)
                {
                    var span = CollectionsMarshal.AsSpan(_cachedSafeSpots);
                    for (int i = 0; i < span.Length; i++)
                    {
                        ref readonly var item = ref span[i];
                        Draw3DSafeSpotGroundRing(drawList, item.Center, item.Radius, item.Color);
                    }
                }
            }
        }

        // 2. Knockback vectors
        lock (_cachedKnockbacks)
        {
            var span = CollectionsMarshal.AsSpan(_cachedKnockbacks);
            for (int i = 0; i < span.Length; i++)
            {
                ref readonly var item = ref span[i];
                Draw3DArrow(drawList, item.Start, item.End, arrowThickness * 1.2f, item.Color, false);
            }
        }

        // 3. Tethers 3D
        if (configuration.BossModMirrorTethers)
        {
            lock (_cachedTethers)
            {
                var span = CollectionsMarshal.AsSpan(_cachedTethers);
                for (int i = 0; i < span.Length; i++)
                {
                    ref readonly var item = ref span[i];
                    Draw3DTetherLine(drawList, item.Source, item.Target, item.Color, item.Thickness);
                }
            }
        }

        // 4. Gaze 3D
        if (configuration.BossModMirrorGaze)
        {
            lock (_cachedGazes)
            {
                var span = CollectionsMarshal.AsSpan(_cachedGazes);
                for (int i = 0; i < span.Length; i++)
                {
                    ref readonly var item = ref span[i];
                    Draw3DGazeWarning(drawList, item.Position, item.Color);
                }
            }
        }

		// 5. Return / Rewind Spots
		if (configuration.BossModMirrorReturnSpots)
		{
			lock (_cachedReturnSpots)
			{
				var span = CollectionsMarshal.AsSpan(_cachedReturnSpots);
				for (int i = 0; i < span.Length; i++)
				{
					ref readonly var item = ref span[i];
					Draw3DReturnSpot(drawList, item.Position, item.Label, item.Color);
				}
			}
		}

		// 6. FFXIV In-Game Style Hints Banners (Blue Info / Red Danger)
		if (configuration.BossModMirrorHintsBanners)
		{
			DrawInGameBanners(drawList, configuration);
		}
	}

	public void DrawInGameBanners(ImDrawListPtr drawList, Configuration configuration)
	{
		lock (_cachedBanners)
		{
			if (_cachedBanners.Count == 0) return;

			var displaySize = ImGui.GetIO().DisplaySize;
			if (displaySize.X <= 100 || displaySize.Y <= 100) return;

			float scale = Math.Clamp(configuration.BossModBannerScale, 0.5f, 2.5f);
			float centerX = displaySize.X * 0.5f;
			float startY = displaySize.Y * Math.Clamp(configuration.BossModBannerOffsetY, 0.02f, 0.90f);

			float bannerHeight = 44f * scale;
			float spacing = 8f * scale;

			int maxBanners = Math.Min(_cachedBanners.Count, 3);
			for (int b = 0; b < maxBanners; b++)
			{
				var banner = _cachedBanners[b];
				float currentY = startY + b * (bannerHeight + spacing);

				DrawSingleInGameBanner(drawList, banner, centerX, currentY, bannerHeight, scale);
			}
		}
	}

	private static void DrawSingleInGameBanner(ImDrawListPtr drawList, OverlayBannerHint banner, float centerX, float currentY, float bannerHeight, float scale)
	{
		bool isDanger = banner.Kind == OverlayBannerKind.DangerRed;

		// Calculate text size to dynamically adapt width
		string displayText = banner.Text;
		string tag = isDanger ? "● DANGER / WARNING" : "◆ TACTICS";
		
		Vector2 tagSize = ImGui.CalcTextSize(tag);
		Vector2 textSize = ImGui.CalcTextSize(displayText);

		float contentWidth = Math.Max(tagSize.X, textSize.X) + 60f * scale;
		float halfWidth = Math.Clamp(contentWidth * 0.5f + 140f * scale, 240f * scale, 580f * scale);

		float left = centerX - halfWidth;
		float right = centerX + halfWidth;
		float top = currentY;
		float bottom = currentY + bannerHeight;

		// Color Definitions (ABGR)
		// Blue banner: Dark midnight navy center, fade to transparent edges
		// Red banner: Deep blood crimson center, fade to transparent edges
		uint centerBgCol = isDanger ? 0xE0180B32u : 0xDD30180Cu; 
		uint edgeBgCol   = isDanger ? 0x00180B32u : 0x0030180Cu;

		uint borderCol   = isDanger ? 0xFF3535FFu : 0xFF3AC4F5u; // Red vs Gold
		uint borderEdge  = 0x00000000u;
		uint tagColor    = isDanger ? 0xFF4A4AFFu : 0xFF65D4F5u; // Bright red vs bright gold

		// 1. Background Gradient (Left Half: 0 -> Center, Right Half: Center -> 0)
		drawList.AddRectFilledMultiColor(
			new Vector2(left, top),
			new Vector2(centerX, bottom),
			edgeBgCol, centerBgCol, centerBgCol, edgeBgCol
		);

		drawList.AddRectFilledMultiColor(
			new Vector2(centerX, top),
			new Vector2(right, bottom),
			centerBgCol, edgeBgCol, edgeBgCol, centerBgCol
		);

		// 2. Top & Bottom Metallic Borders with smooth horizontal fade
		float borderThick = Math.Max(1.5f, 2.0f * scale);
		
		// Top border
		drawList.AddRectFilledMultiColor(
			new Vector2(left + 20f * scale, top),
			new Vector2(centerX, top + borderThick),
			borderEdge, borderCol, borderCol, borderEdge
		);
		drawList.AddRectFilledMultiColor(
			new Vector2(centerX, top),
			new Vector2(right - 20f * scale, top + borderThick),
			borderCol, borderEdge, borderEdge, borderCol
		);

		// Bottom border
		drawList.AddRectFilledMultiColor(
			new Vector2(left + 20f * scale, bottom - borderThick),
			new Vector2(centerX, bottom),
			borderEdge, borderCol, borderCol, borderEdge
		);
		drawList.AddRectFilledMultiColor(
			new Vector2(centerX, bottom - borderThick),
			new Vector2(right - 20f * scale, bottom),
			borderCol, borderEdge, borderEdge, borderCol
		);

		// 3. Center Diamond Ornament
		float diamondSize = 4.5f * scale;
		Vector2 topDiamond = new Vector2(centerX, top);
		drawList.AddQuadFilled(
			topDiamond + new Vector2(0, -diamondSize),
			topDiamond + new Vector2(diamondSize, 0),
			topDiamond + new Vector2(0, diamondSize),
			topDiamond + new Vector2(-diamondSize, 0),
			borderCol
		);

		// 4. Texts (Tag + Main Message)
		float tagY = top + 4f * scale;
		float textY = tagY + tagSize.Y + 2f * scale;

		// Tag with dark shadow
		Vector2 tagPos = new Vector2(centerX - tagSize.X * 0.5f, tagY);
		drawList.AddText(tagPos + new Vector2(1, 1), 0xFF000000, tag);
		drawList.AddText(tagPos, tagColor, tag);

		// Main Hint Text with strong shadow
		Vector2 textPos = new Vector2(centerX - textSize.X * 0.5f, textY);
		drawList.AddText(textPos + new Vector2(1.5f, 1.5f), 0xFF000000, displayText);
		drawList.AddText(textPos + new Vector2(-1f, 0f), 0xFF000000, displayText);
		drawList.AddText(textPos + new Vector2(1f, 0f), 0xFF000000, displayText);
		drawList.AddText(textPos + new Vector2(0f, 1f), 0xFF000000, displayText);
		drawList.AddText(textPos, 0xFFFFFFFF, displayText);
	}

    public static void Draw3DSafeSpotGroundRing(ImDrawListPtr drawList, Vector3 centerWorld, float radiusWorld, uint color)
    {
        const int segments = 24;
        Span<Vector2> screenPoints = stackalloc Vector2[segments];
        int validCount = 0;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * MathF.PI * 2f / segments;
            Vector3 worldPt = centerWorld + new Vector3(MathF.Cos(angle) * radiusWorld, 0f, MathF.Sin(angle) * radiusWorld);
            if (PositionHelper.StableWorldToScreen(worldPt, out var screenPt))
            {
                screenPoints[validCount++] = screenPt;
            }
        }

        if (validCount >= 3)
        {
            uint fillColor = (color & 0x00FFFFFF) | 0x35000000;
            drawList.AddConvexPolyFilled(ref screenPoints[0], validCount, fillColor);
            drawList.AddPolyline(ref screenPoints[0], validCount, color, ImDrawFlags.Closed, 3.0f);
            drawList.AddPolyline(ref screenPoints[0], validCount, 0xE6FFFFFF, ImDrawFlags.Closed, 1.5f);
        }
        else if (PositionHelper.StableWorldToScreen(centerWorld, out var centerScreen))
        {
            drawList.AddCircleFilled(centerScreen, 14f, (color & 0x00FFFFFF) | 0x50000000);
            drawList.AddCircle(centerScreen, 14f, color, 0, 3.0f);
            drawList.AddCircle(centerScreen, 17f, 0xFFFFFFFF, 0, 1.5f);
        }
    }

    public static void Draw3DArrow(ImDrawListPtr drawList, Vector3 start, Vector3 end, float thickness, uint color, bool isSafeSpot)
    {
        Vector3 dir = end - start;
        float len = dir.Length();
        if (len < 0.2f) return;

        Vector3 normDir = Vector3.Normalize(dir);
        Vector3 side = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, normDir));

        StrokeWorldLine(drawList, start, end, thickness, color);

        float headLen = Math.Min(1.5f, len * 0.4f);
        float headWidth = headLen * 0.5f;
        Vector3 arrowBase = end - normDir * headLen;
        Vector3 leftWing = arrowBase + side * headWidth;
        Vector3 rightWing = arrowBase - side * headWidth;

        StrokeWorldLine(drawList, end, leftWing, thickness, color);
        StrokeWorldLine(drawList, end, rightWing, thickness, color);
        StrokeWorldLine(drawList, leftWing, rightWing, thickness * 0.8f, color);

        if (PositionHelper.StableWorldToScreen(end, out var screenPos))
        {
            if (isSafeSpot)
            {
                drawList.AddCircleFilled(screenPos, thickness * 2f, color);
                drawList.AddCircle(screenPos, thickness * 3f, 0xFFFFFFFF, 0, 1.5f);
            }
            else
            {
                drawList.AddCircleFilled(screenPos, thickness * 1.8f, color);
            }
        }
    }

    public static void StrokeWorldLine(ImDrawListPtr drawList, Vector3 start, Vector3 end, float thickness, uint color)
    {
        float dist = Vector3.Distance(start, end);
        int steps = Math.Clamp((int)(dist * 2f), 1, 32);

        Vector2 prevScreen = default;
        bool hasPrev = false;

        for (int i = 0; i <= steps; i++)
        {
            Vector3 worldPos = Vector3.Lerp(start, end, (float)i / steps);
            if (PositionHelper.StableWorldToScreen(worldPos, out var curScreen))
            {
                if (hasPrev)
                {
                    drawList.AddLine(prevScreen, curScreen, color, thickness);
                }
                prevScreen = curScreen;
                hasPrev = true;
            }
            else
            {
                hasPrev = false;
            }
        }
    }

    public static void Draw3DTetherLine(ImDrawListPtr drawList, Vector3 start, Vector3 end, uint color, float thickness)
    {
        if (PositionHelper.StableWorldToScreen(start, out var p1) &&
            PositionHelper.StableWorldToScreen(end, out var p2))
        {
            uint glowColor = (color & 0x00FFFFFF) | 0x40000000;
            drawList.AddLine(p1, p2, glowColor, thickness * 2.2f);
            drawList.AddLine(p1, p2, color, thickness);
            drawList.AddCircleFilled(p1, 5f, color);
            drawList.AddCircleFilled(p2, 5f, color);
        }
    }

    public static void Draw3DGazeWarning(ImDrawListPtr drawList, Vector3 worldPos, uint color)
    {
        if (PositionHelper.StableWorldToScreen(worldPos, out var screenPos))
        {
            float r = 18f;
            drawList.AddCircleFilled(screenPos, r, 0xC0000000);
            drawList.AddCircle(screenPos, r, color, 0, 2.5f);
            drawList.AddCircleFilled(screenPos, 6f, 0xFF0000FF);
            drawList.AddCircleFilled(screenPos, 2.5f, 0xFFFFFFFF);
        }
    }

    public static void Draw3DReturnSpot(ImDrawListPtr drawList, Vector3 centerWorld, string label, uint color)
    {
        if (PositionHelper.StableWorldToScreen(centerWorld, out var screenPos))
        {
            drawList.AddCircleFilled(screenPos, 16f, (color & 0x00FFFFFF) | 0x30000000);
            drawList.AddCircle(screenPos, 16f, color, 0, 2.5f);
            drawList.AddCircle(screenPos, 20f, 0xE6FFFFFF, 0, 1.2f);
            drawList.AddText(screenPos + new Vector2(-18f, -28f), color, label);
        }
    }
}
