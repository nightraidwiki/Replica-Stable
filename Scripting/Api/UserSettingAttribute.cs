using System;

namespace Replica.Scripting.Api;

[AttributeUsage(AttributeTargets.Property)]
public class UserSettingAttribute : Attribute
{
	public string Name { get; }

	public UserSettingAttribute(string name = "")
	{
		Name = name;
	}
}
