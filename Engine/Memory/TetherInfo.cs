namespace Replica.Engine.Memory;

public class TetherInfo(uint from, uint to, int tetherID)
{
	public uint From { get; set; } = from;

	public uint To { get; set; } = to;

	public int TetherID { get; set; } = tetherID;
}
