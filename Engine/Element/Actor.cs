namespace Replica.Engine.Element;

public sealed class Actor
{
	public ulong GameObjectID;

	public ActorTetherInfo Tether;

	public Actor(ulong id)
	{
		GameObjectID = id;
	}
}
