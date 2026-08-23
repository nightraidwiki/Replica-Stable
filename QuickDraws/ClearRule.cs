namespace Replica.QuickDraws;

public sealed class ClearRule
{
	public bool Enabled { get; set; }

	public FollowUpOn On { get; set; } = FollowUpOn.Cast;

	public float Seconds { get; set; } = 12f;

	public string Pattern { get; set; } = "";

	public uint DataId { get; set; }

	public bool MatchById { get; set; }

	public bool OnlyOnSelf { get; set; }

	public ClearRule Clone()
	{
		return (ClearRule)MemberwiseClone();
	}
}
