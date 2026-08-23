using System;

namespace Replica.Scripting.Api;

[Flags]
public enum ScaleMode
{
	None = 0,
	XByDistance = 1,
	YByDistance = 2,
	ByTime = 4,
	XByTime = 8,
	YByTime = 0x10
}
