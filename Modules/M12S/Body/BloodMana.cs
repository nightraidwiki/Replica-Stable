using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M12S.Body;

public class BloodMana : ISpecialAction
{
	private static readonly uint[] OrbBaseIds = new uint[4] { 19206u, 19207u, 19208u, 19209u };

	private static readonly Vector2 EastAnchor = new Vector2(110f, 100f);

	private static readonly Vector2 WestAnchor = new Vector2(90f, 100f);

	private const float PlatformRadius = 10f;

	private readonly Dictionary<ulong, StaticVfx> _marks = new Dictionary<ulong, StaticVfx>();

	private long _showAfterMs;

	private long _showUntilMs;

	private bool _captured;

	public override string Name => "Blood Mana";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 46333u };

	private static Vector4 PickGreen => new Vector4(0.1f, 1f, 0.1f, Plugin.Config.CustomAlpha);

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 46333)
		{
			ClearMarks();
			long tickCount = Environment.TickCount64;
			_showAfterMs = tickCount + 1000;
			_showUntilMs = tickCount + 11000;
			_captured = false;
		}
	}

	public override void Update()
	{
		if (_showUntilMs == 0L)
		{
			return;
		}
		long tickCount = Environment.TickCount64;
		if (tickCount >= _showUntilMs)
		{
			ClearMarks();
		}
		else
		{
			if (tickCount < _showAfterMs || _captured)
			{
				return;
			}
			_captured = true;
			Dictionary<uint, List<IGameObject>> dictionary = new Dictionary<uint, List<IGameObject>>();
			foreach (IGameObject @object in Svc.Objects)
			{
				if (IsOrb(@object) && IsVisible(@object))
				{
					if (!dictionary.TryGetValue(@object.BaseId, out var value))
					{
						value = new List<IGameObject>();
						dictionary[@object.BaseId] = value;
					}
					value.Add(@object);
				}
			}
			HashSet<ulong> hashSet = new HashSet<ulong>();
			foreach (List<IGameObject> value2 in dictionary.Values)
			{
				CollectFarOrbs(value2, EastAnchor, hashSet);
				CollectFarOrbs(value2, WestAnchor, hashSet);
			}
			foreach (ulong key in _marks.Keys)
			{
				if (!hashSet.Contains(key))
				{
					RemoveMark(key);
				}
			}
			foreach (ulong item in hashSet)
			{
				if (_marks.ContainsKey(item))
				{
					continue;
				}
				IGameObject gameObject = item.GameObject();
				if (gameObject != null)
				{
					StaticVfx staticVfx = DrawManager.Draw(new DrawElement
					{
						drawAvfx = "customCircle",
						radiusX = 4f,
						radiusZ = 4f,
						drawOnObject = true,
						refColor = PickGreen,
						refTargetColor = PickGreen,
						destroyTime = 600000f
					}, gameObject);
					if (staticVfx != null)
					{
						_marks[item] = staticVfx;
						aoes.Add(staticVfx);
					}
				}
			}
		}
	}

	public override void Reset()
	{
		ClearMarks();
		base.Reset();
	}

	private static bool IsOrb(IGameObject obj)
	{
		uint baseId = obj.BaseId;
		uint[] orbBaseIds = OrbBaseIds;
		foreach (uint num in orbBaseIds)
		{
			if (baseId == num)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsVisible(IGameObject obj)
	{
		if (obj is ICharacter chara)
		{
			return chara.IsCharacterVisible();
		}
		return true;
	}

	private static void CollectFarOrbs(List<IGameObject> sameType, Vector2 anchor, HashSet<ulong> wanted)
	{
		bool flag = false;
		foreach (IGameObject item in sameType)
		{
			if (DistanceTo(item, anchor) <= 10f)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (IGameObject item2 in sameType)
		{
			if (DistanceTo(item2, anchor) > 10f)
			{
				wanted.Add(item2.GameObjectId);
			}
		}
	}

	private static float DistanceTo(IGameObject orb, Vector2 anchor)
	{
		return Vector2.Distance(new Vector2(orb.Position.X, orb.Position.Z), anchor);
	}

	private void RemoveMark(ulong id)
	{
		if (_marks.TryGetValue(id, out StaticVfx value))
		{
			value?.Remove();
			aoes.Remove(value);
			_marks.Remove(id);
		}
	}

	private void ClearMarks()
	{
		foreach (StaticVfx value in _marks.Values)
		{
			value?.Remove();
		}
		_marks.Clear();
		aoes.Clear();
		_showAfterMs = 0L;
		_showUntilMs = 0L;
		_captured = false;
	}
}
