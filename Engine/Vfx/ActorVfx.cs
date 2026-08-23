using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.Struct.Vfx;

namespace Replica.Engine.Vfx;

public class ActorVfx : BaseVfx
{
	private readonly long _createdTick;

	private long _elapsed;

	private long _initTime;

	private long _remainingTime = long.MaxValue;

	private bool _tracked;

	private int _hitCount;

	public nint caster { get; set; } = IntPtr.Zero;

	public nint target { get; set; } = IntPtr.Zero;

	public long DestroyAt { get; set; } = 3000L;

	public long DelayTime { get; set; }

	public HitCounter? HitCounter { get; set; }

	public StatusCheck? StatusCheck { get; set; }

	public ActorVfx(string path, IGameObject caster, IGameObject target)
		: this(path, caster.Address, target.Address)
	{
	}

	public ActorVfx(string path, nint caster, nint target)
		: base(path)
	{
		this.caster = caster;
		this.target = target;
		_createdTick = Environment.TickCount64;
		FrameworkUpdateManager.ActorVfxs.Add(this);
		ClientOmenHooks.ActorVfxList.Add(this);
	}

	public new void Update()
	{
		_elapsed = Environment.TickCount64 - _createdTick;
		if (_initTime == 0L && _elapsed > DelayTime)
		{
			Create();
			return;
		}
		if (_initTime != 0L)
		{
			_remainingTime = DestroyAt - (Environment.TickCount64 - _initTime);
			ApplyStatusCheck();
		}
		if (HitCounter == null)
		{
			if (_remainingTime < 0)
			{
				Remove();
			}
		}
		else if (_hitCount >= HitCounter.TargetHitCount)
		{
			Remove();
		}
	}

	public unsafe void Create()
	{
		if (!IsLiveActor(caster) || !IsLiveActor(target))
		{
			Remove();
			return;
		}
		Vfx = (VfxStruct*)ClientOmenHooks.createActorVfx(Path, caster, target, -1f, '\0', 0, '\0');
		if (Vfx != null && Vfx != (VfxStruct*)IntPtr.Zero)
		{
			_tracked = !Path.StartsWith("vfx/lockon/eff/");
			if (_tracked)
			{
				ClientOmenHooks.TrackedVfxHandles.Add(((nint)Vfx, (nint)Vfx));
			}
			_initTime = Environment.TickCount64;
		}
	}

	private static bool IsLiveActor(nint address)
	{
		if (address == IntPtr.Zero)
		{
			return false;
		}
		foreach (IGameObject @object in Svc.Objects)
		{
			if (@object != null && @object.Address == address)
			{
				return true;
			}
		}
		return false;
	}

	public unsafe override void Remove()
	{
		FrameworkUpdateManager.ActorVfxs.Remove(this);
		ClientOmenHooks.ActorVfxList.Remove(this);
		if (Vfx != null && ClientOmenHooks.removeActorVfx != null)
		{
			ClientOmenHooks.removeActorVfx((nint)Vfx, '\u0001');
			Vfx = null;
		}
	}

	public void OnHitEvent(uint ActionID, IGameObject? hitGameObject)
	{
		if (HitCounter != null && HitCounter.ActionID.Contains(ActionID))
		{
			if (HitCounter.HitTarget != null && HitCounter.HitTarget == hitGameObject)
			{
				_hitCount++;
			}
			else if (HitCounter.HitTarget == null)
			{
				_hitCount++;
			}
		}
	}

	private void ApplyStatusCheck()
	{
		if (StatusCheck == null)
		{
			return;
		}
		IGameObject checkObject = StatusCheck.CheckObject;
		IBattleChara battleChara = (IBattleChara)((checkObject is IBattleChara) ? checkObject : null);
		if (battleChara == null)
		{
			return;
		}
		if (!StatusCheck.Reverse)
		{
			if (battleChara.StatusList.All((IStatus s) => s.StatusId != StatusCheck.Status))
			{
				Remove();
			}
		}
		else if (battleChara.StatusList.Any((IStatus s) => s.StatusId == StatusCheck.Status))
		{
			Remove();
		}
	}
}
