using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Replica.Engine;

internal static class FightClientState
{
	public readonly struct EnmityEntry(ulong entityId, int enmity)
	{
		public readonly ulong EntityId = entityId;

		public readonly int Enmity = enmity;
	}

	public readonly struct TargetEnmity(ulong targetId, EnmityEntry[] entries)
	{
		public readonly ulong TargetId = targetId;

		public readonly EnmityEntry[] Entries = entries;
	}

	public const int NumEnmityTargets = 32;

	private static readonly Dictionary<ulong, EnmityEntry[]> EnmityByTarget = new Dictionary<ulong, EnmityEntry[]>();

	public static TargetEnmity CurrentTargetEnmity { get; private set; } = new TargetEnmity(0uL, new EnmityEntry[32]);

	public static bool TryGetEnmity(ulong targetId, out EnmityEntry[] entries)
	{
		return EnmityByTarget.TryGetValue(targetId, out entries);
	}

	public static void ClearEnmity()
	{
		EnmityByTarget.Clear();
	}

	public unsafe static void PollEnmity()
	{
		UIState* ptr = UIState.Instance();
		if (ptr != null)
		{
			ref Hate reference = ref ptr->Hate;
			uint hateTargetId = reference.HateTargetId;
			EnmityEntry[] array = new EnmityEntry[32];
			int num = Math.Min(reference.HateArrayLength, 32);
			for (int i = 0; i < num; i++)
			{
				array[i] = new EnmityEntry(reference.HateInfo[i].EntityId, reference.HateInfo[i].Enmity);
			}
			if (hateTargetId != 0)
			{
				EnmityByTarget[hateTargetId] = array;
			}
			if (hateTargetId != CurrentTargetEnmity.TargetId || !((ReadOnlySpan<EnmityEntry>)array.AsSpan()).SequenceEqual((ReadOnlySpan<EnmityEntry>)CurrentTargetEnmity.Entries.AsSpan(), (IEqualityComparer<EnmityEntry>?)null))
			{
				CurrentTargetEnmity = new TargetEnmity(hateTargetId, array);
			}
		}
	}
}
