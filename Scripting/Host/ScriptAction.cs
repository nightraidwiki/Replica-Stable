using System;
using System.Reflection;
using Replica.Scripting.Api;

namespace Replica.Scripting.Host;

public sealed class ScriptAction
{
	public required string MethodName { get; init; }

	public required ScriptMethodAttribute Attribute { get; init; }

	public required MethodInfo Method { get; init; }

	public DateTime LastFired { get; set; } = DateTime.MinValue;

	public string Key
	{
		get
		{
			if (Attribute.Name.Length <= 0)
			{
				return MethodName;
			}
			return Attribute.Name;
		}
	}
}
