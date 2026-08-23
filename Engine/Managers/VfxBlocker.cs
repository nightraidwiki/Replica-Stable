using System.Collections.Generic;
using Lumina.Excel.Sheets;
using Replica.Engine.Helper;
using Replica.Engine.Interop;

namespace Replica.Engine.Managers;

public static class VfxBlocker
{
	public static readonly List<string> BlockedPaths = new List<string>();

	private static readonly HashSet<string> Synced = new HashSet<string>();

	private static uint _lastWeather;

	private static uint _lastGroupId;

	public static void Dispose()
	{
		ClearSyncedBlocks();
	}

	public static void SyncOmenBlocks(IEnumerable<Dictionary<uint, HashSet<uint>>> blockOmenMaps, IEnumerable<Dictionary<uint, HashSet<string>>> blockOmenPathMaps, uint weatherId, uint moduleGroupId)
	{
		if (weatherId == _lastWeather && moduleGroupId == _lastGroupId)
		{
			return;
		}
		foreach (string item in Synced)
		{
			BlockedPaths.Remove(item);
		}
		Synced.Clear();
		_lastWeather = weatherId;
		_lastGroupId = moduleGroupId;
		foreach (Dictionary<uint, HashSet<uint>> blockOmenMap in blockOmenMaps)
		{
			if (blockOmenMap.Count == 0 || (!blockOmenMap.TryGetValue(weatherId, out var value) && !blockOmenMap.TryGetValue(0u, out value)))
			{
				continue;
			}
			foreach (uint item2 in value)
			{
				Action row = Svc.Data.GetExcelSheet<Action>().GetRow(item2);
				if (row.Omen.IsValid)
				{
					string text = row.Omen.Value.Path.ExtractText();
					if (!string.IsNullOrEmpty(text))
					{
						AddBlocked(text.Omen());
					}
				}
			}
			break;
		}
		foreach (Dictionary<uint, HashSet<string>> blockOmenPathMap in blockOmenPathMaps)
		{
			if (blockOmenPathMap.Count == 0 || (!blockOmenPathMap.TryGetValue(weatherId, out var value2) && !blockOmenPathMap.TryGetValue(0u, out value2)))
			{
				continue;
			}
			{
				foreach (string item3 in value2)
				{
					AddBlocked(item3);
				}
				break;
			}
		}
	}

	public static void ClearSyncedBlocks()
	{
		foreach (string item in Synced)
		{
			BlockedPaths.Remove(item);
		}
		Synced.Clear();
		_lastWeather = 0u;
		_lastGroupId = 0u;
	}

	private static void AddBlocked(string path)
	{
		if (!string.IsNullOrEmpty(path) && !Synced.Contains(path))
		{
			Synced.Add(path);
			if (!BlockedPaths.Contains(path))
			{
				BlockedPaths.Add(path);
			}
		}
	}
}
