using System;
using System.Collections.Generic;

namespace Replica.Strats;

public sealed class StratBranch
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public string Name { get; set; } = "Variant";

	public BranchDetect Detect { get; set; }

	public string BossName { get; set; } = "";

	public uint BossId { get; set; }

	public Compass BossSide { get; set; } = Compass.S;

	public uint StatusId { get; set; }

	public string StatusName { get; set; } = "";

	public bool RequireAll { get; set; } = true;

	public List<StratCondition> Conditions { get; set; } = new List<StratCondition>();

	public List<RoleSpot> Spots { get; set; } = new List<RoleSpot>();

	public StratBranch Clone()
	{
		StratBranch obj = (StratBranch)MemberwiseClone();
		obj.Spots = Spots.ConvertAll((RoleSpot x) => x.Clone());
		obj.Conditions = Conditions.ConvertAll((StratCondition x) => x.Clone());
		return obj;
	}
}
