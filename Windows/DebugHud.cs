using System;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Replica.Engine;
using Replica.Engine.Preview;
using Replica.Engine.Vfx;
using Replica.Logging;

namespace Replica.Windows;

public sealed class DebugHud
{
	private readonly Plugin _plugin;

	private bool? _previewOk;

	private static readonly string BuildStamp = GetBuildStamp();

	public DebugHud(Plugin plugin)
	{
		_plugin = plugin;
	}

	public void Draw()
	{
		if (!_plugin.Configuration.DebugHud)
		{
			return;
		}
		ImGui.SetNextWindowPos(ImGui.GetMainViewport().WorkPos + new Vector2(12f, 12f), ImGuiCond.FirstUseEver);
		ImGui.SetNextWindowBgAlpha(0.85f);
		if (!ImGui.Begin("Replica debug", ImGuiWindowFlags.NoNav | ImGuiWindowFlags.AlwaysAutoResize))
		{
			ImGui.End();
			return;
		}
		CombatLogCapture capture = _plugin.Capture;
		FightModuleHost host = _plugin.Host;
		Vector4 col = new Vector4(1f, 0.85f, 0.3f, 1f);
		ImU8String text = new ImU8String(7, 1);
		text.AppendLiteral("Build: ");
		text.AppendFormatted(BuildStamp);
		ImGui.TextColored(in col, text);
		ImU8String text2 = new ImU8String(14, 1);
		text2.AppendLiteral("Active fight: ");
		text2.AppendFormatted(host.FightName);
		ImGui.TextUnformatted(text2);
		ImU8String text3 = new ImU8String(23, 2);
		text3.AppendLiteral("Territory: ");
		text3.AppendFormatted(host.TerritoryId);
		text3.AppendLiteral("   modules: ");
		text3.AppendFormatted(host.ModuleCount);
		ImGui.TextUnformatted(text3);
		ImU8String text4 = new ImU8String(43, 4);
		text4.AppendLiteral("Hooks: casts ");
		text4.AppendFormatted(capture.ActionEffectInstalled);
		text4.AppendLiteral("   control ");
		text4.AppendFormatted(capture.ActorControlInstalled);
		text4.AppendLiteral("   mapfx ");
		text4.AppendFormatted(capture.MapEffectInstalled);
		text4.AppendLiteral("   tether ");
		text4.AppendFormatted(VfxContainerHooks.Installed);
		ImGui.TextUnformatted(text4);
		if (!string.IsNullOrEmpty(capture.InstallError))
		{
			ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), capture.InstallError);
		}
		if (!VfxContainerHooks.Installed && !string.IsNullOrEmpty(VfxContainerHooks.InstallError))
		{
			col = new Vector4(1f, 0.4f, 0.4f, 1f);
			ImU8String text5 = new ImU8String(12, 1);
			text5.AppendLiteral("tether err: ");
			text5.AppendFormatted(VfxContainerHooks.InstallError);
			ImGui.TextColored(in col, text5);
		}
		ImU8String text6 = new ImU8String(15, 1);
		text6.AppendLiteral("Capturing now: ");
		text6.AppendFormatted(capture.ShouldCapture());
		ImGui.TextUnformatted(text6);
		ImU8String text7 = new ImU8String(16, 1);
		text7.AppendLiteral("Actors tracked: ");
		text7.AppendFormatted(capture.ActorsTracked);
		ImGui.TextUnformatted(text7);
		ImGui.Separator();
		ImU8String text8 = new ImU8String(19, 2);
		text8.AppendLiteral("Memory events: ");
		text8.AppendFormatted(capture.TotalEmitted);
		text8.AppendLiteral("  (");
		text8.AppendFormatted(Ago(capture.LastEventAt));
		text8.AppendLiteral(")");
		ImGui.TextUnformatted(text8);
		ImU8String text9 = new ImU8String(21, 2);
		text9.AppendLiteral("  casts ");
		text9.AppendFormatted(capture.KindCount(LogKind.CastStart));
		text9.AppendLiteral("   abilities ");
		text9.AppendFormatted(capture.KindCount(LogKind.Ability));
		ImGui.TextUnformatted(text9);
		ImU8String text10 = new ImU8String(31, 3);
		text10.AppendLiteral("  status+ ");
		text10.AppendFormatted(capture.KindCount(LogKind.StatusGain));
		text10.AppendLiteral("   status- ");
		text10.AppendFormatted(capture.KindCount(LogKind.StatusLose));
		text10.AppendLiteral("   deaths ");
		text10.AppendFormatted(capture.KindCount(LogKind.Death));
		ImGui.TextUnformatted(text10);
		ImU8String text11 = new ImU8String(25, 2);
		text11.AppendLiteral("  headmarkers ");
		text11.AppendFormatted(capture.KindCount(LogKind.Headmarker));
		text11.AppendLiteral("   tethers ");
		text11.AppendFormatted(capture.KindCount(LogKind.Tether));
		ImGui.TextUnformatted(text11);
		long num = capture.KindCount(LogKind.MapEffect);
		col = ((num > 0) ? new Vector4(0.4f, 0.9f, 0.4f, 1f) : new Vector4(1f, 0.6f, 0.3f, 1f));
		ImU8String text12 = new ImU8String(13, 1);
		text12.AppendLiteral("  mapeffects ");
		text12.AppendFormatted(num);
		ImGui.TextColored(in col, text12);
		if (!string.IsNullOrEmpty(capture.MapEffectError))
		{
			col = new Vector4(1f, 0.4f, 0.4f, 1f);
			ImU8String text13 = new ImU8String(13, 1);
			text13.AppendLiteral("  mapfx err: ");
			text13.AppendFormatted(capture.MapEffectError);
			ImGui.TextColored(in col, text13);
		}
		if (!string.IsNullOrEmpty(capture.RecentMapEffects))
		{
			ImU8String text14 = new ImU8String(9, 1);
			text14.AppendLiteral("  mapfx: ");
			text14.AppendFormatted(capture.RecentMapEffects);
			ImGui.TextWrapped(text14);
		}
		ImGui.Separator();
		Configuration configuration = _plugin.Configuration;
		bool v = configuration.ForceUmadActive;
		if (ImGui.Checkbox("Force UMAD active", ref v))
		{
			configuration.ForceUmadActive = v;
			configuration.Save();
		}
		ImGui.TextDisabled("For AnoMech / sim zones outside the duty.");
		if (host.UmadForced)
		{
			ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1f), "UMAD modules listening");
		}
		ImGui.Separator();
		ImGui.TextUnformatted("UMAD preview (works in any zone)");
		if (ImGui.Button("Preview UMAD telegraphs"))
		{
			_previewOk = UmadPreview.Run();
		}
		ImGui.SameLine();
		if (ImGui.Button("Clear draws"))
		{
			host.CleanVfx();
		}
		if (_previewOk == false)
		{
			ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), "no local player");
		}
		ImGui.End();
	}

	private static string GetBuildStamp()
	{
		try
		{
			string fullName = Plugin.PluginInterface.AssemblyLocation.FullName;
			if (!string.IsNullOrEmpty(fullName) && File.Exists(fullName))
			{
				return File.GetLastWriteTime(fullName).ToString("HH:mm:ss");
			}
			return "dev";
		}
		catch
		{
			return "?";
		}
	}

	private static string Ago(DateTime t)
	{
		if (t == DateTime.MinValue)
		{
			return "never";
		}
		double totalSeconds = (DateTime.Now - t).TotalSeconds;
		if (!(totalSeconds < 1.0))
		{
			return $"{totalSeconds:0}s ago";
		}
		return "now";
	}
}
