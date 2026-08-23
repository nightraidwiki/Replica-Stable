using System;
using System.Collections.Generic;
using System.Linq;

namespace Replica.Scripting.Api;

[AttributeUsage(AttributeTargets.Class)]
public class ScriptTypeAttribute : Attribute
{
	public string Name { get; }

	public HashSet<uint> Territorys { get; }

	public string Guid { get; }

	public string Version { get; }

	public string Author { get; }

	public string Note { get; }

	public string UpdateInfo { get; }

	public ScriptTypeAttribute(string guid, string name = "Default Script", uint[]? territorys = null, string version = "0.0.0.1", string author = "Unknown", string note = "", string updateInfo = "")
	{
		Guid = guid;
		Name = name;
		Territorys = territorys?.ToHashSet() ?? new HashSet<uint>();
		Version = version;
		Author = author;
		Note = note;
		UpdateInfo = updateInfo;
	}
}
