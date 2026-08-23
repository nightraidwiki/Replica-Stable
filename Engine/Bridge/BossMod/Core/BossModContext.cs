using Dalamud.Game.ClientState.Objects.SubKinds;

namespace Replica.Engine.Bridge.BossMod.Core;

public readonly struct BossModContext
{
    public Plugin Plugin { get; }
    public IPlayerCharacter LocalPlayer { get; }
    public float GroundY { get; }
    public int PcSlot { get; }
    public object? PcActor { get; }
    public object Module { get; }

    public BossModContext(
        Plugin plugin,
        IPlayerCharacter localPlayer,
        float groundY,
        int pcSlot,
        object? pcActor,
        object module)
    {
        Plugin = plugin;
        LocalPlayer = localPlayer;
        GroundY = groundY;
        PcSlot = pcSlot;
        PcActor = pcActor;
        Module = module;
    }
}
