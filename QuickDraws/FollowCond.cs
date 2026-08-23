namespace Replica.QuickDraws;

public sealed class FollowCond
{
	public string Pattern { get; set; } = "";

	public uint DataId { get; set; }

	public bool MatchById { get; set; }

	public bool OnlyOnSelf { get; set; } = true;

	public bool UseRegex { get; set; }

	public SourceFilter Source { get; set; }

	public RoleFilter SourceRole { get; set; }

	public RoleFilter TargetRole { get; set; }

	public FollowCond Clone()
	{
		return (FollowCond)MemberwiseClone();
	}
}
