namespace Replica.Strats;

public sealed class StratCondition
{
	public CondKind Kind { get; set; }

	public bool Negate { get; set; }

	public uint StatusId { get; set; }

	public string StatusName { get; set; } = "";

	public RoleCat Role { get; set; }

	public Compass BossSide { get; set; }

	public uint BossId { get; set; }

	public string BossName { get; set; } = "";

	public uint TetherId { get; set; }

	public string TetherName { get; set; } = "";

	public StratCondition Clone()
	{
		return (StratCondition)MemberwiseClone();
	}
}
