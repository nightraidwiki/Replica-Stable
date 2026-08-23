using System.Collections.Generic;
using System.Reflection;

namespace Replica.Scripting.Host;

public sealed class CompileResult
{
	public Assembly? Assembly { get; init; }

	public List<string> Errors { get; init; } = new List<string>();

	public bool Ok => Assembly != null;
}
