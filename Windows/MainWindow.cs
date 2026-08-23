using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Replica.Windows;

public sealed class MainWindow : Window, IDisposable
{
	private readonly Plugin _plugin;

	private readonly HomeView _home;

	private readonly LogWindow _log;

	private readonly ModulesView _modules;

	private readonly QuickDrawsView _quickDraws;

	private readonly BossModMirrorView _bossmod;

	private readonly LibraryView _library;

	private readonly ActorMapWindow _map;

	private readonly ConfigWindow _config;

	private readonly HacksView _hacks;

	private readonly RecruitmentView _recruitment;
	public RecruitmentView Recruitment => _recruitment;

	private string? _pendingTab;
	private string? _pendingSubTab;

	public MainWindow(Plugin plugin, LogWindow log, ConfigWindow config)
		: base("Replica v" + Changelog.Version + "###ReplicaMain")
	{
		_plugin = plugin;
		_home = new HomeView(plugin);
		_log = log;
		_modules = new ModulesView(plugin);
		_quickDraws = new QuickDrawsView(plugin);
		_bossmod = new BossModMirrorView(plugin);
		_library = new LibraryView(plugin);
		_map = new ActorMapWindow(plugin);
		_config = config;
		_hacks = new HacksView(plugin);
		_recruitment = new RecruitmentView(plugin);
		base.SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(740f, 480f),
			MaximumSize = new Vector2(2400f, 2000f)
		};
	}

	public void Dispose()
	{
		_map.Dispose();
		_recruitment.Dispose();
	}

	public override void PreDraw()
	{
		Ui.PushTheme();
	}

	public override void PostDraw()
	{
		Ui.PopTheme();
	}

	public void Show(string tab)
	{
		switch (tab.ToLowerInvariant())
		{
			case "modules":
				if (_plugin.Configuration.ModulesEnabled)
				{
					_pendingTab = "draw";
					_pendingSubTab = "modules";
				}
				break;
			case "quickdraws":
			case "quickdraw":
			case "quick_draws":
				_pendingTab = "draw";
				_pendingSubTab = "quickdraws";
				break;
			case "draw":
			case "draws":
				if (_plugin.Configuration.ModulesEnabled || _plugin.Configuration.QuickDrawsEnabled)
				{
					_pendingTab = "draw";
				}
				break;
			case "log":
			case "fightlog":
			case "fightlogs":
				if (_plugin.Configuration.LogsDataEnabled)
				{
					_pendingTab = "logsdata";
					_pendingSubTab = "log";
				}
				break;
			case "library":
				if (_plugin.Configuration.LogsDataEnabled)
				{
					_pendingTab = "logsdata";
					_pendingSubTab = "library";
				}
				break;
			case "map":
			case "livemap":
				if (_plugin.Configuration.LogsDataEnabled)
				{
					_pendingTab = "logsdata";
					_pendingSubTab = "map";
				}
				break;
			case "logs":
			case "logsdata":
			case "logdata":
				if (_plugin.Configuration.LogsDataEnabled)
				{
					_pendingTab = "logsdata";
				}
				break;
			case "bossmod":
				if (_plugin.Configuration.BossModMirrorEnabled)
				{
					_pendingTab = "bossmod";
				}
				break;
			case "hacks":
				if (_plugin.Configuration.HacksEnabled)
				{
					_pendingTab = "hacks";
				}
				break;
			case "pf":
			case "partyfinder":
			case "recruitment":
			case "recrutement":
				_pendingTab = "pf";
				break;
			default:
				_pendingTab = tab;
				break;
		}

		base.IsOpen = true;
		BringToFront();
	}

	public override void Draw()
	{
		if (ImGui.BeginTabBar("##yaptabs"))
		{
			Tab("Home", "home", _home.Draw);
			Tab("Party Finder", "pf", _recruitment.Draw);
			if (_plugin.Configuration.ModulesEnabled || _plugin.Configuration.QuickDrawsEnabled)
			{
				Tab("Draw", "draw", DrawDrawTab);
			}
			if (_plugin.Configuration.BossModMirrorEnabled)
			{
				Tab("BossMod Mirror", "bossmod", _bossmod.Draw);
			}
			if (_plugin.Configuration.LogsDataEnabled)
			{
				Tab("Logs Data", "logsdata", DrawLogsDataTab);
			}
			if (_plugin.Configuration.HacksEnabled)
			{
				Tab("Hacks", "hacks", _hacks.Draw);
			}
			Tab("Settings", "settings", _config.DrawContent);
			ImGui.EndTabBar();
			_pendingTab = null;
		}
	}

	private void DrawDrawTab()
	{
		if (ImGui.BeginTabBar("##drawtabs"))
		{
			if (_plugin.Configuration.ModulesEnabled)
			{
				SubTab("Modules", "modules", _modules.Draw);
			}
			SubTab("Quick Draws", "quickdraws", _quickDraws.Draw);
			ImGui.EndTabBar();
			_pendingSubTab = null;
		}
	}

	private void DrawLogsDataTab()
	{
		if (ImGui.BeginTabBar("##logsdatatabs"))
		{
			SubTab("Fight Log", "log", _log.DrawContent);
			SubTab("Library", "library", _library.Draw);
			SubTab("Live Map", "map", _map.Draw);
			ImGui.EndTabBar();
			_pendingSubTab = null;
		}
	}

	private void Tab(string label, string id, Action body)
	{
		ImGuiTabItemFlags imGuiTabItemFlags = ImGuiTabItemFlags.None;
		if (_pendingTab == id)
		{
			imGuiTabItemFlags |= ImGuiTabItemFlags.SetSelected;
		}
		ImU8String label2 = new ImU8String(7, 2);
		label2.AppendFormatted(label);
		label2.AppendLiteral("###tab_");
		label2.AppendFormatted(id);
		if (ImGui.BeginTabItem(label2, imGuiTabItemFlags))
		{
			ImGui.Spacing();
			body();
			ImGui.EndTabItem();
		}
	}

	private void SubTab(string label, string id, Action body)
	{
		ImGuiTabItemFlags imGuiTabItemFlags = ImGuiTabItemFlags.None;
		if (_pendingSubTab == id)
		{
			imGuiTabItemFlags |= ImGuiTabItemFlags.SetSelected;
		}
		ImU8String label2 = new ImU8String(7, 2);
		label2.AppendFormatted(label);
		label2.AppendLiteral("###subtab_");
		label2.AppendFormatted(id);
		if (ImGui.BeginTabItem(label2, imGuiTabItemFlags))
		{
			ImGui.Spacing();
			body();
			ImGui.EndTabItem();
		}
	}
}
