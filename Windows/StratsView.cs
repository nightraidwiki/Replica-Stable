using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Replica.QuickDraws;
using Replica.Strats;

namespace Replica.Windows;

public sealed class StratsView
{
	private readonly Plugin _plugin;

	private string _status = "";

	private static readonly string[] RoleNames = new string[8] { "MT", "OT", "M1", "M2", "R1", "R2", "H1", "H2" };

	public StratsView(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		Configuration configuration = _plugin.Configuration;
		float globalScale = ImGuiHelpers.GlobalScale;
		bool value = configuration.StratsEnabled;
		if (Ui.ToggleSwitch("##stratmaster", ref value))
		{
			configuration.StratsEnabled = value;
			configuration.Save();
		}
		ImGui.SameLine(0f, 8f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(value ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, "Strats enabled");
		ImGui.SameLine(0f, 16f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextColored(in Ui.Dimmed, "Your role");
		ImGui.SameLine();
		ImGui.SetNextItemWidth(80f * globalScale);
		int currentItem = (int)configuration.MyRole;
		if (ImGui.Combo("##myrole", ref currentItem, RoleNames, RoleNames.Length))
		{
			configuration.MyRole = (StratRole)currentItem;
			configuration.Save();
		}
		ImGui.SameLine(0f, 16f);
		ImGui.AlignTextToFramePadding();
		ImGui.TextDisabled("Pick a strat for this zone and your role; only your spot draws in the fight.");
		ImGui.Separator();
		uint territoryType = Plugin.ClientState.TerritoryType;
		ImGui.AlignTextToFramePadding();
		ImU8String text = new ImU8String(14, 1);
		text.AppendLiteral("Current zone: ");
		text.AppendFormatted(territoryType);
		ImGui.TextColored(in Ui.Dimmed, text);
		ImGui.SameLine();
		if (ImGui.Button("+ New strat (this zone)"))
		{
			StratPack stratPack = new StratPack
			{
				Name = "New strat",
				Territory = territoryType
			};
			configuration.StratPacks.Add(stratPack);
			configuration.Save();
			_plugin.OpenStrat(stratPack);
		}
		ImGui.SameLine();
		if (ImGui.Button("+ Example (Idyllic)"))
		{
			StratPack stratPack2 = StratLibrary.BuildIdyllicExample(territoryType);
			configuration.StratPacks.Add(stratPack2);
			configuration.SelectedStrat[territoryType.ToString()] = stratPack2.Id;
			configuration.Save();
			_plugin.OpenStrat(stratPack2);
		}
		ImGui.SameLine();
		if (ImGui.Button("Clear shapes"))
		{
			_plugin.Host.CleanVfx();
		}
		ImGui.SameLine();
		if (ImGui.Button("Import strat"))
		{
			ImportFromClipboard();
		}
		if (!string.IsNullOrEmpty(_status))
		{
			ImGui.SameLine();
			ImGui.TextDisabled(_status);
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (configuration.StratPacks.Count == 0)
		{
			ImGui.TextColored(in Ui.Dimmed, "No strats yet. Make one for this zone and place your role's spots.");
			return;
		}
		StratPack stratPack3 = null;
		foreach (StratPack stratPack4 in configuration.StratPacks)
		{
			ImGui.PushID(stratPack4.Id);
			bool value2 = stratPack4.Enabled;
			if (Ui.ToggleSwitch("##pen", ref value2))
			{
				stratPack4.Enabled = value2;
				configuration.Save();
			}
			ImGui.SameLine(0f, 8f);
			bool num = IsActive(configuration, stratPack4);
			ImGui.AlignTextToFramePadding();
			ImGui.TextColored(num ? Ui.Green : ((stratPack4.Territory == territoryType) ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed), stratPack4.Name);
			ImGui.SameLine();
			ImU8String text2 = new ImU8String(16, 2);
			text2.AppendLiteral("(zone ");
			text2.AppendFormatted(stratPack4.Territory);
			text2.AppendLiteral(" · ");
			text2.AppendFormatted(stratPack4.Slides.Count);
			text2.AppendLiteral(" steps)");
			ImGui.TextColored(in Ui.Dimmed, text2);
			float x = ImGui.GetContentRegionAvail().X;
			float num2 = 220f * globalScale;
			if (x > num2)
			{
				ImGui.SameLine(ImGui.GetCursorPosX() + (x - num2));
			}
			else
			{
				ImGui.SameLine();
			}
			if (ImGui.SmallButton(num ? "Active" : "Select"))
			{
				configuration.SelectedStrat[stratPack4.Territory.ToString()] = stratPack4.Id;
				configuration.Save();
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Edit"))
			{
				_plugin.OpenStrat(stratPack4);
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("Share"))
			{
				ExportToClipboard(stratPack4);
			}
			ImGui.SameLine();
			if (ImGui.SmallButton("X"))
			{
				stratPack3 = stratPack4;
			}
			if (num)
			{
				DrawManualPicks(stratPack4);
			}
			ImGui.PopID();
			ImGui.Spacing();
		}
		if (stratPack3 != null)
		{
			configuration.StratPacks.Remove(stratPack3);
			configuration.Save();
		}
	}

	private void DrawManualPicks(StratPack p)
	{
		ImGui.Indent(20f);
		foreach (StratSlide slide in p.Slides)
		{
			if (slide.Branches.Count <= 1 || !slide.Branches.Exists((StratBranch b) => b.Detect == BranchDetect.Manual))
			{
				continue;
			}
			ImGui.AlignTextToFramePadding();
			ImU8String text = new ImU8String(1, 1);
			text.AppendFormatted(slide.Name);
			text.AppendLiteral(":");
			ImGui.TextColored(in Ui.Dimmed, text);
			ImGui.SameLine();
			string manualBranch = _plugin.Strat.GetManualBranch(slide.Id);
			foreach (StratBranch branch in slide.Branches)
			{
				bool num = manualBranch == branch.Id;
				if (num)
				{
					Vector4 accent = Ui.Accent;
					accent.W = 0.85f;
					ImGui.PushStyleColor(ImGuiCol.Button, accent);
				}
				ImU8String label = new ImU8String(2, 2);
				label.AppendFormatted(branch.Name);
				label.AppendLiteral("##");
				label.AppendFormatted(branch.Id);
				if (ImGui.SmallButton(label))
				{
					_plugin.Strat.SetManualBranch(slide.Id, branch.Id);
				}
				if (num)
				{
					ImGui.PopStyleColor();
				}
				ImGui.SameLine();
			}
			ImGui.NewLine();
		}
		ImGui.Unindent(20f);
	}

	private void ExportToClipboard(StratPack p)
	{
		try
		{
			StratPack stratPack = p.Clone();
			stratPack.BuiltIn = false;
			ImGui.SetClipboardText(ShareCodec.Encode("YAPSTRAT1:", stratPack));
			_status = "Strat code copied";
		}
		catch (Exception ex)
		{
			_status = "Copy failed";
			Plugin.Log.Warning("[Replica] strat export: " + ex.Message);
		}
	}

	private void ImportFromClipboard()
	{
		string clipboardText = ImGui.GetClipboardText();
		StratPack value;
		if (string.IsNullOrWhiteSpace(clipboardText))
		{
			_status = "Clipboard empty";
		}
		else if (ShareCodec.TryDecode<StratPack>("YAPSTRAT1:", clipboardText, out value) && value != null && value.Slides != null)
		{
			value.Id = Guid.NewGuid().ToString("N");
			value.BuiltIn = false;
			foreach (StratSlide slide in value.Slides)
			{
				slide.Id = Guid.NewGuid().ToString("N");
				foreach (StratBranch branch in slide.Branches)
				{
					branch.Id = Guid.NewGuid().ToString("N");
				}
			}
			_plugin.Configuration.StratPacks.Add(value);
			_plugin.Configuration.Save();
			_status = "Imported \"" + value.Name + "\"";
		}
		else
		{
			_status = "Not a Replica strat code";
		}
	}

	private static bool IsActive(Configuration cfg, StratPack p)
	{
		if (!cfg.StratsEnabled || !p.Enabled)
		{
			return false;
		}
		if (p.Territory != Plugin.ClientState.TerritoryType)
		{
			return false;
		}
		if (cfg.SelectedStrat.TryGetValue(p.Territory.ToString(), out string value) && !string.IsNullOrEmpty(value))
		{
			return value == p.Id;
		}
		return cfg.StratPacks.Find((StratPack x) => x.Enabled && x.Territory == p.Territory) == p;
	}
}
