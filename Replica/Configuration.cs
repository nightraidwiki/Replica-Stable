using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Configuration;
using Replica.QuickDraws;
using Replica.Strats;

namespace Replica;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
	public const string QuickCategory = "Quick Draws";

	public float MapWaymarkSize = 15f;

	public float MapMarkerScale = 1f;

	public int MapPlayerShape = 1;

	public int MapEnemyShape;

	public bool MapJobIcons = true;

	public bool MapShowAoes { get; set; } = true;

	public float MapAoeOpacity { get; set; } = 0.35f;

	public int Version { get; set; } = 2;

	public CaptureMode CaptureWhen { get; set; }

	public int MaxPullsToKeep { get; set; } = 10;

	public bool LogOutsidePulls { get; set; } = false;

	public bool LogActions { get; set; } = true;

	public float CustomAlpha { get; set; } = 1.5f;

	public int EX8GazeOfTheVoidPriority { get; set; } = 1;

	public int M2SSweetheartDrawMode { get; set; }

	public Vector4 TopP6CosmoArrowColor { get; set; } = new Vector4(1f, 1f, 0f, 1f);

	public Vector4 FruP5HellfireColor { get; set; } = new Vector4(1f, 1f, 1f, 2f);

	public bool DancingMadBlockTelegraphs { get; set; } = true;

	public bool DancingMadBlockShockwave { get; set; } = true;

	public bool MahjongShowMyGuide { get; set; } = true;

	public MahjongMacroMode MahjongMacroSend { get; set; }

	public bool ModulesEnabled { get; set; } = true;

	public bool BossModMirrorEnabled { get; set; } = true;

	public bool BossModMirrorAOEs { get; set; } = true;

	public bool BossModMirrorSpreadsStacks { get; set; } = true;

	public bool BossModMirrorMovementArrows { get; set; } = true;

	public bool BossModMirrorSafeZones { get; set; } = true;

	public bool BossModMirrorTethers { get; set; } = true;

	public bool BossModMirrorPartnerTetherHelper { get; set; } = true;

	public bool BossModMirrorGaze { get; set; } = true;

	public bool BossModMirrorSmartTowers { get; set; } = true;

	public bool BossModMirrorExaflares { get; set; } = true;

	public bool BossModMirrorLineStacks { get; set; } = true;

	public bool BossModMirrorReturnSpots { get; set; } = true;
	public bool BossModMirrorHintsBanners { get; set; } = true;
	public bool BossModHintsNativeToast { get; set; } = false;
	public bool BossModHintsRiskOnly { get; set; } = false;
	public float BossModBannerOffsetY { get; set; } = 0.22f;
	public float BossModBannerScale { get; set; } = 1.0f;

	public float BossModHeightOffset { get; set; } = 0.02f;

	public float BossModArrowThickness { get; set; } = 3.0f;

	public float BossModTetherThickness { get; set; } = 2.5f;

	public uint BossModPartnerTetherColor { get; set; } = 0xFF00D7FF;

	public HashSet<string> DisabledFights { get; set; } = new HashSet<string>();

	public HashSet<string> DisabledMechanics { get; set; } = new HashSet<string>();

	public Dictionary<string, bool> ModuleEnabled { get; set; } = new Dictionary<string, bool>();

	public Dictionary<string, string> ModuleConfigs { get; set; } = new Dictionary<string, string>();

	public bool ScriptsEnabled { get; set; }

	public List<string> ScriptFolders { get; set; } = new List<string>();

	public HashSet<string> DisabledScripts { get; set; } = new HashSet<string>();

	public HashSet<string> DisabledMethods { get; set; } = new HashSet<string>();

	public bool QuickDrawsEnabled { get; set; } = true;

	public List<QuickDrawModule> QuickDrawModules { get; set; } = new List<QuickDrawModule>();

	public bool ShowMapFx { get; set; } = true;

	public bool ShowAdds { get; set; } = true;

	public bool ShowControl { get; set; } = true;

	public bool ShowPositions { get; set; } = true;

	public bool LogGameVfx { get; set; }

	public bool ShowVfx { get; set; } = true;

	public bool ShowCasts { get; set; } = true;

	public bool ShowStatus { get; set; } = true;

	public bool ShowDeaths { get; set; } = true;

	public bool ShowMarkers { get; set; } = true;

	public bool ShowEnemies { get; set; } = true;

	public bool ShowYou { get; set; } = true;

	public bool ShowParty { get; set; } = true;

	public bool ShowIds { get; set; } = true;

	public bool ShowDecIds { get; set; }

	public bool MapShowGameMap { get; set; } = true;

	public bool MapShowWaymarks { get; set; } = true;

	public bool MapShowNames { get; set; }

	public bool MapHideDead { get; set; }

	public bool MapShowPlayers { get; set; } = true;

	public bool MapShowEnemies { get; set; } = true;

	public bool MapShowAllies { get; set; } = true;

	public bool MapShowPets { get; set; }

	public bool MapShowObjects { get; set; }

	public bool MapHideUnnamed { get; set; } = true;

	public bool StratsEnabled { get; set; } = true;

	public StratRole MyRole { get; set; }

	public List<StratPack> StratPacks { get; set; } = new List<StratPack>();

	public Dictionary<string, string> SelectedStrat { get; set; } = new Dictionary<string, string>();

	public bool LogWindowOpen { get; set; } = true;

	public bool FirstRun { get; set; } = true;

	public string LastSeenVersion { get; set; } = "";

	public bool DebugHud { get; set; }

	public bool OpenOnLogin { get; set; }

	public bool ForceUmadActive { get; set; }

	public bool HacksEnabled { get; set; } = false;

	public bool HacksUnlocked { get; set; }

	public bool LogsDataEnabled { get; set; } = false;

	public bool SlidecastEnabled { get; set; }
	// Cast / Recast hack properties
	public bool DecCastEnabled { get; set; } = false;
	public float DecCastTime { get; set; } = 0.5f;
	public bool DecRecastEnabled { get; set; } = false;
	public float DecRecastTime { get; set; } = 0.5f;
	public bool MudraNoRecastEnabled { get; set; } = false;

	// Movement hack properties
	public bool SpeedEnabled { get; set; } = false;
	public float SpeedValue { get; set; } = 1.5f;
	public bool MaxAccelerationEnabled { get; set; } = false;
	public bool LocalFlightEnabled { get; set; } = false;
	public bool ProhibitFlightRestrictionsEnabled { get; set; } = false;
	public bool NoClipEnabled { get; set; } = false;


	public float SlidecastWindow { get; set; } = 0.5f;

	public bool ExtendedRangeEnabled { get; set; }

	public float ExtendedRangeDistance { get; set; } = 2.0f;

	public bool GapCloserRangeEnabled { get; set; }

	// Supabase & Recruitment PF Configuration
	public string SupabaseUrl { get; set; } = "https://donarysbdrbdaceackbe.supabase.co";
	public string SupabaseAnonKey { get; set; } = "sb_publishable_1_tLxWerihARY1tUikn0sw_ZRI11NPd";
	public string? DiscordAuthToken { get; set; }
	public string? DiscordRefreshToken { get; set; }
	public string? DiscordUserId { get; set; }
	public string? DiscordId { get; set; }
	public string? DiscordTag { get; set; }
	public string? DiscordAvatarUrl { get; set; }
	public long DiscordTokenExpiresAt { get; set; }
	public string AccountPseudo { get; set; } = string.Empty;
	public string AccountAetherphone { get; set; } = string.Empty;
	public bool ShareDiscordOnAccept { get; set; } = true;
	public bool ShareAetherphoneOnAccept { get; set; } = false;
	public Vector2 RecruitmentToastPosition { get; set; } = new Vector2(-1f, -1f);

	public QuickDrawModule QuickModule()
	{
		QuickDrawModule quickDrawModule = QuickDrawModules.FirstOrDefault((QuickDrawModule m) => !m.BuiltIn && m.Category == "Quick Draws");
		if (quickDrawModule != null)
		{
			return quickDrawModule;
		}
		quickDrawModule = new QuickDrawModule
		{
			Name = "My Quick Draws",
			Category = "Quick Draws"
		};
		QuickDrawModules.Insert(0, quickDrawModule);
		return quickDrawModule;
	}

	public void Save()
	{
		Plugin.PluginInterface.SavePluginConfig(this);
	}
}
