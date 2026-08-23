using System.Numerics;

namespace Replica.Engine.Bridge.BossMod.Core;

public static class BossModConstants
{
    public const int K_CIRCLE  = 1;
    public const int K_CONE    = 2;
    public const int K_RECT    = 3;
    public const int K_DONUT   = 4;
    public const int K_CROSS   = 5;
    public const int K_DONUTS  = 6;
    public const int K_CAPSULE = 7;
    public const int K_TOWER   = 8;
    public const int K_STACK   = 9;
    public const int K_TETHER  = 10;
    public const int K_GAZE    = 11;
    public const int K_PAIR    = 12;
    public const int K_PARTY4  = 13;

    public static readonly Vector4 ColDanger     = new(1f, 0.28f, 0.28f, 1.5f);
    public static readonly Vector4 ColSafe       = new(0.15f, 0.95f, 0.25f, 1.5f);
    public static readonly Vector4 ColBait       = new(1f, 0.55f, 0.10f, 1.5f);
    public static readonly Vector4 ColStack      = new(1f, 1f, 1f, 1.5f);          // White 2-man pair stack
    public static readonly Vector4 ColPartyStack = new(1f, 0.85f, 0.15f, 1.5f);     // Gold/Yellow 4-man & raid stack
    public static readonly Vector4 ColTower      = new(0.20f, 0.80f, 1f, 1.5f);
}
