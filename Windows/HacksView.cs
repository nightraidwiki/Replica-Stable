using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Replica.Engine.Hacks;

namespace Replica.Windows;

public sealed class HacksView
{
	private enum HackCategory
	{
		Invulnerability,
		Battle,
		Movement // New movement hacks tab
	}

	private readonly Plugin _plugin;
	private string _passwordInput = "";
	private string? _passwordError;
	private HackCategory _selectedCategory = HackCategory.Invulnerability;

	private static readonly (HackCategory Cat, string Label, FontAwesomeIcon Icon)[] Categories =
	[
		(HackCategory.Invulnerability, "Invulnerability", FontAwesomeIcon.ShieldAlt),
		(HackCategory.Battle, "Battle Hacks", FontAwesomeIcon.Bolt),
		(HackCategory.Movement, "Movement", FontAwesomeIcon.Walking),
	];

	public HacksView(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		if (!_plugin.Configuration.HacksUnlocked)
		{
			DrawLockScreen();
		}
		else
		{
			DrawUnlockedContent();
		}
	}

	private void DrawLockScreen()
	{
		float availX = ImGui.GetContentRegionAvail().X;
		float cardWidth = MathF.Min(480f * ImGuiHelpers.GlobalScale, availX);

		ImGui.Dummy(new Vector2(0f, 30f * ImGuiHelpers.GlobalScale));
		Center(cardWidth, availX);

		ImGui.BeginGroup();

		// Card Background
		Vector2 pMin = ImGui.GetCursorScreenPos();
		Vector2 pMax = pMin + new Vector2(cardWidth, 230f * ImGuiHelpers.GlobalScale);

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		drawList.AddRectFilled(pMin, pMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.12f, 0.12f, 0.12f, 0.95f)), 10f);
		drawList.AddRect(pMin, pMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.843f, 0.247f, 0.29f, 0.45f)), 10f, ImDrawFlags.None, 1.5f);

		ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 18f * ImGuiHelpers.GlobalScale);

		// Lock Icon & Header
		ImGui.PushFont(UiBuilder.IconFont);
		CenterText(FontAwesomeIcon.Lock.ToIconString(), 2.0f, Ui.Accent, cardWidth);
		ImGui.PopFont();

		ImGui.Dummy(new Vector2(0f, 6f * ImGuiHelpers.GlobalScale));
		CenterText("Protected Access — Hacks Tab", 1.2f, Ui.White, cardWidth);
		CenterText("Please enter the password to unlock this section.", 0.95f, Ui.Dimmed, cardWidth);

		ImGui.Dummy(new Vector2(0f, 12f * ImGuiHelpers.GlobalScale));

		// Password input
		float inputWidth = cardWidth * 0.75f;
		Center(inputWidth, cardWidth);
		ImGui.SetNextItemWidth(inputWidth);

		bool enterPressed = ImGui.InputTextWithHint(
			"##hacks_pass_input",
			"Password...",
			ref _passwordInput,
			64,
			ImGuiInputTextFlags.Password | ImGuiInputTextFlags.EnterReturnsTrue
		);

		ImGui.Dummy(new Vector2(0f, 8f * ImGuiHelpers.GlobalScale));

		float buttonWidth = 160f * ImGuiHelpers.GlobalScale;
		Center(buttonWidth, cardWidth);

		bool clicked = ImGui.Button("Unlock", new Vector2(buttonWidth, 32f * ImGuiHelpers.GlobalScale));

		if (enterPressed || clicked)
		{
			string inputNormalized = _passwordInput.Trim().ToLowerInvariant();
			string inputHash = ComputeSha256Hash(inputNormalized);

			if (string.Equals(inputHash, "d174226fcc7a32c3e3bb1ab43e7fde035701d9682840345484d34d6ad667e16b", StringComparison.Ordinal))
			{
				_plugin.Configuration.HacksUnlocked = true;
				_plugin.Configuration.Save();
				_passwordError = null;
				_passwordInput = "";
				_plugin.UpdateAllHackHookStates();
			}
			else
			{
				_passwordError = "Incorrect password.";
			}
		}

		if (!string.IsNullOrEmpty(_passwordError))
		{
			ImGui.Dummy(new Vector2(0f, 4f * ImGuiHelpers.GlobalScale));
			CenterText(_passwordError, 0.9f, Ui.Red, cardWidth);
		}

		ImGui.EndGroup();
	}

	private void DrawUnlockedContent()
	{
		float availX = ImGui.GetContentRegionAvail().X;
		float scale = ImGuiHelpers.GlobalScale;

		// Top header bar with Re-lock button
		ImGui.BeginGroup();
		string currentTitle = _selectedCategory switch
		{
			HackCategory.Invulnerability => "Invulnerability Mode (God Mode)",
			HackCategory.Battle => "Battle Hacks (Combat & Spell Enhancements)",
			HackCategory.Movement => "Movement Hacks",
			_ => "Hacks Management"
		};
		FontAwesomeIcon currentIcon = _selectedCategory switch
		{
			HackCategory.Invulnerability => FontAwesomeIcon.ShieldAlt,
			HackCategory.Battle => FontAwesomeIcon.Bolt,
			HackCategory.Movement => FontAwesomeIcon.Walking,
			_ => FontAwesomeIcon.Cogs
		};
		Ui.SectionHeader(currentIcon, currentTitle);
		ImGui.EndGroup();

		ImGui.SameLine(availX - 135f * scale);
		if (Ui.IconButton(FontAwesomeIcon.Lock, "Lock", "lock_hacks", new Vector2(130f * scale, 26f * scale), scale))
		{
			if (_plugin.Invulnerability.IsEnabled)
			{
				_plugin.Invulnerability.Disable();
			}
			_plugin.Configuration.HacksUnlocked = false;
			_plugin.Configuration.Save();
			_plugin.UpdateAllHackHookStates();
			return;
		}

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		// Two-Pane Layout: Left Sidebar + Right Content Area
		float sidebarWidth = 200f * scale;

		DrawSidebar(sidebarWidth);
		ImGui.SameLine();

		string childId = _selectedCategory switch
		{
			HackCategory.Invulnerability => "##hack_content_invul",
			HackCategory.Battle => "##hack_content_battle",
			HackCategory.Movement => "##hack_content_movement",
			_ => "##hack_content_area"
		};

		if (ImGui.BeginChild(childId, new Vector2(0f, 0f), false))
		{
			switch (_selectedCategory)
			{
				case HackCategory.Invulnerability:
					DrawInvulnerabilityPane();
					break;
				case HackCategory.Battle:
					DrawBattlePane();
					break;
				case HackCategory.Movement:
					DrawMovementPane();
					break;
			}
			ImGui.EndChild();
		}
	}

	private void DrawSidebar(float sidebarWidth)
	{
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8f, 6f));
		if (!ImGui.BeginChild("##hacks_sidebar", new Vector2(sidebarWidth, 0f), true))
		{
			ImGui.EndChild();
			ImGui.PopStyleVar();
			return;
		}

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();

		for (int i = 0; i < Categories.Length; i++)
		{
			(HackCategory cat, string label, FontAwesomeIcon icon) = Categories[i];
			bool isSelected = _selectedCategory == cat;
			bool isActive = cat switch
			{
				HackCategory.Invulnerability => _plugin.Invulnerability.IsEnabled,
				HackCategory.Battle => _plugin.Slidecast.IsEnabled || _plugin.ExtendedRange.IsEnabled || _plugin.GapCloserRange.IsEnabled || _plugin.CastRecast.DecCastEnabled || _plugin.CastRecast.DecRecastEnabled || _plugin.CastRecast.MudraNoRecastEnabled,
				HackCategory.Movement => _plugin.Speed.IsEnabled || _plugin.LocalFlight.IsEnabled || _plugin.LocalFlight.ProhibitFlightRestrictions || _plugin.NoClip.IsEnabled,
				_ => false
			};

			Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
			float frameHeight = 36f * ImGuiHelpers.GlobalScale;
			float availX = ImGui.GetContentRegionAvail().X;

			ImU8String selectableId = new ImU8String(6, 1);
			selectableId.AppendLiteral("##hcat");
			selectableId.AppendFormatted(cat);

			if (ImGui.Selectable(selectableId, isSelected, ImGuiSelectableFlags.None, new Vector2(availX, frameHeight)))
			{
				_selectedCategory = cat;
			}

			// Active category left border indicator
			if (isSelected)
			{
				drawList.AddRectFilled(
					cursorScreenPos,
					new Vector2(cursorScreenPos.X + 3.5f, cursorScreenPos.Y + frameHeight),
					ImGui.ColorConvertFloat4ToU32(Ui.Accent),
					2f
				);
			}

			float centerY = cursorScreenPos.Y + (frameHeight - ImGui.GetTextLineHeight()) * 0.5f;

			// Icon
			Vector4 iconColor = isSelected ? Ui.Accent : (isActive ? Ui.Green : Ui.Dimmed);
			ImGui.PushFont(UiBuilder.IconFont);
			drawList.AddText(new Vector2(cursorScreenPos.X + 10f, centerY), ImGui.ColorConvertFloat4ToU32(iconColor), icon.ToIconString());
			ImGui.PopFont();

			// Label
			Vector4 labelColor = isSelected ? Ui.White : (isActive ? new Vector4(0.9f, 0.9f, 0.9f, 1f) : Ui.Dimmed);
			drawList.AddText(new Vector2(cursorScreenPos.X + 36f, centerY), ImGui.ColorConvertFloat4ToU32(labelColor), label);

			// Active status badge dot
			if (isActive)
			{
				float dotRadius = 4f * ImGuiHelpers.GlobalScale;
				Vector2 dotPos = new Vector2(cursorScreenPos.X + availX - 14f, cursorScreenPos.Y + frameHeight * 0.5f);
				drawList.AddCircleFilled(dotPos, dotRadius, ImGui.ColorConvertFloat4ToU32(Ui.Green));
			}
		}

		ImGui.EndChild();
		ImGui.PopStyleVar();
	}

	private static void Card(Action drawContent, Vector4? customBg = null, Vector4? customBorder = null)
	{
		float scale = ImGuiHelpers.GlobalScale;
		float padX = 12f * scale;
		float padY = 10f * scale;
		float availWidth = ImGui.GetContentRegionAvail().X;
		Vector2 startPos = ImGui.GetCursorScreenPos();

		ImDrawListPtr drawList = ImGui.GetWindowDrawList();
		drawList.ChannelsSplit(2);
		drawList.ChannelsSetCurrent(1);

		ImGui.SetCursorScreenPos(new Vector2(startPos.X + padX, startPos.Y + padY));

		ImGui.PushItemWidth(MathF.Max(1f, availWidth - padX * 2f));
		ImGui.BeginGroup();
		drawContent();
		ImGui.EndGroup();
		ImGui.PopItemWidth();

		Vector2 itemMax = ImGui.GetItemRectMax();
		Vector2 boxMin = startPos;
		Vector2 boxMax = new Vector2(startPos.X + availWidth, itemMax.Y + padY);

		drawList.ChannelsSetCurrent(0);
		Vector4 bg = customBg ?? new Vector4(0.12f, 0.12f, 0.12f, 0.45f);
		Vector4 border = customBorder ?? new Vector4(0.843f, 0.247f, 0.29f, 0.22f);
		drawList.AddRectFilled(boxMin, boxMax, ImGui.ColorConvertFloat4ToU32(bg), 7f);
		drawList.AddRect(boxMin, boxMax, ImGui.ColorConvertFloat4ToU32(border), 7f, ImDrawFlags.None, 1f);

		drawList.ChannelsMerge();

		ImGui.SetCursorScreenPos(new Vector2(startPos.X, boxMax.Y + 8f * scale));
	}

	private void DrawInvulnerabilityPane()
	{
		float scale = ImGuiHelpers.GlobalScale;

		// Warning Box
		Card(() =>
		{
			ImGui.PushFont(UiBuilder.IconFont);
			ImGui.TextColored(Ui.Gold, FontAwesomeIcon.ExclamationTriangle.ToIconString());
			ImGui.PopFont();
			ImGui.SameLine();
			ImGui.TextColored(Ui.Gold, "Warning & Safety Information:");
			ImGui.TextColored(Ui.Dimmed, "This mode triggers a transition state (DiveEnd) that neutralizes damage and direct collision detection.");
			ImGui.TextColored(Ui.Dimmed, "Invulnerability is automatically disabled on territory changes or plugin unload.");
		}, new Vector4(0.18f, 0.12f, 0.05f, 0.6f), new Vector4(1f, 0.76f, 0.24f, 0.5f));

		ImGui.Spacing();

		// Invulnerability Controls & Status Box
		bool isInvul = _plugin.Invulnerability.IsEnabled;
		bool isAvail = _plugin.Invulnerability.IsAvailable;
		IPlayerCharacter? player = Plugin.ObjectTable.LocalPlayer;

		Card(() =>
		{
			ImGui.TextDisabled("SYSTEM STATUS:");
			ImGui.SameLine();
			if (!isAvail)
			{
				ImGui.TextColored(Ui.Red, "● Unavailable (Memory signatures not found)");
			}
			else if (isInvul)
			{
				ImGui.TextColored(Ui.Green, "● ACTIVE — Character is invulnerable");
			}
			else
			{
				ImGui.TextColored(Ui.Dimmed, "○ INACTIVE — Normal state");
			}

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			// Player Info row
			if (player != null)
			{
				Vector3 pos = player.Position;
				ImGui.TextColored(Ui.Dimmed, "Character:");
				ImGui.SameLine();
				ImGui.TextColored(Ui.White, $"{player.Name} ({player.ClassJob.Value.Abbreviation})");

				float availW = ImGui.GetContentRegionAvail().X;
				float rightColW = 240f * scale;
				if (availW > rightColW + 150f)
				{
					ImGui.SameLine(availW - rightColW);
				}
				else
				{
					ImGui.Spacing();
				}
				ImGui.TextColored(Ui.Dimmed, "Position:");
				ImGui.SameLine();
				ImGui.TextColored(Ui.White, $"X: {pos.X:F2}  Y: {pos.Y:F2}  Z: {pos.Z:F2}");

				ImGui.TextColored(Ui.Dimmed, "Territory ID:");
				ImGui.SameLine();
				ImGui.TextColored(Ui.White, $"{Plugin.ClientState.TerritoryType}");
			}
			else
			{
				ImGui.TextColored(Ui.Dimmed, "No local player currently logged in.");
			}

			ImGui.Spacing();
			ImGui.Spacing();

			// Big Toggle Button
			float btnHeight = 44f * scale;
			float btnWidth = ImGui.GetContentRegionAvail().X;

			if (!isAvail)
			{
				ImGui.BeginDisabled();
				ImGui.Button("Invulnerability mode unavailable", new Vector2(btnWidth, btnHeight));
				ImGui.EndDisabled();
			}
			else if (isInvul)
			{
				ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.22f, 0.65f, 0.32f, 1f));
				ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.28f, 0.75f, 0.38f, 1f));
				ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.18f, 0.55f, 0.26f, 1f));

				if (ImGui.Button("🛡️ DISABLE INVULNERABILITY", new Vector2(btnWidth, btnHeight)))
				{
					_plugin.Invulnerability.Disable();
				}

				ImGui.PopStyleColor(3);
			}
			else
			{
				ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.75f, 0.2f, 0.24f, 1f));
				ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.85f, 0.26f, 0.3f, 1f));
				ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.65f, 0.15f, 0.18f, 1f));

				if (ImGui.Button("⚔️ ENABLE INVULNERABILITY", new Vector2(btnWidth, btnHeight)))
				{
					_plugin.Invulnerability.Enable();
				}

				ImGui.PopStyleColor(3);
			}
		});

		ImGui.Spacing();

		// Quick Commands & Macros
		Ui.SectionHeader(FontAwesomeIcon.Terminal, "Quick Commands & Macros");
		ImGui.Spacing();

		Card(() =>
		{
			ImGui.BulletText("/invuln  or  /invul");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Direct toggle command.");

			ImGui.BulletText("/rep god  or  /rep invul");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Replica toggle command.");

			ImGui.BulletText("/rep hacks");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Opens this configuration tab.");

			ImGui.Spacing();
			ImGui.TextColored(Ui.Accent, "💡 This feature is unrestricted and works in all zones (dungeons, raids, trials, open-world).");
		});
	}

	private void DrawBattlePane()
	{
		// Accordion 1: Perfect Slidecast
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool slidecastOpen = ImGui.CollapsingHeader("🪄 Perfect Slidecast & Cast Movement###accordion_slidecast", ImGuiTreeNodeFlags.DefaultOpen);
		ImGui.PopStyleVar();

		if (slidecastOpen)
		{
			ImGui.Spacing();
			DrawSlidecastSection();
			ImGui.Spacing();
		}

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		// Accordion 2: Extended Action Range
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool extendedRangeOpen = ImGui.CollapsingHeader("🎯 Extended Action Range###accordion_extended_range");
		ImGui.PopStyleVar();

		if (extendedRangeOpen)
		{
			ImGui.Spacing();
			DrawExtendedRangeSection();
			ImGui.Spacing();
		}

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		// Accordion 3: Disable Range Limits of Gap Closer Actions
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool gapCloserOpen = ImGui.CollapsingHeader("🏃 Disable Range Limits of Gap Closer Actions###accordion_gap_closer");
		ImGui.PopStyleVar();

		if (gapCloserOpen)
		{
			ImGui.Spacing();
			DrawGapCloserSection();
			ImGui.Spacing();
		}

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		// Accordion 4: Cast & Recast Reduction
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool castRecastOpen = ImGui.CollapsingHeader("⚡ Cast & Recast Time Reduction###accordion_cast_recast");
		ImGui.PopStyleVar();

		if (castRecastOpen)
		{
			ImGui.Spacing();
			DrawCastRecastSection();
			ImGui.Spacing();
		}

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		// Battle Commands footer
		Ui.SectionHeader(FontAwesomeIcon.Terminal, "Battle Commands & Macros");
		ImGui.Spacing();

		Card(() =>
		{
			ImGui.BulletText("/slidecast  or  /slide");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Perfect Slidecast ON / OFF.");

			ImGui.BulletText("/slidecast <0.0 - 1.0>");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Set slidecast threshold window (e.g. /slidecast 0.6).");

			ImGui.BulletText("/rep slide");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Replica slidecast toggle command.");

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			ImGui.BulletText("/extendedrange  or  /extrange");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Extended Action Range Mode.");

			ImGui.BulletText("/rep range");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Replica range extension toggle command.");

			ImGui.BulletText("/gapcloser");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Disable Gap Closer Limits.");

			ImGui.BulletText("/rep gap  or  /rep gapcloser");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Replica gap closer limits toggle command.");

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			ImGui.BulletText("/deccast <seconds>");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle / configure cast time reduction.");

			ImGui.BulletText("/decrecast <seconds>");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle / configure recast (GCD) reduction.");

			ImGui.BulletText("/mudra");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Ninja instant Mudras (0s recast).");
		});
	}

	private void DrawSlidecastSection()
	{
		float scale = ImGuiHelpers.GlobalScale;
		SlidecastService slidecast = _plugin.Slidecast;
		IPlayerCharacter? player = Plugin.ObjectTable.LocalPlayer;

		// Explanation Info Card
		Card(() =>
		{
			ImGui.PushFont(UiBuilder.IconFont);
			ImGui.TextColored(Ui.Blue, FontAwesomeIcon.InfoCircle.ToIconString());
			ImGui.PopFont();
			ImGui.SameLine();
			ImGui.TextColored(Ui.Blue, "Network Cast Packet Filter");
			ImGui.TextColored(Ui.Dimmed, "Intercepts and suppresses outgoing position packets when remaining cast time is below threshold.");
			ImGui.TextColored(Ui.Dimmed, "The server receives no premature movement delta, ensuring spells complete without interruption.");
			ImGui.TextColored(Ui.Dimmed, "Allows uninterrupted movement and positioning up to the configured time window.");
		}, new Vector4(0.08f, 0.15f, 0.22f, 0.6f), new Vector4(0.2f, 0.55f, 0.85f, 0.5f));

		ImGui.Spacing();

		// Live Telemetry & Cast Gauge Box
		Card(() =>
		{
			ImGui.TextDisabled("SYSTEM STATUS:");
			ImGui.SameLine();
			if (!slidecast.IsAvailable)
			{
				ImGui.TextColored(Ui.Red, "● Unavailable (Memory signatures not found)");
			}
			else if (slidecast.IsEnabled)
			{
				ImGui.TextColored(Ui.Green, "● ACTIVE — Slidecast listening");
			}
			else
			{
				ImGui.TextColored(Ui.Dimmed, "○ INACTIVE — Disabled");
			}

			float availW = ImGui.GetContentRegionAvail().X;
			float rightBlockW = 160f * scale;
			if (availW > rightBlockW + 100f)
			{
				ImGui.SameLine(availW - rightBlockW);
			}
			else
			{
				ImGui.Spacing();
			}
			ImGui.TextDisabled("PACKETS BLOCKED:");
			ImGui.SameLine();
			ImGui.TextColored(Ui.White, $"{slidecast.SuppressedPacketsCount}");
			ImGui.SameLine();
			if (ImGui.SmallButton("Reset##reset_suppressed"))
			{
				slidecast.ResetStats();
			}

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			// Real-time casting gauge
			if (player != null && player.IsCasting)
			{
				float total = MathF.Max(0.01f, player.TotalCastTime);
				float current = player.CurrentCastTime;
				float remaining = MathF.Max(0f, total - current);
				float progress = Math.Clamp(current / total, 0f, 1f);
				bool inSlideWindow = remaining <= slidecast.SlidecastWindow;

				ImGui.TextColored(Ui.Dimmed, "Current Spell:");
				ImGui.SameLine();
				ImGui.TextColored(Ui.Gold, $"Action ID {player.CastActionId}");

				float timeBlockW = 180f * scale;
				if (availW > timeBlockW + 120f)
				{
					ImGui.SameLine(availW - timeBlockW);
				}
				else
				{
					ImGui.Spacing();
				}
				ImGui.TextColored(Ui.Dimmed, "Remaining Time:");
				ImGui.SameLine();
				ImGui.TextColored(inSlideWindow ? Ui.Green : Ui.White, $"{remaining:F2}s / {total:F2}s");

				// Progress bar with color feedback
				Vector4 barColor = inSlideWindow ? Ui.Green : Ui.Accent;
				ImGui.PushStyleColor(ImGuiCol.PlotHistogram, barColor);
				string barOverlay = inSlideWindow ? $"⚡ SLIDECAST ACTIVE ({remaining:F2}s remaining)" : $"Casting... ({remaining:F2}s)";
				ImGui.ProgressBar(progress, new Vector2(-1f, 22f * scale), barOverlay);
				ImGui.PopStyleColor();
			}
			else
			{
				ImGui.TextColored(Ui.Dimmed, "Player Status:");
				ImGui.SameLine();
				ImGui.TextColored(Ui.White, player != null ? "Idle (no spell currently casting)" : "No local player logged in");
				ImGui.Spacing();
				ImGui.ProgressBar(0f, new Vector2(-1f, 22f * scale), "Waiting for spell cast...");
			}
		});

		ImGui.Spacing();

		// Slidecast Settings & Controls Card
		Card(() =>
		{
			Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Slidecast Settings");
			ImGui.Spacing();

			// Toggle Switch
			bool enabled = slidecast.IsEnabled;
			if (Ui.ToggleSwitch("##slidecast_toggle", ref enabled))
			{
				slidecast.IsEnabled = enabled;
			}
			ImGui.SameLine(0f, 10f);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(enabled ? Ui.White : Ui.Dimmed, enabled ? "Perfect Slidecast ENABLED" : "Perfect Slidecast DISABLED");

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			// Slider from 0.0s to 1.0s in steps of 0.1s
			float window = slidecast.SlidecastWindow;
			ImGui.TextColored(Ui.Dimmed, "Slidecast Window (Remaining time threshold before cast completes):");

			ImGui.SetNextItemWidth(-1f);
			if (ImGui.SliderFloat("##slidecast_slider", ref window, 0.0f, 1.0f, "%.1f s", ImGuiSliderFlags.AlwaysClamp))
			{
				slidecast.SlidecastWindow = window;
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("Sets the remaining cast time at which you can freely begin moving without interrupting your spell.\nAdjustable from 0.0s to 1.0s in 0.1s increments.");
			}

			ImGui.Spacing();

			// Quick Preset Buttons
			ImGui.TextDisabled("Quick Presets:");
			ImGui.SameLine();

			float[] presets = [0.2f, 0.4f, 0.5f, 0.7f, 1.0f];
			for (int p = 0; p < presets.Length; p++)
			{
				float presetVal = presets[p];
				string pLabel = presetVal == 0.5f ? "0.5s (Default)" : $"{presetVal:F1}s";
				bool isCurrent = MathF.Abs(slidecast.SlidecastWindow - presetVal) < 0.05f;

				if (isCurrent)
				{
					ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
				}

				if (ImGui.SmallButton(pLabel))
				{
					slidecast.SlidecastWindow = presetVal;
				}

				if (isCurrent)
				{
					ImGui.PopStyleColor();
				}

				if (p < presets.Length - 1)
				{
					ImGui.SameLine();
				}
			}
		});
	}

	private void DrawExtendedRangeSection()
	{
		ExtendedRangeService rangeService = _plugin.ExtendedRange;

		// Explanation Info Card
		Card(() =>
		{
			ImGui.PushFont(UiBuilder.IconFont);
			ImGui.TextColored(Ui.Blue, FontAwesomeIcon.InfoCircle.ToIconString());
			ImGui.PopFont();
			ImGui.SameLine();
			ImGui.TextColored(Ui.Blue, "Extended Action Range");
			ImGui.TextColored(Ui.Dimmed, "Extends the maximum cast range of all actions by a specified distance (melee and ranged).");
			ImGui.TextColored(Ui.Dimmed, "This is checked client-side and helps hitting targets slightly outside normal range.");
		}, new Vector4(0.08f, 0.15f, 0.22f, 0.6f), new Vector4(0.2f, 0.55f, 0.85f, 0.5f));

		ImGui.Spacing();

		// Controls
		Card(() =>
		{
			Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Settings");
			ImGui.Spacing();

			bool enabled = rangeService.IsEnabled;
			if (Ui.ToggleSwitch("##extended_range_toggle", ref enabled))
			{
				rangeService.IsEnabled = enabled;
			}
			ImGui.SameLine(0f, 10f);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(enabled ? Ui.White : Ui.Dimmed, enabled ? "Extended Action Range ENABLED" : "Extended Action Range DISABLED");

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			float distance = rangeService.ExtendedRange;
			ImGui.TextColored(Ui.Dimmed, "Extend Action Ranges by (meters):");
			ImGui.SetNextItemWidth(-1f);
			if (ImGui.SliderFloat("##extended_range_slider", ref distance, 0.0f, 2.0f, "%.1f m", ImGuiSliderFlags.AlwaysClamp))
			{
				rangeService.ExtendedRange = distance;
			}
			if (ImGui.IsItemHovered())
			{
				ImGui.SetTooltip("Sets the extra distance added to spell/action ranges. Configurable up to 2.0 meters.");
			}
		});
	}

	private void DrawGapCloserSection()
	{
		GapCloserRangeService gapCloserService = _plugin.GapCloserRange;

		// Explanation Info Card
		Card(() =>
		{
			ImGui.PushFont(UiBuilder.IconFont);
			ImGui.TextColored(Ui.Blue, FontAwesomeIcon.InfoCircle.ToIconString());
			ImGui.PopFont();
			ImGui.SameLine();
			ImGui.TextColored(Ui.Blue, "Disable Gap Closer Range Limits");
			ImGui.TextColored(Ui.Dimmed, "Bypasses distance and line-of-sight limits for target-bound dash abilities (e.g. Intervene, Corps-a-corps).");
			ImGui.TextColored(Ui.Dimmed, "Allows casting these abilities from any distance once you have target selected.");
		}, new Vector4(0.08f, 0.15f, 0.22f, 0.6f), new Vector4(0.2f, 0.55f, 0.85f, 0.5f));

		ImGui.Spacing();

		// Controls
		Card(() =>
		{
			Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Settings");
			ImGui.Spacing();

			bool enabled = gapCloserService.IsEnabled;
			if (Ui.ToggleSwitch("##gap_closer_toggle", ref enabled))
			{
				gapCloserService.IsEnabled = enabled;
			}
			ImGui.SameLine(0f, 10f);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(enabled ? Ui.White : Ui.Dimmed, enabled ? "Disable Gap Closer Limits ENABLED" : "Disable Gap Closer Limits DISABLED");
		});
	}

	private void DrawCastRecastSection()
	{
		// Explanation Info Card
		Card(() =>
		{
			ImGui.PushFont(UiBuilder.IconFont);
			ImGui.TextColored(Ui.Blue, FontAwesomeIcon.InfoCircle.ToIconString());
			ImGui.PopFont();
			ImGui.SameLine();
			ImGui.TextColored(Ui.Blue, "Cast & Recast (GCD) Reduction");
			ImGui.TextColored(Ui.Dimmed, "Hooks into the client action engine to reduce cast duration, reduce Global Cooldown recast time,");
			ImGui.TextColored(Ui.Dimmed, "and eliminate recast delay on Ninja Mudras (Ten, Chi, Jin).");
		}, new Vector4(0.08f, 0.15f, 0.22f, 0.6f), new Vector4(0.2f, 0.55f, 0.85f, 0.5f));

		ImGui.Spacing();

		// Controls Box
		Card(() =>
		{
			Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Cast Reduction Settings");
			ImGui.Spacing();

			// DecCast
			bool decCast = _plugin.CastRecast.DecCastEnabled;
			if (Ui.ToggleSwitch("##deccast_toggle", ref decCast))
			{
				_plugin.CastRecast.DecCastEnabled = decCast;
			}
			ImGui.SameLine(0f, 10f);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(decCast ? Ui.White : Ui.Dimmed, decCast ? "Cast Time Reduction ENABLED" : "Cast Time Reduction DISABLED");

			float castTime = _plugin.CastRecast.DecCastTime;
			ImGui.TextColored(Ui.Dimmed, "Cast Reduction Amount (seconds subtracted from base cast):");
			ImGui.SetNextItemWidth(-1f);
			if (ImGui.SliderFloat("##deccast_slider", ref castTime, 0.0f, 5.0f, "%.1f s", ImGuiSliderFlags.AlwaysClamp))
			{
				_plugin.CastRecast.DecCastTime = castTime;
			}

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			Ui.SectionHeader(FontAwesomeIcon.Clock, "Recast / GCD Reduction Settings");
			ImGui.Spacing();

			// DecRecast
			bool decRecast = _plugin.CastRecast.DecRecastEnabled;
			if (Ui.ToggleSwitch("##decrecast_toggle", ref decRecast))
			{
				_plugin.CastRecast.DecRecastEnabled = decRecast;
			}
			ImGui.SameLine(0f, 10f);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(decRecast ? Ui.White : Ui.Dimmed, decRecast ? "Recast (GCD) Reduction ENABLED" : "Recast (GCD) Reduction DISABLED");

			float recastTime = _plugin.CastRecast.DecRecastTime;
			ImGui.TextColored(Ui.Dimmed, "Recast Reduction Amount (seconds subtracted from GCD):");
			ImGui.SetNextItemWidth(-1f);
			if (ImGui.SliderFloat("##decrecast_slider", ref recastTime, 0.0f, 2.5f, "%.1f s", ImGuiSliderFlags.AlwaysClamp))
			{
				_plugin.CastRecast.DecRecastTime = recastTime;
			}

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			// Mudra Instant
			bool mudra = _plugin.CastRecast.MudraNoRecastEnabled;
			if (ImGui.Checkbox("Ninja Instant Mudras (Zero GCD on Ten / Chi / Jin)##mudra_norecast", ref mudra))
			{
				_plugin.CastRecast.MudraNoRecastEnabled = mudra;
			}
		});
	}

	private void DrawMovementPane()
	{
		float scale = ImGuiHelpers.GlobalScale;

		// ── Speed Hack ──────────────────────────────────────────────
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool speedOpen = ImGui.CollapsingHeader("🏎️ Speed Hack###accordion_speed", ImGuiTreeNodeFlags.DefaultOpen);
		ImGui.PopStyleVar();

		if (speedOpen)
		{
			ImGui.Spacing();

			Card(() =>
			{
				ImGui.PushFont(UiBuilder.IconFont);
				ImGui.TextColored(Ui.Blue, FontAwesomeIcon.InfoCircle.ToIconString());
				ImGui.PopFont();
				ImGui.SameLine();
				ImGui.TextColored(Ui.Blue, "Movement Speed Multiplier");
				ImGui.TextColored(Ui.Dimmed, "Multiplies the base movement speed by the configured value.");
			}, new Vector4(0.08f, 0.15f, 0.22f, 0.6f), new Vector4(0.2f, 0.55f, 0.85f, 0.5f));

			ImGui.Spacing();

			Card(() =>
			{
				Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Speed Settings");
				ImGui.Spacing();

				bool speedEnabled = _plugin.Speed.IsEnabled;
				if (Ui.ToggleSwitch("##speed_toggle", ref speedEnabled))
				{
					_plugin.Speed.IsEnabled = speedEnabled;
				}
				ImGui.SameLine(0f, 10f);
				ImGui.AlignTextToFramePadding();
				ImGui.TextColored(speedEnabled ? Ui.White : Ui.Dimmed, speedEnabled ? "Speed Hack ENABLED" : "Speed Hack DISABLED");

				ImGui.Spacing();
				ImGui.Separator();
				ImGui.Spacing();

				float speedVal = _plugin.Speed.SpeedValue;
				ImGui.TextColored(Ui.Dimmed, "Speed Multiplier (Additional speed in m/s, step: 0.1):");
				ImGui.SetNextItemWidth(-1f);
				if (ImGui.SliderFloat("##speed_slider", ref speedVal, 0.1f, 10.0f, "+%.1f", ImGuiSliderFlags.AlwaysClamp))
				{
					_plugin.Speed.SpeedValue = speedVal;
				}
			});

			ImGui.Spacing();
		}

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		// ── Max Acceleration ─────────────────────────────────────────
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool accelOpen = ImGui.CollapsingHeader("⚡ Max Acceleration###accordion_accel");
		ImGui.PopStyleVar();

		if (accelOpen)
		{
			ImGui.Spacing();
			Card(() =>
			{
				Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Settings");
				ImGui.Spacing();

				bool accelEnabled = _plugin.Speed.MaxAcceleration;
				if (Ui.ToggleSwitch("##accel_toggle", ref accelEnabled))
				{
					_plugin.Speed.MaxAcceleration = accelEnabled;
				}
				ImGui.SameLine(0f, 10f);
				ImGui.AlignTextToFramePadding();
				ImGui.TextColored(accelEnabled ? Ui.White : Ui.Dimmed, accelEnabled ? "Max Acceleration ENABLED" : "Max Acceleration DISABLED");
			});
			ImGui.Spacing();
		}

		// ── Local Flight Mode ─────────────────────────────────────────
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool flightOpen = ImGui.CollapsingHeader("✈️ Local Flight Mode###accordion_flight");
		ImGui.PopStyleVar();

		if (flightOpen)
		{
			ImGui.Spacing();

			Card(() =>
			{
				ImGui.PushFont(UiBuilder.IconFont);
				ImGui.TextColored(Ui.Blue, FontAwesomeIcon.InfoCircle.ToIconString());
				ImGui.PopFont();
				ImGui.SameLine();
				ImGui.TextColored(Ui.Blue, "Local Flight Mode & Restriction Removers");
				ImGui.TextColored(Ui.Dimmed, "Enables flying in zones where flying is usually restricted or locked.");
			}, new Vector4(0.08f, 0.15f, 0.22f, 0.6f), new Vector4(0.2f, 0.55f, 0.85f, 0.5f));

			ImGui.Spacing();

			Card(() =>
			{
				Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Flight Settings");
				ImGui.Spacing();

				// Toggle Local Flight Mode
				bool flightEnabled = _plugin.LocalFlight.IsEnabled;
				if (Ui.ToggleSwitch("##flight_toggle", ref flightEnabled))
				{
					_plugin.LocalFlight.IsEnabled = flightEnabled;
				}
				ImGui.SameLine(0f, 10f);
				ImGui.AlignTextToFramePadding();
				ImGui.TextColored(flightEnabled ? Ui.White : Ui.Dimmed, flightEnabled ? "Local Flight Mode ENABLED (Unconditional fly)" : "Local Flight Mode DISABLED");

				ImGui.Dummy(new Vector2(0f, 8f * scale));

				// Toggle Flight Restrictions
				bool restrictEnabled = _plugin.LocalFlight.ProhibitFlightRestrictions;
				if (Ui.ToggleSwitch("##restrict_toggle", ref restrictEnabled))
				{
					_plugin.LocalFlight.ProhibitFlightRestrictions = restrictEnabled;
				}
				ImGui.SameLine(0f, 10f);
				ImGui.AlignTextToFramePadding();
				ImGui.TextColored(restrictEnabled ? Ui.White : Ui.Dimmed, restrictEnabled ? "Remove Flight/Mount Restrictions ENABLED (Indoor/Aether Currents)" : "Remove Flight/Mount Restrictions DISABLED");
			});
			ImGui.Spacing();
		}

		// ── Noclip Mode ───────────────────────────────────────────────
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 7f));
		bool noclipOpen = ImGui.CollapsingHeader("👻 Noclip Mode###accordion_noclip");
		ImGui.PopStyleVar();

		if (noclipOpen)
		{
			ImGui.Spacing();

			Card(() =>
			{
				ImGui.PushFont(UiBuilder.IconFont);
				ImGui.TextColored(Ui.Blue, FontAwesomeIcon.InfoCircle.ToIconString());
				ImGui.PopFont();
				ImGui.SameLine();
				ImGui.TextColored(Ui.Blue, "Noclip Mode (DailyRoutine Port)");
				ImGui.TextColored(Ui.Dimmed, "Allows you to walk through walls and solid terrain by disabling collision checks.");
			}, new Vector4(0.08f, 0.15f, 0.22f, 0.6f), new Vector4(0.2f, 0.55f, 0.85f, 0.5f));

			ImGui.Spacing();

			Card(() =>
			{
				Ui.SectionHeader(FontAwesomeIcon.SlidersH, "Noclip Settings");
				ImGui.Spacing();

				// Toggle Noclip Mode
				bool noclipEnabled = _plugin.NoClip.IsEnabled;
				if (Ui.ToggleSwitch("##noclip_toggle", ref noclipEnabled))
				{
					_plugin.NoClip.IsEnabled = noclipEnabled;
				}
				ImGui.SameLine(0f, 10f);
				ImGui.AlignTextToFramePadding();
				ImGui.TextColored(noclipEnabled ? Ui.White : Ui.Dimmed, noclipEnabled ? "Noclip Mode ENABLED" : "Noclip Mode DISABLED");
			});
			ImGui.Spacing();
		}

		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();

		// Movement Commands footer
		Ui.SectionHeader(FontAwesomeIcon.Terminal, "Movement Commands & Macros");
		ImGui.Spacing();

		Card(() =>
		{
			ImGui.BulletText("/speed");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Speed Hack ON / OFF.");

			ImGui.BulletText("/rep flight");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Local Flight Mode ON / OFF.");

			ImGui.BulletText("/rep restrictions");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Flight & Mount Restrictions ON / OFF.");

			ImGui.BulletText("/noclip");
			ImGui.SameLine();
			ImGui.TextColored(Ui.Dimmed, "— Toggle Noclip Mode ON / OFF (or /rep noclip).");
		});
	}

	private static void Center(float itemWidth, float avail)
	{
		float num = (avail - itemWidth) * 0.5f;
		if (num > 0f)
		{
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + num);
		}
	}

	private static void CenterText(string text, float scale, Vector4 color, float avail)
	{
		ImGui.SetWindowFontScale(scale);
		Center(ImGui.CalcTextSize(text).X, avail);
		ImGui.TextColored(in color, text);
		ImGui.SetWindowFontScale(1f);
	}

	private static string ComputeSha256Hash(string rawData)
	{
		using (SHA256 sha256Hash = SHA256.Create())
		{
			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++)
			{
				builder.Append(bytes[i].ToString("x2"));
			}
			return builder.ToString();
		}
	}
}
