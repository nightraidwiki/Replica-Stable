using System;
using System.Collections.Generic;

namespace Replica.Scripting.Api;

[AttributeUsage(AttributeTargets.Method)]
public class ScriptMethodAttribute : Attribute
{
	public EventTypeEnum EventType { get; set; }

	public string Name { get; set; }

	public Dictionary<string, string> EventCondition { get; set; }

	public bool UserControl { get; set; }

	public uint Suppress { get; set; }

	public ScriptMethodAttribute(EventTypeEnum eventType, string name = "", string[]? eventCondition = null, bool userControl = true, uint suppress = 0u)
	{
		EventType = eventType;
		Name = name;
		UserControl = userControl;
		Suppress = suppress;
		EventCondition = new Dictionary<string, string>();
		string[] array = eventCondition ?? Array.Empty<string>();
		foreach (string text in array)
		{
			int num = text.IndexOf(':');
			string key = ((num != -1) ? text.Substring(0, num) : text);
			string value = ((num != -1) ? text.Substring(num + 1) : string.Empty);
			EventCondition[key] = value;
		}
	}
}
