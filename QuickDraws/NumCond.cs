namespace Replica.QuickDraws;

public sealed class NumCond
{
	public NumField Field { get; set; }

	public NumOp Op { get; set; } = NumOp.Ge;

	public float Value { get; set; }

	public NumCond Clone()
	{
		return (NumCond)MemberwiseClone();
	}
}
