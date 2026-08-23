using System;
using System.Collections.Generic;
using Lumina.Excel.Sheets;

namespace Replica.QuickDraws;

public static class GameLibrary
{
	public readonly record struct Entry(uint Id, string Name, uint Icon, bool IsStatus);

	private static List<Entry>? _all;

	private static void Ensure()
	{
		if (_all != null)
		{
			return;
		}
		List<Entry> list = new List<Entry>(8192);
		foreach (Lumina.Excel.Sheets.Action action in Plugin.Actions)
		{
			string text = action.Name.ExtractText();
			if (!string.IsNullOrWhiteSpace(text))
			{
				list.Add(new Entry(action.RowId, text, action.Icon, IsStatus: false));
			}
		}
		foreach (Status status in Plugin.Statuses)
		{
			string text2 = status.Name.ExtractText();
			if (!string.IsNullOrWhiteSpace(text2))
			{
				list.Add(new Entry(status.RowId, text2, status.Icon, IsStatus: true));
			}
		}
		_all = list;
	}

	public static List<Entry> Search(string query, int limit = 40)
	{
		Ensure();
		List<Entry> list = new List<Entry>(limit);
		if (string.IsNullOrWhiteSpace(query))
		{
			return list;
		}
		List<Entry> list2 = new List<Entry>();
		List<Entry> list3 = new List<Entry>();
		List<Entry> list4 = new List<Entry>();
		foreach (Entry item in _all)
		{
			int num = item.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase);
			if (num >= 0)
			{
				if (item.Name.Length == query.Length)
				{
					list2.Add(item);
				}
				else if (num == 0)
				{
					list3.Add(item);
				}
				else
				{
					list4.Add(item);
				}
			}
		}
		List<Entry>[] array = new List<Entry>[3] { list2, list3, list4 };
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Entry item2 in array[i])
			{
				if (list.Count >= limit)
				{
					return list;
				}
				list.Add(item2);
			}
		}
		return list;
	}
}
