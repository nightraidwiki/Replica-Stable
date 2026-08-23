using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.Chat;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.DutyState;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Replica.Engine;
using Replica.Logging;
using Replica.QuickDraws;
using Replica.Scripting.Host;
using Replica.Strats;
using Replica.Windows;
using Replica.Engine.Bridge;
using Replica.Engine.Hacks;

namespace Replica;

public sealed class Plugin : IDalamudPlugin, IDisposable
{
	private static ExcelSheet<Lumina.Excel.Sheets.Action>? _actionsEn;

	private static ExcelSheet<Status>? _statusesEn;

	private const string CmdMain = "/replica";

	private const string CmdShort = "/rep";

	internal static Configuration? ConfigStatic;

	public readonly WindowSystem WindowSystem = new WindowSystem("Replica");

	private readonly MainWindow _mainWindow;

	private readonly LogWindow _logWindow;

	private readonly ConfigWindow _configWindow;

	private readonly ModuleConfigWindow _moduleConfigWindow;

	private readonly QuickDrawEditorWindow _quickDrawEditor;

	private readonly StratEditorWindow _stratEditor;

	private readonly DebugHud _debugHud;

	private readonly LabelOverlay _labelOverlay;

	private readonly ArrowOverlay _arrowOverlay;

	private readonly RecruitmentToastOverlay _recruitmentToaster;

	private readonly IFontHandle _labelFont;

	internal static Plugin? Instance { get; private set; }

	[PluginService]
	internal static IDalamudPluginInterface PluginInterface { get; private set; }

	[PluginService]
	internal static ICommandManager CommandManager { get; private set; }

	[PluginService]
	internal static IClientState ClientState { get; private set; }

	[PluginService]
	internal static IPlayerState PlayerState { get; private set; }

	[PluginService]
	internal static IObjectTable ObjectTable { get; private set; }

	[PluginService]
	internal static IFramework Framework { get; private set; }

	[PluginService]
	internal static IDataManager DataManager { get; private set; }

	[PluginService]
	internal static IDutyState DutyState { get; private set; }

	[PluginService]
	internal static ICondition Condition { get; private set; }

	[PluginService]
	internal static IGameInteropProvider GameInterop { get; private set; }

	[PluginService]
	internal static ISigScanner SigScanner { get; private set; }

	[PluginService]
	internal static IAddonLifecycle AddonLifecycle { get; private set; }

	[PluginService]
	internal static ITextureProvider TextureProvider { get; private set; }

	[PluginService]
	internal static IGameGui GameGui { get; private set; }

	[PluginService]
	internal static IPartyList PartyList { get; private set; }

	[PluginService]
	internal static IChatGui ChatGui { get; private set; }

	[PluginService]
	internal static IToastGui ToastGui { get; private set; }

	[PluginService]
	internal static IPluginLog Log { get; private set; }

	internal static ExcelSheet<Lumina.Excel.Sheets.Action> Actions => _actionsEn ?? (_actionsEn = DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>(ClientLanguage.English));

	internal static ExcelSheet<Status> Statuses => _statusesEn ?? (_statusesEn = DataManager.GetExcelSheet<Status>(ClientLanguage.English));

	public Configuration Configuration { get; private set; }

	public static Configuration Config => ConfigStatic;

	public CombatLogCapture Capture { get; }

	public FightModuleHost Host { get; }

	public QuickDrawEngine Engine { get; }

	public FightCatalog Catalog { get; }

	public StratEngine Strat { get; }

	public ScriptManager Scripts { get; }

	public BossModBridgeEngine BossModBridge { get; }

	public InvulnerabilityService Invulnerability { get; }

	public SlidecastService Slidecast { get; }

	public ExtendedRangeService ExtendedRange { get; }

	public GapCloserRangeService GapCloserRange { get; }

	public SpeedService Speed { get; }

	public CastRecastService CastRecast { get; }

	public LocalFlightService LocalFlight { get; }

	public NoClipService NoClip { get; }

	public IFontHandle LabelFont => _labelFont;

	public static void DebugLog(string message)
	{
		Log?.Debug(message);
	}

	public static void DebugChat(string message)
	{
		Log?.Info("[Replica] " + message);
	}

	public static void Chat(string message)
	{
		DebugLog(message);
	}

