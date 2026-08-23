using System.Collections.Generic;

namespace Replica.Engine;

internal static class FightDisplayNames
{
	private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
	{
		["BlackCat"] = "M1S — Black Cat",
		["HoneyBLovely"] = "M2S — Honey B. Lovely",
		["BruteBomber"] = "M3S — Brute Bomber",
		["WickedThunder"] = "M4S — Wicked Thunder",
		["DancingGreen"] = "M5S — Dancing Green",
		["SugarRiot"] = "M6S — Sugar Riot",
		["BruteAbombinator"] = "M7S — Brute Abombinator",
		["HowlingBlade"] = "M8S — Howling Blade",
		["FatalBeauty"] = "M9S — Fatal Beauty",
		["LimitBrothers"] = "M10S — Limit Brothers",
		["Tyrant"] = "M11S — Tyrant",
		["Lindblum"] = "M12S — Lindblum",
		["Athena"] = "P12S — Athena",
		["DancingMad"] = "UMAD — Dancing Mad",
		["UWU"] = "UWU — Ultima Weapon",
		["UnendingCoil"] = "UCoB — Unending Coil",
		["TEA"] = "TEA — Alexander",
		["DSR"] = "DSR — Dragonsong",
		["TOP"] = "TOP — Omega",
		["FRU"] = "FRU — Futures Rewritten",
		["CloudOfDarkness"] = "CoD — Cloud of Darkness",
		["CloudofDarknessModule"] = "CoD — Cloud of Darkness",
		["JeunoArc1"] = "Alliance — Jeuno Arc 1",
		["SanDoriaArc2"] = "Alliance — San d'Oria Arc 2",
		["M1"] = "M1 — Black Cat",
		["M2"] = "M2 — Honey B. Lovely",
		["M3"] = "M3 — Brute Bomber",
		["M4"] = "M4 — Wicked Thunder",
		["M5"] = "M5 — Dancing Green",
		["M6"] = "M6 — Sugar Riot",
		["M7"] = "M7 — Brute Abombinator",
		["M8"] = "M8 — Howling Blade",
		["PilgrimsTraverse"] = "Deep Dungeon — Pilgrims Traverse",
		["Alexandria"] = "Dungeon — Alexandria",
		["AlexandriaDt"] = "Dungeon — Alexandria Dt",
		["CastrumMeridianum"] = "Dungeon — Castrum Meridianum",
		["Ihuykatumu"] = "Dungeon — Ihuykatumu",
		["LunarSubterrane"] = "Dungeon — Lunar Subterrane",
		["Origenics"] = "Dungeon — Origenics",
		["Praetorium"] = "Dungeon — Praetorium",
		["SkydeepCenote"] = "Dungeon — Skydeep Cenote",
		["StrayboroughEw"] = "Dungeon — Strayborough Ew",
		["TenderValley"] = "Dungeon — Tender Valley",
		["TheMesoTerminal"] = "Dungeon — The Meso Terminal",
		["Vanguard"] = "Dungeon — Vanguard",
		["WorqorLarDor"] = "Dungeon — Worqor Lar Dor",
		["BarbaricciaEx"] = "EX — Barbariccia",
		["Everkeep"] = "EX — Everkeep",
		["GolbezEx"] = "EX — Golbez",
		["LockWyvernEx"] = "EX — Lock Wyvern",
		["QueenEternalEx"] = "EX — Sphene",
		["SpheneDarkEx"] = "EX — Sphene (Dark)",
		["Valigarmanda"] = "EX — Valigarmanda",
		["Zelenia"] = "EX — Zelenia",
		["Zeromus"] = "EX — Zeromus",
		["CE103WithExtremePrejudice"] = "Foray — CE103With Extreme Prejudice",
		["CE105CrawlingDeath"] = "Foray — CE105Crawling Death",
		["CE106TrialByClaw"] = "Foray — CE106Trial By Claw",
		["CE107Unbridled"] = "Foray — CE107Unbridled",
		["CE110FlameOfDusk"] = "Foray — CE110Flame Of Dusk",
		["CE112EternalWatch"] = "Foray — CE112Eternal Watch",
		["TheForkedTower"] = "Foray — The Forked Tower",
		["A4"] = "Raid — A4",
		["A4S"] = "Raid — A4S",
		["E11"] = "Raid — E11",
		["E2"] = "Raid — E2",
		["E3"] = "Raid — E3",
		["P1N"] = "Raid — P1N",
		["P1NHaunted"] = "Raid — P1NHaunted",
		["P3N"] = "Raid — P3N",
		["P4N"] = "Raid — P4N",
		["P4NHaunted"] = "Raid — P4NHaunted",
		["CenoteJaJa"] = "Treasure Hunt — Cenote Ja Ja",
		["DiamondWeapon"] = "Trial — Diamond Weapon",
		["EmeraldWeapon"] = "Trial — Emerald Weapon",
		["Manderville"] = "Trial — Manderville",
		["UltimaWeapon"] = "Trial — Ultima Weapon",
		["theGreatHunt"] = "Trial — the Great Hunt",
		["TsukuyomiUnreal"] = "Unreal — Tsukuyomi Unreal",
		["AloaloVc"] = "Variant — Aloalo Vc",
		["ShishuDeep"] = "Variant — Shishu Deep",
		["ShishuVc"] = "Variant — Shishu Vc"
	};

	public static string For(string key)
	{
		if (!Names.TryGetValue(key, out string value))
		{
			return key;
		}
		return value;
	}
}
