using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Replica.Engine.Interop;

internal static class Svc
{
	public static IDataManager Data => Plugin.DataManager;

	public static IObjectTable Objects => Plugin.ObjectTable;

	public static IPluginLog Log => Plugin.Log;

	public static ISigScanner SigScanner => Plugin.SigScanner;

	public static IGameInteropProvider Hook => Plugin.GameInterop;

	public static IClientState ClientState => Plugin.ClientState;

	public static IFramework Framework => Plugin.Framework;

	public static ICondition Condition => Plugin.Condition;

	public static IDalamudPluginInterface PluginInterface => Plugin.PluginInterface;

	public static IAddonLifecycle AddonLifecycle => Plugin.AddonLifecycle;
}