	public Plugin()
	{
		Instance = this;
		Log.Information("=== REPLICA DEV VERSION CONSTRUCTOR EXECUTION ===");
		Configuration = (PluginInterface.GetPluginConfig() as Configuration) ?? new Configuration();
		if (!Configuration.HacksEnabled && Configuration.HacksUnlocked)
		{
			Configuration.HacksUnlocked = false;
			Configuration.Save();
		}
		if (Configuration.CustomAlpha < 1f)
		{
			Configuration.CustomAlpha = 1.5f;
		}
		Configuration.QuickDrawModules.Clear();
		// Load built-in default quick draws in the Extreme category
		foreach (var module in DefaultQuickDraws.GetModules())
		{
			var existing = Configuration.QuickDrawModules.FirstOrDefault(m => m.Name == module.Name);
			if (existing != null)
			{
				existing.Category = module.Category;
				existing.BuiltIn = module.BuiltIn;
			}
			else
			{
				Configuration.QuickDrawModules.Add(module);
			}
		}
		foreach (QuickDrawModule quickDrawModule in Configuration.QuickDrawModules)
		{
			foreach (QuickDrawDef draw in quickDrawModule.Draws)
			{
				draw.Draw.NormalizeLegacy();
				foreach (FollowUpStep followUp in draw.FollowUps)
				{
					followUp.Draw.NormalizeLegacy();
				}
			}
		}
		ConfigStatic = Configuration;
		Capture = new CombatLogCapture(Configuration, GameInterop, Log);
		Host = new FightModuleHost(Log, Capture);
		Engine = new QuickDrawEngine(Configuration, Log, Capture);
		Catalog = new FightCatalog(PluginInterface.GetPluginConfigDirectory(), Log);
		Strat = new StratEngine(Configuration, Engine, Log, Capture);
		BossModBridge = new BossModBridgeEngine(this);
		Capture.ActiveAoeProvider = () => BossModBridge.GetActiveMapAoes();
		Scripts = new ScriptManager(Configuration);
		Invulnerability = new InvulnerabilityService(this);
		Slidecast = new SlidecastService(this);
		ExtendedRange = new ExtendedRangeService(this);
		GapCloserRange = new GapCloserRangeService(this);
		Speed = new SpeedService(this);
		CastRecast = new CastRecastService(this);
		LocalFlight = new LocalFlightService(this);
		NoClip = new NoClipService(this);
		Capture.OnEvent += Host.OnEvent;
		Capture.OnEvent += Engine.Handle;
		Capture.OnEvent += Scripts.OnLogEvent;
		Capture.OnEvent += Catalog.Record;
		Capture.OnNpcYell += Host.HandleNpcYell;
		ChatGui.ChatMessage += OnChatMessage;
		DutyState.DutyWiped += OnDutyWiped;
		DutyState.DutyRecommenced += OnDutyRecommenced;
		DutyState.DutyCompleted += OnDutyCompleted;
		_labelFont = PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(delegate(IFontAtlasBuildToolkit e)
		{
			e.OnPreBuild(delegate(IFontAtlasBuildToolkitPreBuild tk)
			{
				string path = Environment.ExpandEnvironmentVariables("%SystemRoot%\\Fonts");
				SafeFontConfig fontConfig = new SafeFontConfig
				{
					SizePx = 72f
				};
				string[] array = new string[5] { "segoeuib.ttf", "arialbd.ttf", "ariblk.ttf", "tahomabd.ttf", "verdanab.ttf" };
				foreach (string path2 in array)
				{
					string path3 = Path.Combine(path, path2);
					if (File.Exists(path3))
					{
						tk.AddFontFromFile(path3, in fontConfig);
						return;
					}
				}
				tk.AddDalamudDefaultFont(72f);
			});
		});
		_debugHud = new DebugHud(this);
		_labelOverlay = new LabelOverlay(this);
		_arrowOverlay = new ArrowOverlay(this);
		_logWindow = new LogWindow(this);
		_configWindow = new ConfigWindow(this);
		_quickDrawEditor = new QuickDrawEditorWindow(this);
		_stratEditor = new StratEditorWindow(this);
		_mainWindow = new MainWindow(this, _logWindow, _configWindow);
		_moduleConfigWindow = new ModuleConfigWindow();
		_recruitmentToaster = new RecruitmentToastOverlay(this);
		WindowSystem.AddWindow(_mainWindow);
		WindowSystem.AddWindow(_moduleConfigWindow);
		WindowSystem.AddWindow(_quickDrawEditor);
		WindowSystem.AddWindow(_stratEditor);
		if (Configuration.LastSeenVersion != Changelog.Version)
		{
			Configuration.LastSeenVersion = Changelog.Version;
			Configuration.Save();
		}
		CommandManager.AddHandler("/replica", new CommandInfo(OnCommand)
		{
			ShowInHelp = false
		});
		CommandManager.AddHandler(CmdShort, new CommandInfo(OnCommand)
		{
			HelpMessage = "Open Replica.\n/rep modules\n/rep config\n/rep clean\n/rep scripts"
		});
		CommandManager.AddHandler("/invuln", new CommandInfo(OnInvulnCommand)
		{
			HelpMessage = "Toggle Invulnerability Mode",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/invul", new CommandInfo(OnInvulnCommand)
		{
			HelpMessage = "Toggle Invulnerability Mode",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/slidecast", new CommandInfo(OnSlidecastCommand)
		{
			HelpMessage = "Toggle Slidecast Mode (or /slidecast <seconds>)",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/slide", new CommandInfo(OnSlidecastCommand)
		{
			HelpMessage = "Toggle Slidecast Mode",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/extendedrange", new CommandInfo(OnExtendedRangeCommand)
		{
			HelpMessage = "Toggle Extended Action Range Mode",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/extrange", new CommandInfo(OnExtendedRangeCommand)
		{
			HelpMessage = "Toggle Extended Action Range Mode",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/gapcloser", new CommandInfo(OnGapCloserCommand)
		{
			HelpMessage = "Toggle Disable Gap Closer Range Limits",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/speed", new CommandInfo(OnSpeedCommand)
		{
			HelpMessage = "Toggle Speed Hack",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/deccast", new CommandInfo(OnDecCastCommand)
		{
			HelpMessage = "Toggle Cast Time Reduction (or /deccast <seconds>)",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/decrecast", new CommandInfo(OnDecRecastCommand)
		{
			HelpMessage = "Toggle Recast Time Reduction (or /decrecast <seconds>)",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/mudra", new CommandInfo(OnMudraCommand)
		{
			HelpMessage = "Toggle Ninja Instant Mudras (Zero GCD on Ten/Chi/Jin)",
			ShowInHelp = false
		});
		CommandManager.AddHandler("/noclip", new CommandInfo(OnNoClipCommand)
		{
			HelpMessage = "Toggle Noclip Mode",
			ShowInHelp = false
		});
		PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
		PluginInterface.UiBuilder.Draw += _debugHud.Draw;
		PluginInterface.UiBuilder.Draw += _arrowOverlay.Draw;
		PluginInterface.UiBuilder.Draw += _labelOverlay.Draw;
		PluginInterface.UiBuilder.Draw += BossModBridge.DrawOverlay;
		PluginInterface.UiBuilder.Draw += _recruitmentToaster.DrawOverlay;
		PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
		PluginInterface.UiBuilder.OpenMainUi += ToggleMain;
		ClientState.Login += OnLogin;
		Framework.Update += OnFrameworkUpdate;
		try
		{
			Scripts.Reload();
		}
		catch (Exception ex)
		{
			Log.Error("[Replica] script load: " + ex.Message);
		}
	}

	private void OnLogin()
	{
		if (Configuration.OpenOnLogin)
		{
			_mainWindow.Show("home");
		}
	}

	private void OnDutyWiped(IDutyStateEventArgs args)
	{
		Host.HandleDutyWipe();
	}

	private void OnDutyRecommenced(IDutyStateEventArgs args)
	{
		Host.HandleDutyWipe();
	}

	private void OnDutyCompleted(IDutyStateEventArgs args)
	{
		Host.HandleDutyWipe();
	}

	public void Dispose()
	{
		Framework.Update -= OnFrameworkUpdate;
		ChatGui.ChatMessage -= OnChatMessage;
		Capture.OnEvent -= Host.OnEvent;
		Capture.OnEvent -= Engine.Handle;
		Capture.OnEvent -= Scripts.OnLogEvent;
		Scripts.Dispose();
		Capture.OnEvent -= Catalog.Record;
		Capture.OnNpcYell -= Host.HandleNpcYell;
		DutyState.DutyWiped -= OnDutyWiped;
		DutyState.DutyRecommenced -= OnDutyRecommenced;
		DutyState.DutyCompleted -= OnDutyCompleted;
		try
		{
			Catalog.Save();
		}
		catch (Exception ex)
		{
			Log.Debug("[Replica] catalog save: " + ex.Message);
		}
		PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
		PluginInterface.UiBuilder.Draw -= _debugHud.Draw;
		PluginInterface.UiBuilder.Draw -= _arrowOverlay.Draw;
		PluginInterface.UiBuilder.Draw -= _labelOverlay.Draw;
		PluginInterface.UiBuilder.Draw -= BossModBridge.DrawOverlay;
		PluginInterface.UiBuilder.Draw -= _recruitmentToaster.DrawOverlay;
		PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
		PluginInterface.UiBuilder.OpenMainUi -= ToggleMain;
		ClientState.Login -= OnLogin;
		WindowSystem.RemoveAllWindows();
		try
		{
			_labelFont.Dispose();
		}
		catch (Exception ex2)
		{
			Log.Debug("[Replica] label font dispose: " + ex2.Message);
		}
		Assets.Dispose();
		try
		{
			Host.Dispose();
		}
		catch (Exception ex3)
		{
			Log.Debug("[Replica] host dispose: " + ex3.Message);
		}
		try
		{
			Capture.Dispose();
		}
		catch (Exception ex4)
		{
			Log.Debug("[Replica] capture dispose: " + ex4.Message);
		}
		try
		{
			BossModBridge.Dispose();
		}
		catch (Exception exBoss)
		{
			Log.Debug("[Replica] bossmod bridge dispose: " + exBoss.Message);
		}
		try
		{
			Invulnerability.Dispose();
		}
		catch (Exception exInvul)
		{
			Log.Debug("[Replica] invulnerability dispose: " + exInvul.Message);
		}
		try
		{
			Slidecast.Dispose();
		}
		catch (Exception exSlide)
		{
			Log.Debug("[Replica] slidecast dispose: " + exSlide.Message);
		}
		try
		{
			ExtendedRange.Dispose();
		}
		catch (Exception exRange)
		{
			Log.Debug("[Replica] extended range dispose: " + exRange.Message);
		}
		try
		{
			GapCloserRange.Dispose();
		}
		catch (Exception exGap)
		{
			Log.Debug("[Replica] gap closer range dispose: " + exGap.Message);
		}
		try
		{
			Speed.Dispose();
		}
		catch (Exception exSpeed)
		{
			Log.Debug("[Replica] speed dispose: " + exSpeed.Message);
		}
		try
		{
			CastRecast.Dispose();
		}
		catch (Exception exCr)
		{
			Log.Debug("[Replica] cast/recast dispose: " + exCr.Message);
		}
		try
		{
			LocalFlight.Dispose();
		}
		catch (Exception exFlight)
		{
			Log.Debug("[Replica] local flight dispose: " + exFlight.Message);
		}
		try
		{
			NoClip.Dispose();
		}
		catch (Exception exNoClip)
		{
			Log.Debug("[Replica] noclip dispose: " + exNoClip.Message);
		}
		CommandManager.RemoveHandler("/replica");
		CommandManager.RemoveHandler(CmdShort);
		CommandManager.RemoveHandler("/noclip");
		CommandManager.RemoveHandler("/invuln");
		CommandManager.RemoveHandler("/invul");
		CommandManager.RemoveHandler("/slidecast");
		CommandManager.RemoveHandler("/slide");
		CommandManager.RemoveHandler("/extendedrange");
		CommandManager.RemoveHandler("/extrange");
		CommandManager.RemoveHandler("/gapcloser");
		CommandManager.RemoveHandler("/speed");
		CommandManager.RemoveHandler("/deccast");
		CommandManager.RemoveHandler("/decrecast");
		CommandManager.RemoveHandler("/mudra");
		_recruitmentToaster.Dispose();
		_mainWindow.Dispose();
		Instance = null;
	}

	public void ShowRecruitment(int subTab = 0)
	{
		_mainWindow.Show("pf");
		_mainWindow.Recruitment.OpenSubTab(subTab);
	}

	public void UpdateAllHackHookStates()
	{
		try
		{
			Slidecast?.UpdateHookState();
			ExtendedRange?.UpdateHookState();
			GapCloserRange?.UpdateHookState();
			Speed?.UpdateHookState();
			CastRecast?.UpdateHookState();
			LocalFlight?.UpdateHookState();
			NoClip?.UpdateHookState();
		}
		catch (Exception ex)
		{
			Log?.Warning($"[Replica] Error updating all hack hook states: {ex.Message}");
		}
	}

	private bool CheckHacksEnabledAndUnlocked()
	{
		if (!Configuration.HacksEnabled)
		{
			ChatGui.PrintError("[Replica] Hacks are disabled. Please enable them in the Home menu first.");
			return false;
		}
		if (!Configuration.HacksUnlocked)
		{
			ChatGui.PrintError("[Replica] Hacks tab is locked. Please enter the password in the Hacks tab first (/rep hacks).");
			ShowTab("hacks");
			return false;
		}
		return true;
	}

	private void OnInvulnCommand(string command, string args)
	{
		if (CheckHacksEnabledAndUnlocked())
		{
			Invulnerability.Toggle();
		}
	}

	private void OnSlidecastCommand(string command, string args)
	{
		if (!CheckHacksEnabledAndUnlocked())
		{
			return;
		}

		string trimmed = args.Trim();
		if (!string.IsNullOrEmpty(trimmed) && float.TryParse(trimmed, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float window))
		{
			Slidecast.SlidecastWindow = window;
			Slidecast.IsEnabled = true;
			ChatGui.Print($"[Replica] Slidecast ENABLED with window {Slidecast.SlidecastWindow:F1}s.");
		}
		else
		{
			bool enabled = Slidecast.Toggle();
			ChatGui.Print($"[Replica] Slidecast {(enabled ? "ENABLED" : "DISABLED")} (window: {Slidecast.SlidecastWindow:F1}s).");
		}
	}

	private void OnExtendedRangeCommand(string command, string args)
	{
		if (CheckHacksEnabledAndUnlocked())
		{
			ExtendedRange.IsEnabled = !ExtendedRange.IsEnabled;
			ChatGui.Print($"[Replica] Extended Action Range {(ExtendedRange.IsEnabled ? "ENABLED" : "DISABLED")}.");
		}
	}

	private void OnGapCloserCommand(string command, string args)
	{
		if (CheckHacksEnabledAndUnlocked())
		{
			GapCloserRange.IsEnabled = !GapCloserRange.IsEnabled;
			ChatGui.Print($"[Replica] Disable Gap Closer Range Limits {(GapCloserRange.IsEnabled ? "ENABLED" : "DISABLED")}.");
		}
	}

	private void OnSpeedCommand(string command, string args)
	{
		if (CheckHacksEnabledAndUnlocked())
		{
			Speed.IsEnabled = !Speed.IsEnabled;
			ChatGui.Print($"[Replica] Speed Hack {(Speed.IsEnabled ? "ENABLED" : "DISABLED")} (multiplier: {Speed.SpeedMultiplier:F1}x).");
		}
	}

	private void OnDecCastCommand(string command, string args)
	{
		if (!CheckHacksEnabledAndUnlocked())
		{
			return;
		}

		string trimmed = args.Trim();
		if (!string.IsNullOrEmpty(trimmed) && float.TryParse(trimmed, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float seconds))
		{
			CastRecast.DecCastTime = seconds;
			CastRecast.DecCastEnabled = true;
			ChatGui.Print($"[Replica] Cast Time Reduction ENABLED (-{CastRecast.DecCastTime:F1}s).");
		}
		else
		{
			CastRecast.DecCastEnabled = !CastRecast.DecCastEnabled;
			ChatGui.Print($"[Replica] Cast Time Reduction {(CastRecast.DecCastEnabled ? "ENABLED" : "DISABLED")} (-{CastRecast.DecCastTime:F1}s).");
		}
	}

	private void OnDecRecastCommand(string command, string args)
	{
		if (!CheckHacksEnabledAndUnlocked())
		{
			return;
		}

		string trimmed = args.Trim();
		if (!string.IsNullOrEmpty(trimmed) && float.TryParse(trimmed, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float seconds))
		{
			CastRecast.DecRecastTime = seconds;
			CastRecast.DecRecastEnabled = true;
			ChatGui.Print($"[Replica] Recast Time Reduction ENABLED (-{CastRecast.DecRecastTime:F1}s).");
		}
		else
		{
			CastRecast.DecRecastEnabled = !CastRecast.DecRecastEnabled;
			ChatGui.Print($"[Replica] Recast Time Reduction {(CastRecast.DecRecastEnabled ? "ENABLED" : "DISABLED")} (-{CastRecast.DecRecastTime:F1}s).");
		}
	}

	private void OnMudraCommand(string command, string args)
	{
		if (CheckHacksEnabledAndUnlocked())
		{
			CastRecast.MudraNoRecastEnabled = !CastRecast.MudraNoRecastEnabled;
			ChatGui.Print($"[Replica] Ninja Instant Mudras {(CastRecast.MudraNoRecastEnabled ? "ENABLED" : "DISABLED")}.");
		}
	}

	private void OnNoClipCommand(string command, string args)
	{
		if (CheckHacksEnabledAndUnlocked())
		{
			NoClip.IsEnabled = !NoClip.IsEnabled;
			ChatGui.Print($"[Replica] Noclip Mode {(NoClip.IsEnabled ? "ENABLED" : "DISABLED")}.");
		}
	}

	private DateTime _lastRecruitmentPoll = DateTime.UtcNow.AddSeconds(-10);

	private void OnFrameworkUpdate(IFramework framework)
	{
		bool inCombat = Condition[ConditionFlag.InCombat];
		Capture.NotifyCombat(inCombat);
		if (Configuration.FirstRun)
		{
			Configuration.FirstRun = false;
			Configuration.Save();
		}
		Capture.Update();
		Host.Tick();
		Engine.Tick();
		BossModBridge.Tick();
		Scripts.Update();
		Catalog.MaybeSave();
		_quickDrawEditor.TickGroundPick();

		bool windowOpen = _mainWindow?.IsOpen ?? false;
		double pollInterval = windowOpen ? 30.0 : 60.0;

		if (!inCombat && (DateTime.UtcNow - _lastRecruitmentPoll).TotalSeconds >= pollInterval)
		{
			_lastRecruitmentPoll = DateTime.UtcNow;
			if (_mainWindow?.Recruitment != null)
			{
				_ = _mainWindow.Recruitment.PollNotificationsAsync();
			}
		}
	}

	private void OnChatMessage(IHandleableChatMessage message)
	{
		if ((int)(message.LogKind - 41) > 8)
		{
			Host.HandleChatMessage((uint)message.LogKind, message.Message.TextValue);
		}
	}

	private void HandleScripts()
	{
		Scripts.Reload();
		ChatGui.Print("[Replica] scripts folder: " + Scripts.ScriptsPath);
		if (Scripts.Scripts.Count == 0)
		{
			ChatGui.Print("[Replica] no scripts loaded.");
		}
		foreach (LoadedScript script in Scripts.Scripts)
		{
			string value = ((script.Territorys.Count == 0) ? "all zones" : string.Join(",", script.Territorys));
			ChatGui.Print($"[Replica] {script.Name} v{script.Version} by {script.Author} — {script.Actions.Count} handlers, {value}");
		}
		IReadOnlyList<string> loadErrors = Scripts.LoadErrors;
		if (loadErrors.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (string item in loadErrors)
		{
			if (!item.Contains("CS0009", StringComparison.Ordinal))
			{
				ChatGui.PrintError("[Replica] " + item);
				if (++num >= 8)
				{
					break;
				}
			}
		}
		if (loadErrors.Count > num)
		{
			ChatGui.PrintError($"[Replica] …and {loadErrors.Count - num} more (see dalamud.log)");
		}
		foreach (string item2 in loadErrors)
		{
			Log.Error("[Replica] " + item2);
		}
	}

	private void OnCommand(string command, string args)
	{
		string text = args.Trim().ToLowerInvariant();
		if (text.StartsWith("diag"))
		{
			HandleDiag(text);
			return;
		}
		if ((text == "scripts" || text == "reload") ? true : false)
		{
			HandleScripts();
			return;
		}
		if (text.StartsWith("slidecast") || text.StartsWith("slide"))
		{
			string slideArgs = text.StartsWith("slidecast") ? text.Substring("slidecast".Length).Trim() : text.Substring("slide".Length).Trim();
			OnSlidecastCommand(command, slideArgs);
			return;
		}
		switch (text)
		{
		case "c":
		case "settings":
				case "config":
			ShowTab("settings");
			break;
		case "bm":
		case "bossmod":
		case "mirror":
			if (!Configuration.BossModMirrorEnabled)
				ChatGui.PrintError("[Replica] BossMod Mirror is disabled. Please enable it in the Home menu first.");
			else
				ShowTab("bossmod");
			break;
		case "m":
		case "modules":
			if (!Configuration.ModulesEnabled)
				ChatGui.PrintError("[Replica] Modules are disabled. Please enable them in the Home menu first.");
			else
				ShowTab("modules");
			break;
		case "home":
			ShowTab("home");
			break;
		case "draw":
		case "draws":
			ShowTab("draw");
			break;
		case "qd":
		case "quickdraw":
		case "quickdraws":
			ShowTab("quickdraws");
			break;
		case "pf":
		case "partyfinder":
		case "recruitment":
		case "recrutement":
			ShowTab("pf");
			break;
		case "testtoast":
		case "toast":
			RecruitmentToastOverlay.AddToast(new RecruitmentToast
			{
				Kind = RecruitmentToastKind.ApplicationAccepted,
				Title = "Application Accepted!",
				Message = "Your application for AAC Light-heavyweight M4S was ACCEPTED! Coords unlocked.",
				Icon = FontAwesomeIcon.CheckCircle,
				OnOpen = () => ShowRecruitment(2)
			});
			ChatGui.Print("[Replica] Test recruitment toast banner triggered.");
			break;
		case "refresh":
		case "tokenrefresh":
			if (_mainWindow?.Recruitment?.Service != null)
			{
				ChatGui.Print("[Replica] Forcing token refresh...");
				_ = System.Threading.Tasks.Task.Run(async () =>
				{
					bool res = await _mainWindow.Recruitment.Service.RefreshTokenAsync();
					ChatGui.Print($"[Replica] Token refresh result: {res}");
				});
			}
			break;
		case "log":
		case "fightlog":
		case "fightlogs":
			if (!Configuration.LogsDataEnabled)
				ChatGui.PrintError("[Replica] Logs Data is disabled. Please enable it in the Home menu first.");
			else
				ShowTab("log");
			break;
		case "lib":
		case "library":
			if (!Configuration.LogsDataEnabled)
				ChatGui.PrintError("[Replica] Logs Data is disabled. Please enable it in the Home menu first.");
			else
				ShowTab("library");
			break;
		case "logs":
		case "logdata":
		case "logsdata":
			if (!Configuration.LogsDataEnabled)
				ChatGui.PrintError("[Replica] Logs Data is disabled. Please enable it in the Home menu first.");
			else
				ShowTab("logsdata");
			break;
		case "livemap":
		case "map":
			if (!Configuration.LogsDataEnabled)
				ChatGui.PrintError("[Replica] Logs Data is disabled. Please enable it in the Home menu first.");
			else
				ShowTab("map");
			break;
		case "hacks":
		case "hack":
			if (!Configuration.HacksEnabled)
				ChatGui.PrintError("[Replica] Hacks are disabled. Please enable them in the Home menu first.");
			else
				ShowTab("hacks");
			break;
		case "god":
		case "invul":
		case "invuln":
		case "godmode":
			if (CheckHacksEnabledAndUnlocked())
			{
				Invulnerability.Toggle();
			}
			break;
		case "range":
		case "extrange":
		case "extendedrange":
			if (CheckHacksEnabledAndUnlocked())
			{
				ExtendedRange.IsEnabled = !ExtendedRange.IsEnabled;
				ChatGui.Print($"[Replica] Extended Action Range {(ExtendedRange.IsEnabled ? "ENABLED" : "DISABLED")}.");
			}
			break;
		case "gap":
		case "gapcloser":
		case "dash":
			if (CheckHacksEnabledAndUnlocked())
			{
				GapCloserRange.IsEnabled = !GapCloserRange.IsEnabled;
				ChatGui.Print($"[Replica] Disable Gap Closer Range Limits {(GapCloserRange.IsEnabled ? "ENABLED" : "DISABLED")}.");
			}
			break;
		case "speed":
			OnSpeedCommand(command, "");
			break;
		case "deccast":
			OnDecCastCommand(command, "");
			break;
		case "decrecast":
			OnDecRecastCommand(command, "");
			break;
		case "mudra":
			OnMudraCommand(command, "");
			break;
		case "flight":
		case "localflight":
			if (CheckHacksEnabledAndUnlocked())
			{
				LocalFlight.IsEnabled = !LocalFlight.IsEnabled;
				ChatGui.Print($"[Replica] Local Flight Mode {(LocalFlight.IsEnabled ? "ENABLED" : "DISABLED")}.");
			}
			break;
		case "noflightlimit":
		case "flightrestrictions":
		case "restrictions":
			if (CheckHacksEnabledAndUnlocked())
			{
				LocalFlight.ProhibitFlightRestrictions = !LocalFlight.ProhibitFlightRestrictions;
				ChatGui.Print($"[Replica] Remove Flight Restrictions {(LocalFlight.ProhibitFlightRestrictions ? "ENABLED" : "DISABLED")}.");
			}
			break;
		case "noclip":
		case "ghost":
			if (CheckHacksEnabledAndUnlocked())
			{
				NoClip.IsEnabled = !NoClip.IsEnabled;
				ChatGui.Print($"[Replica] Noclip Mode {(NoClip.IsEnabled ? "ENABLED" : "DISABLED")}.");
			}
			break;
		case "cleanvfx":
		case "clean":
			Host.CleanVfx();
			DebugChat("Cleared all drawn VFX.");
			break;
		default:
			ToggleMain();
			break;
		}
	}

	private void HandleDiag(string lower)
	{
		string[] array = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length < 3)
		{
			ChatGui.Print("[Replica] /rep diag <all|resource|actioneffect|cast|actorcontrol|mapeffect|timelinesync|npcyell|vfx|tether> <on|off>");
			return;
		}
		string text = array[1];
		bool flag = array[2] == "on";
		if (text == "all")
		{
			Capture.SetAllGameHooks(flag);
			Host.SetResourceHook(flag);
			ChatGui.Print("[Replica] all game hooks " + (flag ? "ENABLED" : "DISABLED"));
		}
		else if (text == "resource")
		{
			Host.SetResourceHook(flag);
			ChatGui.Print("[Replica] resource hook " + (flag ? "ENABLED" : "DISABLED"));
		}
		else if (Capture.SetGameHook(text, flag))
		{
			ChatGui.Print("[Replica] " + text + " hook " + (flag ? "ENABLED" : "DISABLED"));
		}
		else
		{
			ChatGui.Print("[Replica] unknown hook '" + text + "'");
		}
	}

	public void ShowTab(string tab)
	{
		_mainWindow.Show(tab);
	}

	public void OpenChangelog()
	{
		ShowTab("home");
	}

	public void OpenModuleConfig(string title, System.Action body)
	{
		_moduleConfigWindow.Open(title, body);
	}

	public StratEditorWindow StratEditor => _stratEditor;

	public void OpenStrat(StratPack pack)
	{
		_stratEditor.Open(pack);
	}

	public void OpenQuickDraw(QuickDrawDef def)
	{
		_quickDrawEditor.Open(def);
	}

	public void OpenQuickDrawFor(LogEvent e)
	{
		_quickDrawEditor.OpenFor(e);
	}

	public void OpenQuickDrawForCatalog(FightCatalog.Entry entry, uint territory)
	{
		_quickDrawEditor.OpenForCatalog(entry, territory);
	}

	public void OpenQuickDrawForMapAoe(MapAoe aoe, string? actionName = null, uint territory = 0)
	{
		_quickDrawEditor.OpenForMapAoe(aoe, actionName, territory);
	}

	public void ToggleMain()
	{
		if (_mainWindow.IsOpen)
		{
			_mainWindow.IsOpen = false;
		}
		else
		{
			_mainWindow.Show("home");
		}
	}

	public void ToggleLog()
	{
		_mainWindow.Show("log");
	}

	public void ToggleConfig()
	{
		_mainWindow.Show("settings");
	}

	public void OpenConfig()
	{
		_mainWindow.Show("settings");
	}
}
