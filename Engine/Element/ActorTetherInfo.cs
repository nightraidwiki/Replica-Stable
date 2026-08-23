namespace Replica.Engine.Element;

public readonly struct ActorTetherInfo(uint id, ulong target)
{
	public readonly uint ID = id;

	public readonly ulong Target = target;
}
