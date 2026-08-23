namespace Replica.Engine.Bridge.BossMod.Vfx;

public readonly record struct MirrorKey(
    int Kind,
    ulong ActorHash,
    float Ox,
    float Oz,
    float Rot,
    float P1,
    float P2,
    float P3);
