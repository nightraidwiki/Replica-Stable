using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Windows;

namespace Replica.Modules.DancingMad;

public class DancingMad : DrawModule
{
	private const string TelegraphLabel = "Hide the game's telegraphs";

	private const string ShockwaveLabel = "Hide shockwave white-out";

	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Ultimate,
		GroupType = GroupType.CFC,
		GroupID = 1094u
	};

	public override string Author => "Null";

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 49746u, 49744u };

	public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (77u, 78u) };

	public override Dictionary<uint, HashSet<uint>> BlockOmenMap
	{
		get
		{
			if (!Plugin.Config.DancingMadBlockTelegraphs)
			{
				return new Dictionary<uint, HashSet<uint>>();
			}
			return new Dictionary<uint, HashSet<uint>> { [0u] = new HashSet<uint> { 47768u, 47771u, 47774u, 47775u, 47776u, 47777u } };
		}
	}

	public override Dictionary<uint, HashSet<string>> BlockOmenPathMap
	{
		get
		{
			HashSet<string> hashSet = new HashSet<string>();
			if (Plugin.Config.DancingMadBlockTelegraphs)
			{
				hashSet.Add("vfx/lockon/eff/m0462trg_a0c.avfx");
				hashSet.Add("vfx/lockon/eff/m0462trg_b0c.avfx");
			}
			if (Plugin.Config.DancingMadBlockShockwave)
			{
				hashSet.Add("vfx/monster/m0462/eff/m0462hide_sp03c0c.avfx");
			}
			if (hashSet.Count != 0)
			{
				return new Dictionary<uint, HashSet<string>> { [0u] = hashSet };
			}
			return new Dictionary<uint, HashSet<string>>();
		}
	}

	public override bool HasConfig => true;

	public override void DrawConfig()
	{
		float columnX = StratUI.OptionColumn("Hide the game's telegraphs", "Hide shockwave white-out");
		bool v = Plugin.Config.DancingMadBlockTelegraphs;
		StratUI.OptionLabel("Hide the game's telegraphs", columnX, "Blocks the native ground markers and head lock-ons this module redraws,\nso you don't get both at once. Turn off to keep the originals.");
		if (ImGui.Checkbox("##dm_telegraphs", ref v))
		{
			Plugin.Config.DancingMadBlockTelegraphs = v;
			Plugin.Config.Save();
			VfxBlocker.ClearSyncedBlocks();
		}
		bool v2 = Plugin.Config.DancingMadBlockShockwave;
		StratUI.OptionLabel("Hide shockwave white-out", columnX, "Hides the fullscreen white-out during the Ultimate Shockwave.");
		if (ImGui.Checkbox("##dm_shockwave", ref v2))
		{
			Plugin.Config.DancingMadBlockShockwave = v2;
			Plugin.Config.Save();
			VfxBlocker.ClearSyncedBlocks();
		}
	}
}
