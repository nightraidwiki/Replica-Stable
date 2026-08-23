using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Replica.Recruitment.Models;

public static class RecruitmentConstants
{
    public static readonly string[] Regions = ["EU", "NA", "JP", "OCE"];

    public static readonly Dictionary<string, string[]> DatacentersByRegion = new()
    {
        ["EU"] = ["Chaos", "Light"],
        ["NA"] = ["Aether", "Primal", "Crystal", "Dynamis"],
        ["JP"] = ["Elemental", "Gaia", "Mana", "Meteor"],
        ["OCE"] = ["Materia"]
    };

    public static readonly string[] Categories =
    [
        "Ultimate",
        "Savage",
        "Criterion",
        "FC Recruiting",
        "Looking for Friends"
    ];

    public static readonly string[] ContentTypes =
    [
        "All Content",
        "Ultimate",
        "Savage",
        "Criterion",
        "FC Recruiting",
        "Looking for Friends"
    ];

    public static readonly string[] UltimateDuties =
    [
        "Futures Rewritten (FRU)",
        "The Omega Protocol (TOP)",
        "Dragonsong's Reprise (DSR)",
        "The Epic of Alexander (TEA)",
        "The Weapon's Refrain (UWU)",
        "The Unending Coil of Bahamut (UCoB)"
    ];

    public static readonly string[] SavageDuties =
    [
        "M1S - M4S (AAC Light-heavyweight)",
        "M1S", "M2S", "M3S", "M4S",
        "Legacy Savage / Old Tiers"
    ];

    public static readonly string[] Roles =
    [
        "TankMT",
        "TankOT",
        "PureHealer",
        "ShieldHealer",
        "Melee1",
        "Melee2",
        "PhysRanged",
        "Caster"
    ];

    public static readonly string[] RolesDisplay =
    [
        "Main Tank (MT)",
        "Off Tank (OT)",
        "Pure Healer (WHM/AST)",
        "Shield Healer (SCH/SGE)",
        "Melee DPS (M1)",
        "Melee DPS (M2)",
        "Physical Ranged (BRD/MCH/DNC)",
        "Magical Ranged Caster (BLM/SMN/RDM/PCT)"
    ];

    public static readonly Dictionary<string, string[]> JobsByRole = new()
    {
        ["Tank"] = ["PLD", "WAR", "DRK", "GNB"],
        ["PureHealer"] = ["WHM", "AST"],
        ["ShieldHealer"] = ["SCH", "SGE"],
        ["Melee"] = ["MNK", "DRG", "NIN", "SAM", "RPR", "VPR"],
        ["PhysRanged"] = ["BRD", "MCH", "DNC"],
        ["Caster"] = ["BLM", "SMN", "RDM", "PCT"]
    };

    public static readonly string[] AllJobs =
    [
        "PLD", "WAR", "DRK", "GNB",
        "WHM", "AST", "SCH", "SGE",
        "MNK", "DRG", "NIN", "SAM", "RPR", "VPR",
        "BRD", "MCH", "DNC",
        "BLM", "SMN", "RDM", "PCT"
    ];

    public static readonly string[] DaysOfWeek =
    [
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    ];

    public static readonly string[] DaysOfWeekEn =
    [
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    ];

    public static readonly string[] SupportedLanguages = ["EN", "FR", "DE", "JP"];
}

public sealed class PluginsUsed
{
    [JsonPropertyName("bossmod")]
    public bool BossMod { get; set; } = false;

    [JsonPropertyName("splatoon")]
    public bool Splatoon { get; set; } = false;

    [JsonPropertyName("wrath")]
    public bool Wrath { get; set; } = false;

    [JsonPropertyName("rsr")]
    public bool Rsr { get; set; } = false;

    [JsonPropertyName("replica")]
    public bool Replica { get; set; } = true;

    [JsonPropertyName("artisan")]
    public bool Artisan { get; set; } = false;

    [JsonPropertyName("cactbot")]
    public bool Cactbot { get; set; } = false;

    [JsonPropertyName("modbeast")]
    public bool ModBeast { get; set; } = false;
}

public sealed class RosterSlot
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("job")]
    public string? Job { get; set; }
}

