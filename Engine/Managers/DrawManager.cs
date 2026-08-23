using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Memory;
using Replica.Engine.Properties;
using Replica.Engine.Vfx;

namespace Replica.Engine.Managers;

public class DrawManager
{
	private static readonly Dictionary<string, Action<DrawElement>> CustomVfxBuilders = new Dictionary<string, Action<DrawElement>>
	{
		["customFan"] = delegate(DrawElement e)
		{
			OmenResourceCache.GetFan(e.refRadian, out string path);
			e.drawAvfx = path;
		},
		["customCircle"] = delegate(DrawElement e)
		{
			OmenResourceCache.GetCircle(out string path);
			e.drawAvfx = path;
		},
		["customDonut"] = delegate(DrawElement e)
		{
			OmenResourceCache.GetDonut(e.refRadian, out string path);
			e.drawAvfx = path;
		},
		["customRect"] = delegate(DrawElement e)
		{
			OmenResourceCache.GetRect(out string path);
			e.drawAvfx = path;
		},
		["customRect2"] = delegate(DrawElement e)
		{
			OmenResourceCache.GetRect2(out string path);
			e.drawAvfx = path;
		},
		["tank_fan90"] = delegate(DrawElement e)
		{
			OmenResourceCache.RegisterRaw(Resources.TankFan90, "vfx/omen/eff/yd/tankfan90.avfx");
			e.drawAvfx = "vfx/omen/eff/yd/tankfan90.avfx";
			e.radiusX = 1f;
			e.radiusY = 1f;
			e.radiusZ = 1f;
		},
		["share2_6m"] = delegate(DrawElement e)
		{
			OmenResourceCache.RegisterRaw(Resources.Share2_6m_5s_omen, "vfx/omen/eff/yd/share2_6m.avfx");
			e.drawAvfx = "vfx/omen/eff/yd/share2_6m.avfx";
			e.radiusX = 1f;
			e.radiusY = 2f;
			e.radiusZ = 1f;
			if (e.LoopInterval == 0f)
			{
				e.LoopInterval = 4900f;
			}
		},
		["eye_warn"] = delegate(DrawElement e)
		{
			OmenResourceCache.RegisterRaw(Resources.eyewarn, "vfx/omen/eff/yd/eyewarn.avfx");
			e.drawAvfx = "vfx/omen/eff/yd/eyewarn.avfx";
			e.radiusX = 1f;
			e.radiusY = 1f;
			e.radiusZ = 1f;
		},
		["e5d1_b1_kblaser_t1"] = delegate(DrawElement e)
		{
			e.refColor = new Vector4(1f, 1f, 1f, 2.5f);
			e.refTargetColor = new Vector4(1f, 1f, 1f, 2.5f);
		},
		["tower_noc"] = delegate(DrawElement e)
		{
			e.drawAvfx = SilentOmen("tower_noc", "vfx/omen/eff/yd/tower_noc.avfx", "m0119_trap_02t");
		},
		["knockback_noc"] = delegate(DrawElement e)
		{
			e.drawAvfx = SilentOmen("knockback_noc", "vfx/omen/eff/yd/knockback_noc.avfx", "m0501_nockback_omen01d1");
		},
		["laser_noc"] = delegate(DrawElement e)
		{
			e.drawAvfx = SilentOmen("laser_noc", "vfx/omen/eff/yd/laser_noc.avfx", "e5d1_b1_kblaser_t1");
			e.refColor = new Vector4(1f, 1f, 1f, 2.5f);
			e.refTargetColor = new Vector4(1f, 1f, 1f, 2.5f);
		},
		["tank_lockon_3m_5s_noc"] = delegate(DrawElement e)
		{
			OmenResourceCache.RegisterRaw(Resources.tank_lockon_3m_5s_noc, "vfx/omen/eff/yd/tank_lockon_3m_5s_noc.avfx");
			e.drawAvfx = "vfx/omen/eff/yd/tank_lockon_3m_5s_noc.avfx";
			e.radiusX = 1f;
			e.radiusY = 1f;
			e.radiusZ = 1f;
		},
		["tank_lockon_5m_5s_noc"] = delegate(DrawElement e)
		{
			OmenResourceCache.RegisterRaw(Resources.tank_lockon_5m_5s_noc, "vfx/omen/eff/yd/tank_lockon_5m_5s_noc.avfx");
			e.drawAvfx = "vfx/omen/eff/yd/tank_lockon_5m_5s_noc.avfx";
			e.radiusX = 1f;
			e.radiusY = 1f;
			e.radiusZ = 1f;
		},
		["ShareLazerGround5s"] = delegate(DrawElement e)
		{
			OmenResourceCache.RegisterRaw(Resources.ShareLazer5sGround, "vfx/omen/eff/yd/share_lazer_5s_ground.avfx");
			e.drawAvfx = "vfx/omen/eff/yd/share_lazer_5s_ground.avfx";
			e.radiusX = 1f;
			e.radiusY = 1f;
			e.radiusZ = 1f;
		}
	};

	private static string SilentOmen(string resourceName, string path, string fallback)
	{
		byte[] array = Resources.TryGet(resourceName);
		if (array == null)
		{
			return fallback;
		}
		OmenResourceCache.RegisterRaw(array, path);
		return path;
	}

