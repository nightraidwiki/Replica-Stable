using System;
using System.Collections;
using System.Collections.Generic;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Processors;

public enum BossModStackKind
{
    Pair,       // 2-player stack (e.g. 4 pairs or 2 pairs, minSize=2, maxSize=2)
    Party4,     // 4-player stack (e.g. Light Party stack, G1/G2, Support/DPS, minSize=4)
    Raid8       // 8-player stack (Full raid stack)
}

public static class BossModStackClassifier
{
    // Explicit module / component name overrides for known raid mechanics
    private static readonly Dictionary<string, BossModStackKind> Overrides = new(StringComparer.OrdinalIgnoreCase)
    {
        // FRU (Futures Rewritten Ultimate)
        { "P1CyclonicBreak", BossModStackKind.Pair },
        { "P1UtopianSky", BossModStackKind.Party4 },
        { "P2DiamondDust", BossModStackKind.Party4 },
        { "P2Banish", BossModStackKind.Pair },
        { "P2BanishIII", BossModStackKind.Pair },
        { "DarklitDragonsong", BossModStackKind.Party4 },
        { "AkhMorn", BossModStackKind.Party4 },
        { "AkhAfah", BossModStackKind.Party4 },

        // TOP / TEA / DSR
        { "P2PartySynergy", BossModStackKind.Pair },
        { "P3Wormhole", BossModStackKind.Pair },
        { "P4FateCalibrationBeta", BossModStackKind.Party4 },

        // Savage raids (M1S-M4S, M5S-M8S, M9S-M12S)
        { "DropSplashOfVenom", BossModStackKind.Pair },
        { "MidnightSabbath", BossModStackKind.Party4 },
        { "ElectropeEdge", BossModStackKind.Party4 },
        { "Diveboom", BossModStackKind.Party4 },
        { "BombarianSpecial", BossModStackKind.Party4 },
        { "ArchaicRockbreaker", BossModStackKind.Party4 },
        { "DaemoniacBonds", BossModStackKind.Party4 },
        { "WindsHoly", BossModStackKind.Party4 },
        { "ZenithStrike", BossModStackKind.Party4 },
        { "MimicCellSecond", BossModStackKind.Party4 },
    };

    public static void RegisterOverride(string componentOrMechanicName, BossModStackKind kind)
    {
        Overrides[componentOrMechanicName] = kind;
    }

    public static BossModStackKind Classify(object? stackItem, object? comp, int totalStacksCount)
    {
        if (comp != null)
        {
            string compName = comp.GetType().Name;
            foreach (var kv in Overrides)
            {
                if (compName.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kv.Value;
                }
            }

            if (compName.Contains("Pair", StringComparison.OrdinalIgnoreCase))
            {
                return BossModStackKind.Pair;
            }
            if (compName.Contains("LightParty", StringComparison.OrdinalIgnoreCase) ||
                compName.Contains("PartyStack", StringComparison.OrdinalIgnoreCase) ||
                compName.Contains("GroupStack", StringComparison.OrdinalIgnoreCase) ||
                compName.Contains("HealerStack", StringComparison.OrdinalIgnoreCase))
            {
                return BossModStackKind.Party4;
            }
        }

        if (stackItem != null)
        {
            int minSize = Ff(GetField(stackItem, "MinSize"), 2) is float ms ? (int)ms : 2;
            int maxSize = Ff(GetField(stackItem, "MaxSize"), int.MaxValue) is float mx ? (int)mx : int.MaxValue;

            if (minSize >= 6 || maxSize == 8)
            {
                return BossModStackKind.Raid8;
            }

            if (minSize >= 4 || maxSize == 4)
            {
                return BossModStackKind.Party4;
            }

            if (maxSize == 2)
            {
                return BossModStackKind.Pair;
            }
        }

        // Automatic deduction based on total concurrent stack targets
        if (totalStacksCount >= 4)
        {
            // 4 stacks across 8 party members = 2 players per stack (Pairs)
            return BossModStackKind.Pair;
        }
        if (totalStacksCount == 2)
        {
            // 2 stacks across 8 party members = 4 players per stack (Light Party 4-man stack)
            return BossModStackKind.Party4;
        }

        // 1 stack = Full party / general stack
        return BossModStackKind.Raid8;
    }
}
