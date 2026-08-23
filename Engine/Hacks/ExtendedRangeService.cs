using System;
using Dalamud.Hooking;

namespace Replica.Engine.Hacks;

public sealed class ExtendedRangeService : IDisposable
{
	private readonly Plugin _plugin;

	// Signature for ActionManager::GetActionRange call
	private const string GetActionRangeSig = "E8 ?? ?? ?? ?? F3 41 0F 11 06 80 3B";

	private delegate float GetActionRangeDelegate(uint actionId);
	private Hook<GetActionRangeDelegate>? _getActionRangeHook;

	public bool IsAvailable => _getActionRangeHook != null;

	public bool IsActive => IsEnabled && _plugin.Configuration.HacksUnlocked;

	public bool IsEnabled
	{
		get => _plugin.Configuration.ExtendedRangeEnabled;
		set
		{
			if (_plugin.Configuration.ExtendedRangeEnabled != value)
			{
				_plugin.Configuration.ExtendedRangeEnabled = value;
				_plugin.Configuration.Save();
				UpdateHookState();
			}
		}
	}

	public float ExtendedRange
	{
		get => Math.Clamp(_plugin.Configuration.ExtendedRangeDistance, 0.0f, 2.0f);
		set
		{
			float clamped = Math.Clamp(MathF.Round(value * 10f) / 10f, 0.0f, 2.0f);
			if (Math.Abs(_plugin.Configuration.ExtendedRangeDistance - clamped) > 0.001f)
			{
				_plugin.Configuration.ExtendedRangeDistance = clamped;
				_plugin.Configuration.Save();
			}
		}
	}

	public ExtendedRangeService(Plugin plugin)
	{
		_plugin = plugin;

		try
		{
			_getActionRangeHook = Plugin.GameInterop.HookFromSignature<GetActionRangeDelegate>(GetActionRangeSig, OnGetActionRangeDetour);
			Plugin.Log?.Information("[Replica] ExtendedRangeService initialized.");
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Could not initialize ExtendedRangeService: {ex.Message}");
		}

		UpdateHookState();
	}

	public void UpdateHookState()
	{
		try
		{
			if (IsActive && _getActionRangeHook != null)
			{
				if (!_getActionRangeHook.IsEnabled)
					_getActionRangeHook.Enable();
			}
			else
			{
				if (_getActionRangeHook != null && _getActionRangeHook.IsEnabled)
					_getActionRangeHook.Disable();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Warning($"[Replica] Error updating ExtendedRangeService hook state: {ex.Message}");
		}
	}

	private float OnGetActionRangeDetour(uint actionId)
	{
		float originalRange = _getActionRangeHook!.Original(actionId);
		if (!IsEnabled || !_plugin.Configuration.HacksUnlocked)
		{
			return originalRange;
		}

		if (actionId == 0 || originalRange == 0f)
		{
			return originalRange;
		}

		try
		{
			var action = Plugin.Actions.GetRowOrDefault(actionId);
			if (!action.HasValue || action.Value.TargetArea)
			{
				return originalRange;
			}
		}
		catch
		{
			return originalRange;
		}

		return originalRange + ExtendedRange;
	}

	public void Dispose()
	{
		try
		{
			if (_getActionRangeHook != null)
			{
				if (_getActionRangeHook.IsEnabled)
					_getActionRangeHook.Disable();
				_getActionRangeHook.Dispose();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log?.Debug($"[Replica] ExtendedRangeService dispose: {ex.Message}");
		}
		_getActionRangeHook = null;
	}
}
