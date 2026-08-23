namespace Replica.QuickDraws;

public sealed class VarAction
{
	public string Name { get; set; } = "";

	public VarOp Op { get; set; }

	public string Value { get; set; } = "1";

	public VarAction Clone()
	{
		return (VarAction)MemberwiseClone();
	}
}
