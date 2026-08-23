namespace Replica.Engine.Bridge.BossMod.Vfx;

public readonly record struct SyntheticAOE(
    object Origin,
    object? Rotation,
    object Shape,
    bool IsSafe);
