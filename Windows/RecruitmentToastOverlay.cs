using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace Replica.Windows;

public enum RecruitmentToastKind
{
    ApplicationAccepted,
    NewApplicationReceived,
    ApplicationSent,
    GeneralInfo
}

public sealed class RecruitmentToast
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public RecruitmentToastKind Kind { get; set; } = RecruitmentToastKind.GeneralInfo;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public FontAwesomeIcon Icon { get; set; } = FontAwesomeIcon.Bell;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Action? OnOpen { get; set; }
}

public sealed class RecruitmentToastOverlay : IDisposable
{
    private readonly Plugin _plugin;
    public static readonly List<RecruitmentToast> ActiveToasts = [];
    private static readonly object _lock = new();

    public RecruitmentToastOverlay(Plugin plugin)
    {
        _plugin = plugin;
    }

    public static void AddToast(RecruitmentToast toast)
    {
        lock (_lock)
        {
            if (!ActiveToasts.Any(t => t.Title == toast.Title && t.Message == toast.Message))
            {
                ActiveToasts.Add(toast);
            }
        }
    }

    public static void RemoveToast(string id)
    {
        lock (_lock)
        {
            ActiveToasts.RemoveAll(t => t.Id == id);
        }
    }

    public void DrawOverlay()
    {
        List<RecruitmentToast> toasts;
        lock (_lock)
        {
            if (ActiveToasts.Count == 0) return;
            toasts = [.. ActiveToasts];
        }

        var displaySize = ImGui.GetIO().DisplaySize;
        if (displaySize.X <= 100 || displaySize.Y <= 100) return;

        float scale = ImGuiHelpers.GlobalScale;
        float bannerWidth = 480f * scale;

        // Position: User saved position, or default to upper center of screen (away from hotbars)
        Vector2 savedPos = _plugin.Configuration.RecruitmentToastPosition;
        if (savedPos.X < 0 || savedPos.Y < 0)
        {
            savedPos = new Vector2((displaySize.X - bannerWidth) * 0.5f, 110f * scale);
        }

        ImGui.SetNextWindowPos(savedPos, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(bannerWidth, 80f * scale), new Vector2(bannerWidth + 40f * scale, 1200f * scale));

        ImGuiWindowFlags winFlags = ImGuiWindowFlags.NoTitleBar |
                                    ImGuiWindowFlags.AlwaysAutoResize |
                                    ImGuiWindowFlags.NoSavedSettings |
                                    ImGuiWindowFlags.NoFocusOnAppearing |
                                    ImGuiWindowFlags.NoBackground |
                                    ImGuiWindowFlags.NoCollapse;

        if (ImGui.Begin("##ReplicaRecruitmentToasterWindow", winFlags))
        {
            // Save position if user drags window
            if (ImGui.IsWindowHovered() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                Vector2 currentPos = ImGui.GetWindowPos();
                if (currentPos != _plugin.Configuration.RecruitmentToastPosition)
                {
                    _plugin.Configuration.RecruitmentToastPosition = currentPos;
                    _plugin.Configuration.Save();
                }
            }

            for (int i = 0; i < toasts.Count; i++)
            {
                DrawToastBanner(toasts[i], scale, bannerWidth);
                if (i < toasts.Count - 1)
                {
                    ImGui.Dummy(new Vector2(0f, 10f * scale));
                }
            }

            ImGui.End();
        }
    }

