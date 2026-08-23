using System;

namespace Replica.Scripting.Api;

[AttributeUsage(AttributeTargets.Property)]
public class ScriptDataAttribute : Attribute
{
	public string Name { get; }

	public ScriptDataAttribute(string name = "")
	{
		Name = name;
	}
}
