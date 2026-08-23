using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Replica.Logging;

namespace Replica.QuickDraws;

public sealed class FightCatalog
{
	public enum Kind : byte
	{
		Cast,
		Status,
		Headmarker,
		Tether
	}

	public sealed class Entry
	{
		public Kind Kind { get; set; }

		public uint Id { get; set; }

		public string Name { get; set; } = "";

		public uint Icon { get; set; }
	}

	private sealed class ZoneData
	{
		public uint Territory { get; set; }

		public List<Entry> Entries { get; set; } = new List<Entry>();
	}

	private sealed class Store
	{
		public int Version { get; set; }

		public List<ZoneData> Zones { get; set; } = new List<ZoneData>();
	}

	private const int StoreVersion = 1;

	private readonly Dictionary<uint, Dictionary<long, Entry>> _byZone = new Dictionary<uint, Dictionary<long, Entry>>();

	private readonly string _path;

	private readonly IPluginLog _log;

	private bool _dirty;

	private DateTime _lastSave = DateTime.MinValue;

	public FightCatalog(string dir, IPluginLog log)
	{
		_log = log;
		_path = Path.Combine(dir, "catalog.json");
		Load();
	}

	private static long Key(Kind kind, uint id)
	{
		return (long)(((ulong)kind << 32) | id);
	}

	private static bool SourceIsHostile(uint srcId)
	{
		if (srcId == 0)
		{
			return false;
		}
		IGameObject gameObject = Plugin.ObjectTable.SearchById(srcId);
		if (gameObject == null)
		{
			return true;
		}
		if (gameObject.ObjectKind == ObjectKind.Pc)
		{
			return false;
		}
		if (gameObject is IBattleNpc battleNpc)
		{
			byte battleNpcKind = (byte)battleNpc.BattleNpcKind;
			bool flag = (((uint)(battleNpcKind - 2) <= 1u || battleNpcKind == 9) ? true : false);
			return !flag;
		}
		return true;
	}

	public void Record(LogEvent e)
	{
		if (Plugin.ConfigStatic != null && !Plugin.ConfigStatic.LogActions)
		{
			return;
		}
		uint territoryType = Plugin.ClientState.TerritoryType;
		if (territoryType == 0)
		{
			return;
		}
		Kind kind;
		switch (e.Kind)
		{
		default:
			return;
		case LogKind.CastStart:
			if (e.SourceKind != ActorKind.Enemy)
			{
				return;
			}
			kind = Kind.Cast;
			break;
		case LogKind.StatusGain:
			if (!SourceIsHostile(e.SourceId))
			{
				return;
			}
			kind = Kind.Status;
			break;
		case LogKind.Headmarker:
			kind = Kind.Headmarker;
			break;
		case LogKind.Tether:
			kind = Kind.Tether;
			break;
		}
		if (e.DataId != 0 || !string.IsNullOrEmpty(e.Name))
		{
			if (!_byZone.TryGetValue(territoryType, out Dictionary<long, Entry> value))
			{
				value = new Dictionary<long, Entry>();
				_byZone[territoryType] = value;
			}
			long key = Key(kind, e.DataId);
			if (!value.ContainsKey(key))
			{
				value[key] = new Entry
				{
					Kind = kind,
					Id = e.DataId,
					Name = e.Name,
					Icon = e.IconId
				};
				_dirty = true;
			}
		}
	}

	public List<uint> Zones()
	{
		return new List<uint>(_byZone.Keys);
	}

	public List<Entry> Entries(uint territory)
	{
		if (!_byZone.TryGetValue(territory, out Dictionary<long, Entry> value))
		{
			return new List<Entry>();
		}
		return new List<Entry>(value.Values);
	}

	public int Count(uint territory)
	{
		if (!_byZone.TryGetValue(territory, out Dictionary<long, Entry> value))
		{
			return 0;
		}
		return value.Count;
	}

	public void Clear(uint territory)
	{
		if (_byZone.Remove(territory))
		{
			_dirty = true;
			Save();
		}
	}

	public void MaybeSave()
	{
		if (_dirty && !((DateTime.Now - _lastSave).TotalSeconds < 5.0))
		{
			Save();
		}
	}

	public void Save()
	{
		try
		{
			Store store = new Store
			{
				Version = 1
			};
			foreach (var (territory, dictionary2) in _byZone)
			{
				store.Zones.Add(new ZoneData
				{
					Territory = territory,
					Entries = new List<Entry>(dictionary2.Values)
				});
			}
			File.WriteAllText(_path, JsonSerializer.Serialize(store));
			_dirty = false;
			_lastSave = DateTime.Now;
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] catalog save: " + ex.Message);
		}
	}

	private void Load()
	{
		try
		{
			if (!File.Exists(_path))
			{
				return;
			}
			Store store = JsonSerializer.Deserialize<Store>(File.ReadAllText(_path));
			if (store?.Zones == null)
			{
				return;
			}
			if (store.Version < 1)
			{
				_dirty = true;
				return;
			}
			foreach (ZoneData zone in store.Zones)
			{
				Dictionary<long, Entry> dictionary = new Dictionary<long, Entry>();
				foreach (Entry entry in zone.Entries)
				{
					dictionary[Key(entry.Kind, entry.Id)] = entry;
				}
				_byZone[zone.Territory] = dictionary;
			}
		}
		catch (Exception ex)
		{
			_log.Debug("[Replica] catalog load: " + ex.Message);
		}
	}
}