	public static List<StaticVfx> Draw(DrawElement element, List<IGameObject> target, IGameObject? castObject = null)
	{
		List<StaticVfx> list = new List<StaticVfx>();
		if (element.drawAvfx.IsNullOrEmpty())
		{
			return list;
		}
		foreach (IGameObject item in target)
		{
			StaticVfx staticVfx = Draw(element, item, castObject);
			if (staticVfx != null)
			{
				list.Add(staticVfx);
			}
		}
		return list;
	}

	public static StaticVfx Draw(DrawElement element, IGameObject? target = null, IGameObject? castObject = null)
	{
		if (element.drawAvfx.IsNullOrEmpty())
		{
			return null;
		}
		if (element.drawOnObject)
		{
			if (target == null)
			{
				return null;
			}
			if (Svc.Objects.SearchById(target.GameObjectId) == null)
			{
				return null;
			}
		}
		switch (element.drawType)
		{
		case ElementType.Omen:
			return DrawOmen(target, element);
		case ElementType.LockOn:
			DrawLockOn(target, castObject, element);
			return null;
		case ElementType.Channeling:
			DrawChanneling(target, castObject, element);
			return null;
		case ElementType.RawAvfx:
			DrawRawAvfx(target, castObject, element);
			return null;
		default:
			return null;
		}
	}

	private static StaticVfx DrawOmen(IGameObject target, DrawElement element)
	{
		if (CustomVfxBuilders.TryGetValue(element.drawAvfx, out Action<DrawElement> value))
		{
			value(element);
		}
		StaticVfx staticVfx = CreateStaticVfx(element, target);
		ApplyElement(staticVfx, element);
		staticVfx.Init = true;
		return staticVfx;
	}

	private static StaticVfx CreateStaticVfx(DrawElement element, IGameObject target)
	{
		string path = ((element.drawAvfx.Split('/').Length == 1) ? element.drawAvfx.Omen() : element.drawAvfx);
		Vector3 scale = new Vector3(element.radiusX, element.radiusY, element.radiusZ);
		if (!element.drawOnObject)
		{
			return new StaticVfx(path, scale, element.Position, element.refColor, element.refRotation);
		}
		return new StaticVfx(path, scale, target, element.refColor);
	}

	private static void ApplyElement(StaticVfx vfx, DrawElement element)
	{
		vfx.Actor = element.Actor;
		vfx.Enable = element.Enable;
		vfx.Offset = new Vector3(element.refOffsetX, element.refOffsetY, element.refOffsetZ);
		vfx.Color = element.refColor;
		vfx.TargetColor = element.refTargetColor;
		vfx.Rotation = element.refRotation;
		vfx.OffsetRotation = element.refOffsetRotation;
		vfx.Radian = element.refRadian;
		vfx.Target = element.target;
		vfx.TargetPosition = element.targetPosition;
		vfx.DrawTime = (long)element.destroyTime;
		vfx.DelayTime = (long)element.delayDrawTime;
		vfx.LoopInterval = (long)element.LoopInterval;
		vfx.FixRotation = element.fixRotation;
		vfx.EndToTarget = element.endToTarget;
		vfx.AlwaysFaceCurrentTarget = element.alwaysFaceCurrentTarget;
		vfx.AlwaysDrawOnCurrentTarget = element.alwaysDrawOnCurrentTarget;
		vfx.OnlyVisible = element.OnlyVisible;
		vfx.PositionCustomAction = element.PositionCustomAction;
		vfx.TargetPositionCustomAction = element.TargetPositionCustomAction;
		vfx.RotationCustomAction = element.RotationCustomAction;
		vfx.HitCounter = element.hitCounter;
		vfx.DistanceCheck = element.distanceCheck;
		vfx.TetherCheck = element.TetherCheck;
		vfx.StatusCheck = element.StatusCheck;
		vfx.CountCheck = element.CountCheck;
		vfx.KnockBackCheck = element.KnockBackCheck;
		vfx.WatchCheck = element.WatchCheck;
	}

	private static void DrawLockOn(IGameObject target, IGameObject? castObject, DrawElement element)
	{
		if (castObject == null)
		{
			castObject = target;
		}
		new ActorVfx(element.drawAvfx.LockOn(), castObject.Address, target.Address).DelayTime = (long)element.delayDrawTime;
	}

	private static void DrawChanneling(IGameObject target, IGameObject? castObject, DrawElement element)
	{
		if (castObject == null)
		{
			castObject = target;
		}
		new ActorVfx(element.drawAvfx.Channeling(), castObject.Address, target.Address)
		{
			DelayTime = (long)element.delayDrawTime,
			DestroyAt = (long)element.destroyTime,
			HitCounter = element.hitCounter,
			StatusCheck = element.StatusCheck
		};
	}

	private static void DrawRawAvfx(IGameObject target, IGameObject? castObject, DrawElement element)
	{
		if (castObject == null)
		{
			castObject = target;
		}
		new ActorVfx(element.drawAvfx, castObject.Address, target.Address)
		{
			DelayTime = (long)element.delayDrawTime,
			DestroyAt = (long)element.destroyTime
		};
	}
}
