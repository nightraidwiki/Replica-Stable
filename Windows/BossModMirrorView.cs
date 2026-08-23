using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Replica.Engine;

namespace Replica.Windows;

public sealed class BossModMirrorView
{
	private readonly Plugin _plugin;

	public BossModMirrorView(Plugin plugin)
	{
		_plugin = plugin;
	}

	private static void ToggleRow(string label, string desc, ref bool value, out bool changed)
	{
		changed = Ui.ToggleSwitch("##t_" + label, ref value);
		ImGui.SameLine(0f, 10f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(value ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, label);
		if (!string.IsNullOrEmpty(desc))
		{
			float indentW = 44f * ImGuiHelpers.GlobalScale;
			ImGui.Indent(indentW);
			ImGui.TextDisabled(desc);
			ImGui.Unindent(indentW);
		}
	}

	public void Draw()
	{
		Configuration configuration = _plugin.Configuration;

		ImGui.TextDisabled("BossMod Reborn Mirror (In-Game 3D) [ALPHA]");
		string status = _plugin.BossModBridge.GetStatusText();
		bool isBmActive = _plugin.BossModBridge.IsBossModActive();
		ImGui.TextColored(isBmActive ? Ui.Green : Ui.Dimmed, $"● {status}");
		ImGui.SameLine();
		ImGui.TextColored(Ui.Gold, "  (Alpha)");
		ImGui.Spacing();

		bool bmEnabled = configuration.BossModMirrorEnabled;
		ToggleRow("Enable BossMod mirror in-game [ALPHA]", "Automatically projects all BossMod radar telegraphs directly into the 3D game arena (Feature currently in ALPHA).", ref bmEnabled, out var bmChanged);
		if (bmChanged)
		{
			configuration.BossModMirrorEnabled = bmEnabled;
			configuration.Save();
		}

		bool modulesEnabled = configuration.ModulesEnabled;
		bool showWarning = bmEnabled && modulesEnabled;

		if (showWarning)
		{
			ImGui.Spacing();
			ImGui.PushStyleColor(ImGuiCol.Text, Ui.Gold);
			ImGui.PushFont(UiBuilder.IconFont);
			ImGui.Text(FontAwesomeIcon.ExclamationTriangle.ToIconString());
			ImGui.PopFont();
			ImGui.SameLine(0f, 4f);
			ImGui.Text("Warning: Both Replica's native drawings and BossMod Mirror are active!");
			ImGui.PopStyleColor();
			ImGui.TextDisabled("This may result in overlapping or duplicate telegraph drawings.");
		}

		string nativeDesc = showWarning
			? "Toggle this off to prevent duplicate drawings."
			: "Master switch for all Replica native telegraphs.";

		ToggleRow("Replica native mechanic drawing", nativeDesc, ref modulesEnabled, out var modulesChanged);
		if (modulesChanged)
		{
			configuration.ModulesEnabled = modulesEnabled;
			configuration.Save();
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		if (bmEnabled)
		{
			float indentW = 20f * ImGuiHelpers.GlobalScale;
			ImGui.Indent(indentW);

			bool bmAoes = configuration.BossModMirrorAOEs;
			ToggleRow("Draw BossMod AOEs", "Circles, cones, rectangles, donuts and crosses in 3D.", ref bmAoes, out var aoeChanged);
			if (aoeChanged)
			{
				configuration.BossModMirrorAOEs = bmAoes;
				configuration.Save();
			}

			bool bmSpreads = configuration.BossModMirrorSpreadsStacks;
			ToggleRow("Draw Spreads & Stacks", "Player-targeted spreads and stack indicators.", ref bmSpreads, out var spreadsChanged);
			if (spreadsChanged)
			{
				configuration.BossModMirrorSpreadsStacks = bmSpreads;
				configuration.Save();
			}

			bool bmArrows = configuration.BossModMirrorMovementArrows;
			ToggleRow("Draw Movement Arrows", "Safe spot navigation arrows projected on the ground.", ref bmArrows, out var arrowsChanged);
			if (arrowsChanged)
			{
				configuration.BossModMirrorMovementArrows = bmArrows;
				configuration.Save();
			}

			bool bmSafe = configuration.BossModMirrorSafeZones;
			ToggleRow("Draw Safe Zones & Safe Spots", "Safe spots, LoS safe zones, and green radar circles in 3D.", ref bmSafe, out var safeChanged);
			if (safeChanged)
			{
				configuration.BossModMirrorSafeZones = bmSafe;
				configuration.Save();
			}

			bool bmTethers = configuration.BossModMirrorTethers;
			ToggleRow("Draw Tethers & Chains 3D", "Player and boss tether beams with safe/danger color coding.", ref bmTethers, out var tethersChanged);
			if (tethersChanged)
			{
				configuration.BossModMirrorTethers = bmTethers;
				configuration.Save();
			}

			bool bmPartnerTethers = configuration.BossModMirrorPartnerTetherHelper;
			ToggleRow("Draw Partner Tether Helper", "Draws a 3D helper line between you and your designated mechanic partner.", ref bmPartnerTethers, out var partnerTethersChanged);
			if (partnerTethersChanged)
			{
				configuration.BossModMirrorPartnerTetherHelper = bmPartnerTethers;
				configuration.Save();
			}

			bool bmGaze = configuration.BossModMirrorGaze;
			ToggleRow("Draw Gaze & Look-Away Warnings", "3D eye warning markers over gaze and look-away mechanics.", ref bmGaze, out var gazeChanged);
			if (gazeChanged)
			{
				configuration.BossModMirrorGaze = bmGaze;
				configuration.Save();
			}

			bool bmSmartTowers = configuration.BossModMirrorSmartTowers;
			ToggleRow("Smart Towers & Role Masking", "Highlights your assigned tower in green and marks forbidden towers in danger red.", ref bmSmartTowers, out var towersChanged);
			if (towersChanged)
			{
				configuration.BossModMirrorSmartTowers = bmSmartTowers;
				configuration.Save();
			}

			bool bmExaflares = configuration.BossModMirrorExaflares;
			ToggleRow("Draw Exaflares & Trajectories", "Exaflare trajectory predictions and movement direction arrows.", ref bmExaflares, out var exaChanged);
			if (exaChanged)
			{
				configuration.BossModMirrorExaflares = bmExaflares;
				configuration.Save();
			}

			bool bmLineStacks = configuration.BossModMirrorLineStacks;
			ToggleRow("Draw Line Stacks", "Shared line cleave corridors with stack visual indicators.", ref bmLineStacks, out var lineStacksChanged);
			if (lineStacksChanged)
			{
				configuration.BossModMirrorLineStacks = bmLineStacks;
				configuration.Save();
			}

			bool bmReturnSpots = configuration.BossModMirrorReturnSpots;
			ToggleRow("Draw Return & Rewind Positions", "Mark 3D ground locations where the player will be teleported by time rewind.", ref bmReturnSpots, out var returnChanged);
			if (returnChanged)
			{
				configuration.BossModMirrorReturnSpots = bmReturnSpots;
				configuration.Save();
			}

			bool bmBanners = configuration.BossModMirrorHintsBanners;
			ToggleRow("Show Tactical Hints Banners (Blue/Red)", "Displays in-game FFXIV style banners for mechanics (Blue for raid strat/orders, Red for danger alerts).", ref bmBanners, out var bannersChanged);
			if (bannersChanged)
			{
				configuration.BossModMirrorHintsBanners = bmBanners;
				configuration.Save();
			}

			if (bmBanners)
			{
				ImGui.Indent(indentW);

				bool bmRiskOnly = configuration.BossModHintsRiskOnly;
				ToggleRow("Danger alerts only (Red)", "Only show critical danger warnings (GTFO, Taunt, Soak) and hide informational hints.", ref bmRiskOnly, out var riskChanged);
				if (riskChanged)
				{
					configuration.BossModHintsRiskOnly = bmRiskOnly;
					configuration.Save();
				}

				bool bmNativeToast = configuration.BossModHintsNativeToast;
				ToggleRow("Trigger in-game native toasts", "Triggers official FFXIV system toasts (Dalamud ToastGui) alongside the banner.", ref bmNativeToast, out var toastChanged);
				if (toastChanged)
				{
					configuration.BossModHintsNativeToast = bmNativeToast;
					configuration.Save();
				}

				float bannerScale = configuration.BossModBannerScale;
				ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
				if (ImGui.SliderFloat("Banner Scale", ref bannerScale, 0.6f, 2.0f, "%.2fx"))
				{
					configuration.BossModBannerScale = bannerScale;
					configuration.Save();
				}

				float bannerOffsetY = configuration.BossModBannerOffsetY;
				ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
				if (ImGui.SliderFloat("Banner Vertical Position", ref bannerOffsetY, 0.05f, 0.85f, "%.2f"))
				{
					configuration.BossModBannerOffsetY = bannerOffsetY;
					configuration.Save();
				}

				ImGui.Unindent(indentW);
			}

			ImGui.Unindent(indentW);
		}
	}
}
