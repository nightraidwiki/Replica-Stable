using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Interop.ActionEffect;
using Replica.Engine.Util;

namespace Replica.Engine.Struct;

public struct ActorAbilityInfo
{
	public uint ActionId;

	public IGameObject Source;

	public IGameObject? Target;

	public TargetEffect[] TargetEffects;

	public Angle Rotation;

	public Vector3 Pos;
}