    private void DrawToastBanner(RecruitmentToast toast, float scale, float width)
    {
        float height = 90f * scale;
        Vector2 cursor = ImGui.GetCursorScreenPos();
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        float left = cursor.X;
        float right = cursor.X + width;
        float centerX = left + width * 0.5f;
        float top = cursor.Y;
        float bottom = cursor.Y + height;

        // Colors (ABGR format)
        uint centerBgCol;
        uint edgeBgCol;
        uint borderCol;
        uint borderEdge = 0x00000000u;
        uint tagColor;
        string tag;

        switch (toast.Kind)
        {
            case RecruitmentToastKind.ApplicationAccepted:
                centerBgCol = 0xF00D2A18u;
                edgeBgCol   = 0x200D2A18u;
                borderCol   = 0xFF4AE082u; // Emerald
                tagColor    = 0xFF6BF8A3u;
                tag         = "REPLICA PF • APPLICATION ACCEPTED";
                break;
            case RecruitmentToastKind.NewApplicationReceived:
                centerBgCol = 0xF0142338u;
                edgeBgCol   = 0x20142338u;
                borderCol   = 0xFF3AC4F5u; // Gold
                tagColor    = 0xFF65D4F5u;
                tag         = "REPLICA PF • NEW APPLICATION";
                break;
            case RecruitmentToastKind.ApplicationSent:
                centerBgCol = 0xF02A1C0Du;
                edgeBgCol   = 0x202A1C0Du;
                borderCol   = 0xFFF5B03Au; // Cyan/Blue
                tagColor    = 0xFFF8CB6Bu;
                tag         = "REPLICA PF • APPLICATION SENT";
                break;
            default:
                centerBgCol = 0xF0251825u;
                edgeBgCol   = 0x20251825u;
                borderCol   = 0xFFD070F5u;
                tagColor    = 0xFFE090F8u;
                tag         = "REPLICA PF • NOTIFICATION";
                break;
        }

        // 1. Background Gradient
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

        // 2. Top & Bottom Metallic Borders
        float borderThick = Math.Max(1.5f, 2.0f * scale);

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

        // Drag handle indicator on left
        drawList.AddText(cursor + new Vector2(10f * scale, 8f * scale), 0x88FFFFFF, "[Drag to move]");

        // 4. Texts
        Vector2 tagSize = ImGui.CalcTextSize(tag);
        Vector2 tagPos = new Vector2(centerX - tagSize.X * 0.5f, top + 6f * scale);
        drawList.AddText(tagPos + new Vector2(1, 1), 0xFF000000, tag);
        drawList.AddText(tagPos, tagColor, tag);

        // Main message text (centered with shadow)
        Vector2 textSize = ImGui.CalcTextSize(toast.Message);
        Vector2 textPos = new Vector2(centerX - textSize.X * 0.5f, top + 26f * scale);
        drawList.AddText(textPos + new Vector2(1.5f, 1.5f), 0xFF000000, toast.Message);
        drawList.AddText(textPos + new Vector2(-1f, 0f), 0xFF000000, toast.Message);
        drawList.AddText(textPos + new Vector2(1f, 0f), 0xFF000000, toast.Message);
        drawList.AddText(textPos + new Vector2(0f, 1f), 0xFF000000, toast.Message);
        drawList.AddText(textPos, 0xFFFFFFFF, toast.Message);

        // 5. Interactive Buttons
        float btnWidth = 140f * scale;
        float dismissWidth = 90f * scale;
        float btnHeight = 26f * scale;
        float btnsTotalW = btnWidth + dismissWidth + 12f * scale;

        ImGui.SetCursorScreenPos(cursor + new Vector2((width - btnsTotalW) * 0.5f, 52f * scale));

        Vector4 btnCol = toast.Kind switch
        {
            RecruitmentToastKind.ApplicationAccepted => new Vector4(0.15f, 0.6f, 0.35f, 0.9f),
            RecruitmentToastKind.NewApplicationReceived => new Vector4(0.85f, 0.55f, 0.1f, 0.9f),
            RecruitmentToastKind.ApplicationSent => new Vector4(0.18f, 0.45f, 0.85f, 0.9f),
            _ => new Vector4(0.5f, 0.3f, 0.7f, 0.9f)
        };

        ImGui.PushStyleColor(ImGuiCol.Button, btnCol);
        if (ImGui.Button($"Open PF###open_toast_{toast.Id}", new Vector2(btnWidth, btnHeight)))
        {
            toast.OnOpen?.Invoke();
            RemoveToast(toast.Id);
        }
        ImGui.PopStyleColor();

        ImGui.SameLine(0f, 12f * scale);

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.25f, 0.25f, 0.28f, 0.85f));
        if (ImGui.Button($"Dismiss###dism_{toast.Id}", new Vector2(dismissWidth, btnHeight)))
        {
            RemoveToast(toast.Id);
        }
        ImGui.PopStyleColor();

        ImGui.SetCursorScreenPos(cursor + new Vector2(0f, height));
    }

    public void Dispose()
    {
        lock (_lock)
        {
            ActiveToasts.Clear();
        }
    }
}
