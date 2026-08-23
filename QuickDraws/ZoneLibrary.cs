using System;
using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace Replica.QuickDraws;

public static class ZoneLibrary
{
	public readonly record struct Zone(uint TerritoryId, string Name);

	private static List<Zone>? _all;

	private static Dictionary<uint, string>? _names;

	private static Dictionary<uint, string>? _cats;

	private static void Ensure()
	{
		if (_all != null)
		{
			return;
		}
		List<Zone> list = new List<Zone>(2048);
		Dictionary<uint, string> dictionary = new Dictionary<uint, string>();
		Dictionary<uint, string> dictionary2 = new Dictionary<uint, string>();
		foreach (ContentFinderCondition item in Plugin.DataManager.GetExcelSheet<ContentFinderCondition>())
		{
			string text = item.Name.ExtractText();
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			uint rowId = item.TerritoryType.RowId;
			if (rowId != 0)
			{
				list.Add(new Zone(rowId, text));
				dictionary[rowId] = text;
				string text2 = "";
				try
				{
					text2 = item.ContentType.Value.Name.ExtractText();
				}
				catch
				{
				}
				dictionary2[rowId] = (string.IsNullOrWhiteSpace(text2) ? "Other" : text2);
			}
		}
		_all = list;
		_names = dictionary;
		_cats = dictionary2;
	}

	public static string CategoryOf(uint territoryId)
	{
		Ensure();
		if (!_cats.TryGetValue(territoryId, out string value))
		{
			return "Other";
		}
		return value;
	}

	public static string NameOf(uint territoryId)
	{
		Ensure();
		if (territoryId == 0)
		{
			return "Open world / unknown";
		}
		if (_names.TryGetValue(territoryId, out string value))
		{
			return value;
		}
		try
		{
			string text = Plugin.DataManager.GetExcelSheet<TerritoryType>().GetRowOrDefault(territoryId)?.PlaceName.Value.Name.ExtractText();
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		catch
		{
		}
		return $"Zone {territoryId}";
	}

	public static List<Zone> Search(string query, int limit = 30)
	{
		Ensure();
		List<Zone> list = new List<Zone>();
		if (string.IsNullOrWhiteSpace(query))
		{
			return list;
		}
		foreach (Zone item in _all)
		{
			if (item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				list.Add(item);
				if (list.Count >= limit)
				{
					break;
				}
			}
		}
		return list;
	}
}
