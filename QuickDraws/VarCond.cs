namespace Replica.QuickDraws;

public sealed class VarCond
{
	public string Name { get; set; } = "";

	public NumOp Op { get; set; }

	public string Value { get; set; } = "";

	public bool Numeric { get; set; }

	public VarCond Clone()
	{
		return (VarCond)MemberwiseClone();
	}
}
