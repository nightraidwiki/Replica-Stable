using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Replica.Engine.Bridge.BossMod.Core;
using Replica.Engine.Bridge.BossMod.Overlay;
using static Replica.Engine.Bridge.BossMod.Reflection.BossModReflection;

namespace Replica.Engine.Bridge.BossMod.Processors;

public sealed class BossModHintsProcessor
{
	private readonly BossModOverlayRenderer _overlay;

	// Caches for reflection types and methods
	private Type? _globalHintsType;
	private Type? _textHintsType;
	private MethodInfo? _calcGlobalHintsMethod;
	private MethodInfo? _calcMemberHintsMethod;

	private object? _globalHintsObj;
	private object? _textHintsObj;

	private readonly List<OverlayBannerHint> _activeBanners = new(8);
	private string _lastPrimaryToastMessage = string.Empty;
	private DateTime _lastToastTime = DateTime.MinValue;

	public BossModHintsProcessor(BossModOverlayRenderer overlay)
	{
		_overlay = overlay;
	}

	public void Process(BossModContext ctx)
	{
		var config = ctx.Plugin.Configuration;
		if (!config.BossModMirrorHintsBanners)
		{
			_overlay.ClearBanners();
			return;
		}

		_activeBanners.Clear();

		try
		{
			EnsureTypes(ctx);

			// 1. Process Global Hints (Blue Banner)
			ProcessGlobalHints(ctx, config);

			// 2. Process Player-specific Text Hints (Red or Blue depending on isRisk)
			ProcessTextHints(ctx, config);

			// 3. Dispatch to Overlay Renderer
			_overlay.SetBanners(_activeBanners);

			// 4. Dispatch optional native in-game ToastGui notifications
			if (config.BossModHintsNativeToast && _activeBanners.Count > 0)
			{
				TriggerNativeToast(_activeBanners[0]);
			}
		}
		catch
		{
			_overlay.ClearBanners();
		}
	}

	public void Clear()
	{
		_activeBanners.Clear();
		_lastPrimaryToastMessage = string.Empty;
		_overlay.ClearBanners();
	}

	private void EnsureTypes(BossModContext ctx)
	{
		if (_globalHintsType != null && _textHintsType != null) return;

		var asm = ctx.Module.GetType().Assembly;
		_globalHintsType = asm.GetType("BossMod.BossComponent+GlobalHints");
		_textHintsType = asm.GetType("BossMod.BossComponent+TextHints");

		if (_globalHintsType != null && _globalHintsObj == null)
			_globalHintsObj = Activator.CreateInstance(_globalHintsType);

		if (_textHintsType != null && _textHintsObj == null)
			_textHintsObj = Activator.CreateInstance(_textHintsType);
	}

	private void ProcessGlobalHints(BossModContext ctx, Configuration config)
	{
		if (config.BossModHintsRiskOnly) return; // Skip info hints if risk-only mode is active

		// Method A: Try Module.CalculateGlobalHints()
		if (_calcGlobalHintsMethod == null)
			_calcGlobalHintsMethod = ctx.Module.GetType().GetMethod("CalculateGlobalHints", BindingFlags.Public | BindingFlags.Instance);

		if (_calcGlobalHintsMethod != null)
		{
			try
			{
				var res = _calcGlobalHintsMethod.Invoke(ctx.Module, null);
				if (res is IEnumerable list)
				{
					foreach (var item in list)
					{
						if (item is string s && !string.IsNullOrWhiteSpace(s))
						{
							AddRawHint(s.Trim(), OverlayBannerKind.InfoBlue, false);
						}
					}
				}
			}
			catch { }
		}

		// Method B: Fallback if empty - iterate module.Components calling comp.AddGlobalHints(_globalHintsObj)
		if (_activeBanners.Count == 0 && _globalHintsObj is IList globalList)
		{
			globalList.Clear();
			if (Get(ctx.Module, "Components") is IEnumerable comps)
			{
				foreach (var comp in comps)
				{
					if (comp == null) continue;
					var method = comp.GetType().GetMethod("AddGlobalHints", BindingFlags.Public | BindingFlags.Instance);
					if (method != null)
					{
						try { method.Invoke(comp, [_globalHintsObj]); } catch { }
					}
				}
			}

			foreach (var item in globalList)
			{
				if (item is string s && !string.IsNullOrWhiteSpace(s))
				{
					AddRawHint(s.Trim(), OverlayBannerKind.InfoBlue, false);
				}
			}
		}
	}

