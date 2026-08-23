using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Replica.Engine.Hacks;

public static class GameCommands
{
	public const int FlagDiveEnd = 607; // ExecuteLocationCommand
	public const int FlagTerritoryTransport = 201; // ExecuteCommand

	public static bool IsAvailable => true;
	public static string? InitError => null;

	public static unsafe bool Execute(int commandId, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0)
	{
		try
		{
			return GameMain.ExecuteCommand(commandId, param1, param2, param3, param4);
		}
		catch (Exception ex)
		{
			Plugin.Log?.Error($"[Replica] ExecuteCommand exception: {ex}");
			return false;
		}
	}

	public static unsafe bool ExecuteLocation(int commandId, Vector3 location, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0)
	{
		try
		{
			return GameMain.ExecuteLocationCommand(commandId, &location, param1, param2, param3, param4);
		}
		catch (Exception ex)
		{
			Plugin.Log?.Error($"[Replica] ExecuteLocationCommand exception: {ex}");
			return false;
		}
	}
}
