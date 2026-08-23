using System.Reflection;

namespace Replica;

public static class Changelog
{
	public static string Version => Plugin.PluginInterface?.Manifest.AssemblyVersion?.ToString() ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";

	public const string Title = "What's new";

	public static readonly string[] Notes = new string[]
	{
		"release of the public version"
	};
}
