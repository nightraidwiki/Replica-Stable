namespace Replica.QuickDraws;

public sealed class StatusGate
{
	public StatusGateWho Who { get; set; }

	public bool Have { get; set; } = true;

	public uint StatusId { get; set; }

	public string Name { get; set; } = "";

	public StatusGate Clone()
	{
		return (StatusGate)MemberwiseClone();
	}
}