	private void ProcessTextHints(BossModContext ctx, Configuration config)
	{
		if (ctx.PcActor == null) return;

		// Method A: Try Module.CalculateHintsForRaidMember(slot, actor)
		if (_calcMemberHintsMethod == null)
			_calcMemberHintsMethod = ctx.Module.GetType().GetMethod("CalculateHintsForRaidMember", BindingFlags.Public | BindingFlags.Instance);

		if (_calcMemberHintsMethod != null)
		{
			try
			{
				var res = _calcMemberHintsMethod.Invoke(ctx.Module, [ctx.PcSlot, ctx.PcActor]);
				if (res is IEnumerable list)
				{
					foreach (var item in list)
					{
						ExtractTextHintItem(item, config);
					}
				}
			}
			catch { }
		}

		// Method B: Fallback - iterate module.Components calling comp.AddHints(slot, actor, _textHintsObj)
		if (_textHintsObj is IList textList)
		{
			textList.Clear();
			if (Get(ctx.Module, "Components") is IEnumerable comps)
			{
				foreach (var comp in comps)
				{
					if (comp == null) continue;
					var method = comp.GetType().GetMethod("AddHints", BindingFlags.Public | BindingFlags.Instance);
					if (method != null)
					{
						try { method.Invoke(comp, [ctx.PcSlot, ctx.PcActor, _textHintsObj]); } catch { }
					}
				}
			}

			foreach (var item in textList)
			{
				ExtractTextHintItem(item, config);
			}
		}
	}

	private void ExtractTextHintItem(object? item, Configuration config)
	{
		if (item == null) return;

		string? text = null;
		bool isRisk = true;

		// ValueTuple<string, bool> / (string, bool)
		var t1 = GetField(item, "Item1");
		var t2 = GetField(item, "Item2");

		if (t1 is string s)
		{
			text = s;
			if (t2 is bool b) isRisk = b;
		}
		else if (item is string plain)
		{
			text = plain;
		}

		if (string.IsNullOrWhiteSpace(text)) return;
		text = text.Trim();

		if (config.BossModHintsRiskOnly && !isRisk)
			return;

		var kind = isRisk ? OverlayBannerKind.DangerRed : OverlayBannerKind.InfoBlue;
		AddRawHint(text, kind, isRisk);
	}

	private void AddRawHint(string text, OverlayBannerKind kind, bool isRisk)
	{
		// Avoid duplicate identical text within the same frame
		for (int i = 0; i < _activeBanners.Count; i++)
		{
			if (_activeBanners[i].Text == text)
			{
				if (isRisk && !_activeBanners[i].IsRisk)
				{
					_activeBanners[i] = new OverlayBannerHint(text, kind, isRisk);
				}
				return;
			}
		}

		if (isRisk)
		{
			_activeBanners.Insert(0, new OverlayBannerHint(text, kind, isRisk));
		}
		else
		{
			_activeBanners.Add(new OverlayBannerHint(text, kind, isRisk));
		}
	}

	private void TriggerNativeToast(OverlayBannerHint primary)
	{
		try
		{
			var now = DateTime.UtcNow;
			if (_lastPrimaryToastMessage == primary.Text && (now - _lastToastTime).TotalSeconds < 4.0)
				return;

			_lastPrimaryToastMessage = primary.Text;
			_lastToastTime = now;

			if (Plugin.ToastGui != null)
			{
				if (primary.Kind == OverlayBannerKind.DangerRed)
				{
					Plugin.ToastGui.ShowError($"[ALERT] {primary.Text}");
				}
				else
				{
					Plugin.ToastGui.ShowQuest($"[STRAT] {primary.Text}");
				}
			}
		}
		catch { }
	}
}
