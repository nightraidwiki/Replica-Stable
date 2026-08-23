using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Replica.Engine;
using Replica.Logging;

namespace Replica.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
	private readonly Plugin _plugin;

	private static readonly string[] CaptureNames = new string[4] { "Always", "In combat", "In a duty", "Disabled" };

	public ConfigWindow(Plugin plugin)
		: base("Replica Settings###ReplicaConfig", ImGuiWindowFlags.AlwaysAutoResize)
	{
		_plugin = plugin;
	}

	public void Dispose()
	{
	}

	private static void Sep(string label)
	{
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.TextDisabled(label);
	}

	public override void Draw()
	{
		DrawContent();
	}

	private void DrawHealth()
	{
		CombatLogCapture capture = _plugin.Capture;
		bool actionEffectInstalled = capture.ActionEffectInstalled;
		bool actorControlInstalled = capture.ActorControlInstalled;
		bool mapEffectInstalled = capture.MapEffectInstalled;
		if (actionEffectInstalled & actorControlInstalled & mapEffectInstalled)
		{
			ImGui.TextColored(in Ui.Green, "● Connected");
			return;
		}
		HookRow("Casts & statuses", actionEffectInstalled);
		HookRow("Headmarkers & tethers", actorControlInstalled);
		HookRow("Arena map effects", mapEffectInstalled);
		if (!actionEffectInstalled)
		{
			ImGui.TextColored(in Ui.Red, "Core detection is down — likely a game patch. Tell the dev which lines are red.");
		}
		else if (!actorControlInstalled || !mapEffectInstalled)
		{
			ImGui.TextColored(in Ui.Dimmed, "Some optional feeds are down; casts & statuses still work.");
		}
	}

	private static void HookRow(string label, bool ok)
	{
		ImGui.TextColored(ok ? Ui.Green : Ui.Red, ok ? "  ● " : "  ✕ ");
		ImGui.SameLine(0f, 0f);
		Vector4 col = (ok ? Ui.Dimmed : Ui.Red);
		ImU8String text = new ImU8String(2, 2);
		text.AppendFormatted(label);
		text.AppendLiteral(": ");
		text.AppendFormatted(ok ? "OK" : "NOT WORKING");
		ImGui.TextColored(in col, text);
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

	public void DrawContent()
	{
		Configuration configuration = _plugin.Configuration;
		FightModuleHost host = _plugin.Host;
		Sep("Drawing");
		bool value = configuration.ModulesEnabled;
		ToggleRow("Enable mechanic drawing", "Master switch for all fight telegraphs.", ref value, out var changed);
		if (changed)
		{
			configuration.ModulesEnabled = value;
			configuration.Save();
		}
		ImGui.Spacing();
		float v = configuration.CustomAlpha;
		ImGui.SetNextItemWidth(220f * ImGuiHelpers.GlobalScale);
		if (ImGui.SliderFloat("Omen opacity", ref v, 1f, 2f))
		{
			configuration.CustomAlpha = v;
			configuration.Save();
		}


		Sep("Window");
		bool value2 = configuration.OpenOnLogin;
		ToggleRow("Open on login", "Pop the main window automatically when you log in.", ref value2, out var changed2);
		if (changed2)
		{
			configuration.OpenOnLogin = value2;
			configuration.Save();
		}
		Sep("Capture");
		int currentItem = (int)configuration.CaptureWhen;
		ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
		if (ImGui.Combo("When to read combat", ref currentItem, CaptureNames, CaptureNames.Length))
		{
			configuration.CaptureWhen = (CaptureMode)currentItem;
			configuration.Save();
			_plugin.Capture.UpdateHookStates();
			if (configuration.CaptureWhen == CaptureMode.Disabled)
			{
				_plugin.Capture.TrimPulls();
				_plugin.Capture.SaveToDisk();
			}
		}
		ImGui.Spacing();
		int maxPulls = configuration.MaxPullsToKeep;
		ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
		if (ImGui.InputInt("Max pulls to keep", ref maxPulls, 1, 5))
		{
			if (maxPulls < 0)
			{
				maxPulls = 0;
			}
			configuration.MaxPullsToKeep = maxPulls;
			configuration.Save();
			_plugin.Capture.TrimPulls();
			_plugin.Capture.SaveToDisk();
		}
		ImGui.TextDisabled("Limits when the plugin processes casts and effects.");
		ImGui.Spacing();
		bool logOutside = configuration.LogOutsidePulls;
		ToggleRow("Log outside of pulls", "Capture casts and statuses even when not in combat (e.g. in towns). Note: This can increase memory usage.", ref logOutside, out var changedOutside);
		if (changedOutside)
		{
			configuration.LogOutsidePulls = logOutside;
			configuration.Save();
			if (!logOutside)
			{
				_plugin.Capture.TrimPulls();
				_plugin.Capture.SaveToDisk();
			}
		}
		ImGui.Spacing();
		bool logActions = configuration.LogActions;
		ToggleRow("Log casts and abilities", "Capture player and enemy actions/casts. Disabling this saves memory but hides casts in replay/logs.", ref logActions, out var changedActions);
		if (changedActions)
		{
			configuration.LogActions = logActions;
			configuration.Save();
			if (!logActions)
			{
				_plugin.Capture.TrimPulls();
				_plugin.Capture.SaveToDisk();
			}
		}
		ImGui.Spacing();
		bool value3 = configuration.DebugHud;
		ToggleRow("Show debug overlay", "On-screen counters for active fight and captured events.", ref value3, out var changed3);
		if (changed3)
		{
			configuration.DebugHud = value3;
			configuration.Save();
		}
		Sep("Toggled off");
		int count = configuration.DisabledFights.Count;
		int count2 = configuration.DisabledMechanics.Count;
		if (count == 0 && count2 == 0)
		{
			ImGui.TextDisabled("Everything is enabled.");
		}
		else
		{
			ImU8String text = new ImU8String(38, 2);
			text.AppendFormatted(count);
			text.AppendLiteral(" fight(s) and ");
			text.AppendFormatted(count2);
			text.AppendLiteral(" mechanic(s) turned off.");
			ImGui.TextDisabled(text);
			ImGui.Spacing();
			if (ImGui.Button("Re-enable everything"))
			{
				configuration.DisabledFights.Clear();
				configuration.DisabledMechanics.Clear();
				configuration.Save();
			}
		}
		Sep("Game data");
		DrawHealth();
		Sep("Engine");
		ImGui.TextColored(host.HooksInstalled ? Ui.Green : Ui.Red, host.HooksInstalled ? "● Omen engine ready" : "✕ Omen engine failed to init");
		ImU8String text2 = new ImU8String(29, 2);
		text2.AppendLiteral("Active fight: ");
		text2.AppendFormatted(host.FightName);
		text2.AppendLiteral("  ·  Territory ");
		text2.AppendFormatted(host.TerritoryId);
		ImGui.TextDisabled(text2);
	}
}
