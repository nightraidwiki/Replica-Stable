using System.Collections.Generic;
using System.Reflection;
using Replica.Scripting.Api;

namespace Replica.Scripting.Host;

public sealed class LoadedScript
{
	public required string Guid { get; init; }

	public required string Name { get; init; }

	public required string Version { get; init; }

	public required string Author { get; init; }

	public required string Note { get; init; }

	public required HashSet<uint> Territorys { get; init; }

	public required string SourcePath { get; init; }

	public required object Instance { get; init; }

	public required ScriptAccessory Accessory { get; init; }

	public required List<ScriptAction> Actions { get; init; }

	public MethodInfo? InitMethod { get; init; }

	public bool MatchesTerritory(uint territory)
	{
		if (Territorys.Count != 0)
		{
			return Territorys.Contains(territory);
		}
		return true;
	}
}
