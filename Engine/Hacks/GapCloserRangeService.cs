using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace Replica.Engine.Hacks;

public unsafe sealed class GapCloserRangeService : IDisposable
{
	private readonly Plugin _plugin;

	// Signature for ActionManager::GetActionInRangeOrLoS call
	private const string GetActionInRangeOrLoSSig = "E8 ?? ?? ?? ?? 85 C0 75 02 33 C0";

	private unsafe delegate uint GetActionInRangeOrLoSDelegate(uint actionId, GameObject* source, GameObject* target);
	private Hook<GetActionInRangeOrLoSDelegate>? _getActionInRangeOrLoSHook;

	public bool IsAvailable => _getActionInRangeOrLoSHook != null;

	public bool IsActive => IsEnabled && _plugin.Configuration.HacksUnlocked;

	public bool IsEnabled
	{
		get => _plugin.Configuration.GapCloserRangeEnabled;
		set
		{
			if (_plugin.Configuration.GapCloserRangeEnabled != value)
			{
				_plugin.Configuration.GapCloserRangeEnabled = value;
				_plugin.Configuration.Save();
				UpdateHookState();
			}
		}
	}

	public GapCloserRangeService(Plugin plugin)
	{
		_plugin = plugin;

		try
		{
			_getActionInRangeOrLoSHook = Plugin.GameInterop.HookFromSignature<GetActionInRangeOrLoSDelegate>(GetActionInRangeOrLoSSig, OnGetActionInRangeOrLoSDetour);
			Plugin.Log?.Information("[Replica] GapCloserRangeService initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize GapCloserRangeService: {ex.Message}");
		}

		UpdateHookState();
	}

	public void UpdateHookState()
	{
		try
		{
			if (IsActive && _getActionInRangeOrLoSHook != null)
			{
				if (!_getActionInRangeOrLoSHook.IsEnabled)
					_getActionInRangeOrLoSHook.Enable();
			}
			else
			{
				if (_getActionInRangeOrLoSHook != null && _getActionInRangeOrLoSHook.IsEnabled)
					_getActionInRangeOrLoSHook.Disable();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error updating GapCloserRangeService hook state: {ex.Message}");
		}
	}

	private unsafe uint OnGetActionInRangeOrLoSDetour(uint actionId, GameObject* source, GameObject* target)
	{
		if (IsEnabled && _plugin.Configuration.HacksUnlocked)
		{
			try
			{
				var action = Plugin.Actions.GetRowOrDefault(actionId);
				if (action.HasValue && action.Value.AffectsPosition)
				{
					// Return 0 to bypass range and line-of-sight limits for gap closers
					return 0u;
				}
			}
			catch
			{
				// Defensive fallback
			}
		}

		return _getActionInRangeOrLoSHook!.Original(actionId, source, target);
	}

	public void Dispose()
	{
		try
		{
			if (_getActionInRangeOrLoSHook != null)
			{
				if (_getActionInRangeOrLoSHook.IsEnabled)
					_getActionInRangeOrLoSHook.Disable();
				_getActionInRangeOrLoSHook.Dispose();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Debug($"[Replica] GapCloserRangeService dispose: {ex.Message}");
		}
		_getActionInRangeOrLoSHook = null;
	}
}