public sealed class RecruitmentListing
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("author_discord_id")]
    public string AuthorDiscordId { get; set; } = string.Empty;

    [JsonPropertyName("author_discord_tag")]
    public string AuthorDiscordTag { get; set; } = string.Empty;

    [JsonPropertyName("author_display_name")]
    public string AuthorDisplayName { get; set; } = string.Empty;

    [JsonPropertyName("author_aetherphone")]
    public string? AuthorAetherphone { get; set; }

    [JsonPropertyName("author_avatar_url")]
    public string? AuthorAvatarUrl { get; set; }

    [JsonPropertyName("share_discord_on_accept")]
    public bool ShareDiscordOnAccept { get; set; } = true;

    [JsonPropertyName("share_aetherphone_on_accept")]
    public bool ShareAetherphoneOnAccept { get; set; } = false;

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "Ultimate";

    [JsonPropertyName("target_duty")]
    public string TargetDuty { get; set; } = "Futures Rewritten (FRU)";

    [JsonPropertyName("region")]
    public string Region { get; set; } = "EU";

    [JsonPropertyName("datacenter")]
    public string Datacenter { get; set; } = string.Empty;

    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = ["FR"];

    [JsonPropertyName("progression")]
    public string Progression { get; set; } = string.Empty;

    [JsonPropertyName("schedule_days")]
    public List<string> ScheduleDays { get; set; } = [];

    [JsonPropertyName("schedule_time_start")]
    public string ScheduleTimeStart { get; set; } = "20:45";

    [JsonPropertyName("schedule_time_end")]
    public string ScheduleTimeEnd { get; set; } = "23:00";

    [JsonPropertyName("schedule_timezone")]
    public string ScheduleTimezone { get; set; } = "Europe/Paris";

    [JsonPropertyName("roles_needed")]
    public List<string> RolesNeeded { get; set; } = [];

    [JsonPropertyName("current_roster")]
    public List<RosterSlot> CurrentRoster { get; set; } = [];

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "OPEN";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("bumped_at")]
    public DateTime BumpedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(14);

    [JsonPropertyName("applications")]
    public List<ApplicationItem>? Applications { get; set; }
}

public sealed class CandidateProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("discord_id")]
    public string DiscordId { get; set; } = string.Empty;

    [JsonPropertyName("discord_tag")]
    public string DiscordTag { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("aetherphone")]
    public string? Aetherphone { get; set; }

    [JsonPropertyName("share_discord_on_accept")]
    public bool ShareDiscordOnAccept { get; set; } = true;

    [JsonPropertyName("share_aetherphone_on_accept")]
    public bool ShareAetherphoneOnAccept { get; set; } = false;

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("character_name")]
    public string CharacterName { get; set; } = string.Empty;

    [JsonPropertyName("character_world")]
    public string CharacterWorld { get; set; } = string.Empty;

    [JsonPropertyName("character_datacenter")]
    public string CharacterDatacenter { get; set; } = string.Empty;

    [JsonPropertyName("character_region")]
    public string CharacterRegion { get; set; } = "EU";

    [JsonPropertyName("ilvl")]
    public int Ilvl { get; set; } = 730;

    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = ["FR"];

    [JsonPropertyName("regions_accepted")]
    public List<string> RegionsAccepted { get; set; } = ["EU"];

    [JsonPropertyName("main_jobs")]
    public List<string> MainJobs { get; set; } = [];

    [JsonPropertyName("secondary_jobs")]
    public List<string> SecondaryJobs { get; set; } = [];

    [JsonPropertyName("plugins_used")]
    public PluginsUsed PluginsUsed { get; set; } = new();

    [JsonPropertyName("available_days")]
    public List<string> AvailableDays { get; set; } = [];

    [JsonPropertyName("preferred_time_start")]
    public string PreferredTimeStart { get; set; } = "20:30";

    [JsonPropertyName("preferred_time_end")]
    public string PreferredTimeEnd { get; set; } = "23:30";

    [JsonPropertyName("nights_per_week")]
    public string NightsPerWeek { get; set; } = "3-4";

    [JsonPropertyName("experience")]
    public string Experience { get; set; } = string.Empty;

    [JsonPropertyName("about_me")]
    public string AboutMe { get; set; } = string.Empty;

    [JsonPropertyName("link_fflogs")]
    public string? LinkFflogs { get; set; }

    [JsonPropertyName("link_tomestone")]
    public string? LinkTomestone { get; set; }

    [JsonPropertyName("link_lodestone")]
    public string? LinkLodestone { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ApplicationItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("listing_id")]
    public string ListingId { get; set; } = string.Empty;

    [JsonPropertyName("applicant_user_id")]
    public string ApplicantUserId { get; set; } = string.Empty;

    [JsonPropertyName("applicant_profile_snapshot")]
    public CandidateProfile ApplicantProfileSnapshot { get; set; } = new();

    [JsonPropertyName("applied_as_job")]
    public string AppliedAsJob { get; set; } = string.Empty;

    [JsonPropertyName("applied_as_role")]
    public string AppliedAsRole { get; set; } = string.Empty;

    [JsonPropertyName("custom_message")]
    public string? CustomMessage { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "PENDING"; // PENDING, ACCEPTED, DECLINED, WITHDRAWN

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class ReplicaSession
{
    [JsonPropertyName("auth_token")]
    public string? AuthToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("discord_tag")]
    public string? DiscordTag { get; set; }

    [JsonPropertyName("discord_id")]
    public string? DiscordId { get; set; }

    [JsonPropertyName("discord_avatar_url")]
    public string? DiscordAvatarUrl { get; set; }

    [JsonPropertyName("token_expires_at")]
    public long TokenExpiresAt { get; set; }

    /// <summary>
    /// Static API key generated once by Supabase upon first login.
    /// Used instead of OAuth refresh tokens for permanent silent reconnection.
    /// </summary>
    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }
}
