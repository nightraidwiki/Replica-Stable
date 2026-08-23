using System;
using System.Collections.Generic;

namespace Replica.Scripting.Host;

public sealed class ScriptRuntime
{
	private sealed class TickEntry
	{
		public string ScriptGuid = "";

		public Action Action = delegate
		{
		};
	}

	private readonly Dictionary<string, TickEntry> _ticks = new Dictionary<string, TickEntry>();

	private readonly object _gate = new object();

	public ScriptDrawBridge Draw { get; } = new ScriptDrawBridge();

	public void Speak(string message)
	{
		if (!string.IsNullOrWhiteSpace(message))
		{
			Plugin.ChatGui.Print(message);
		}
	}

	public void ShowText(string message)
	{
		if (!string.IsNullOrWhiteSpace(message))
		{
			Plugin.ChatGui.Print(message);
		}
	}

	public string RegisterTick(string scriptGuid, Action action, bool deactivateExisting)
	{
		if (action == null)
		{
			return "";
		}
		lock (_gate)
		{
			if (deactivateExisting)
			{
				RemoveByScript(scriptGuid);
			}
			string text = Guid.NewGuid().ToString("N");
			_ticks[text] = new TickEntry
			{
				ScriptGuid = scriptGuid,
				Action = action
			};
			return text;
		}
	}

	public void UnregisterTick(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return;
		}
		lock (_gate)
		{
			_ticks.Remove(id);
		}
	}

	public void ClearTicks(string scriptGuid)
	{
		lock (_gate)
		{
			RemoveByScript(scriptGuid);
		}
	}

	public void ClearAllTicks()
	{
		lock (_gate)
		{
			_ticks.Clear();
		}
	}

	public void Tick()
	{
		List<TickEntry> list;
		lock (_gate)
		{
			if (_ticks.Count == 0)
			{
				return;
			}
			list = new List<TickEntry>(_ticks.Values);
		}
		foreach (TickEntry item in list)
		{
			try
			{
				item.Action();
			}
			catch (Exception ex)
			{
				Plugin.Log.Debug("[script tick] " + ex.Message);
			}
		}
	}

	private void RemoveByScript(string scriptGuid)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, TickEntry> tick in _ticks)
		{
			if (tick.Value.ScriptGuid == scriptGuid)
			{
				list.Add(tick.Key);
			}
		}
		foreach (string item in list)
		{
			_ticks.Remove(item);
		}
	}
}
