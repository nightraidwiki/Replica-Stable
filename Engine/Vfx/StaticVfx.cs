using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Interop.Game;
using Replica.Engine.Managers;
using Replica.Engine.Memory;
using Replica.Engine.Struct.Vfx;
using Replica.Engine.Util;

namespace Replica.Engine.Vfx;

public class StaticVfx : BaseVfx
{
	private nint _ownerAddress = IntPtr.Zero;

	private readonly long _createdTick;

	private long _elapsed;

	public long initTime;

	private long _remainingTime = long.MaxValue;

	private int _hitCount;

	private Vector4 _currentColor = Vector4.One;

	public float height;

	public bool Init;

	private IGameObject? _owner;

	public Actor? Actor { get; set; }

	public bool Enable { get; set; } = true;

	public Vector3 Scale { get; set; } = Vector3.One;

	public Vector3 Position { get; set; }

	public Vector3 Offset { get; set; } = Vector3.Zero;

	public Vector4 Color { get; set; } = Vector4.One;

	public Vector4 TargetColor { get; set; } = Vector4.One;

	public Angle Rotation { get; set; }

	public Angle OffsetRotation { get; set; }

	public float Radian { get; set; } = 1f;

	public IGameObject? Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
			_ownerAddress = value?.Address ?? IntPtr.Zero;
		}
	}

	public IGameObject? Target { get; set; }

	public Vector3 TargetPosition { get; set; } = new Vector3(float.MinValue, 0f, 0f);

	public long DrawTime { get; set; }

	public long DelayTime { get; set; }

	public long LoopInterval { get; set; }

	public bool FixRotation { get; set; }

	public bool EndToTarget { get; set; }

	public bool AlwaysFaceCurrentTarget { get; set; }

	public bool AlwaysDrawOnCurrentTarget { get; set; }

	public bool OnlyVisible { get; set; }

	public Vector3 LastPosition { get; private set; }

	public Func<Vector3>? PositionCustomAction { get; set; }

	public Func<Vector3>? TargetPositionCustomAction { get; set; }

	public Func<Angle>? RotationCustomAction { get; set; }

	public HitCounter? HitCounter { get; set; }

	public DistanceCheck? DistanceCheck { get; set; }

	public TetherCheck? TetherCheck { get; set; }

	public StatusCheck? StatusCheck { get; set; }

	public CountCheck? CountCheck { get; set; }

	public KnockBackCheck? KnockBackCheck { get; set; }

	public WatchCheck? WatchCheck { get; set; }

	public StaticVfx(string path, Vector3 scale, IGameObject owner, Vector4 color)
		: this(path, scale, owner.Position, color, owner.Rotation.Radians())
	{
		Owner = owner;
		_ownerAddress = owner.Address;
	}

	public StaticVfx(string path, Vector3 scale, Vector3 position, Vector4 color, Angle rotation)
		: base(path)
	{
		Scale = scale;
		Position = position;
		Color = color;
		Rotation = rotation;
		_createdTick = Environment.TickCount64;
		FrameworkUpdateManager.StaticVfxs.Add(this);
		ClientOmenHooks.drawOmenElementList.Add(this);
	}

	public new unsafe void Update()
	{
		if (!Init)
		{
			return;
		}
		try
		{
			_elapsed = Environment.TickCount64 - _createdTick;
			if (initTime == 0L && _elapsed > DelayTime)
			{
				Create();
			}
			else
			{
				if (initTime == 0L)
				{
					return;
				}
				height = Scale.Y;
				_remainingTime = DrawTime - (Environment.TickCount64 - (_createdTick + DelayTime));
				if (HitCounter == null)
				{
					if (_remainingTime < 0)
					{
						Remove();
						return;
					}
				}
				else if (_hitCount >= HitCounter.TargetHitCount)
				{
					Remove();
					return;
				}
				if (LoopInterval > 0)
				{
					if (Environment.TickCount64 - initTime >= LoopInterval)
					{
						if (Vfx != null && ClientOmenHooks.runStaticVfx != null)
						{
							ClientOmenHooks.runStaticVfx((nint)Vfx, 0f, uint.MaxValue);
							initTime = Environment.TickCount64;
						}
					}
				}
				if (Actor != null)
				{
					IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.GameObjectId == Actor.GameObjectID);
					if (gameObject == null)
					{
						Remove();
						return;
					}
					if (gameObject.IsDead)
					{
						IBattleChara battleChara = (IBattleChara)((gameObject is IBattleChara) ? gameObject : null);
						if (battleChara != null && !battleChara.IsCasting)
						{
							Remove();
							return;
						}
					}
				}
				if (Owner != null)
				{
					IGameObject gameObject2 = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == _ownerAddress);
					if (gameObject2 == null || gameObject2.IsDead)
					{
						Remove();
						return;
					}
					if (AlwaysFaceCurrentTarget)
					{
						Target = gameObject2.TargetObject;
					}
					if (AlwaysDrawOnCurrentTarget)
					{
						Owner = gameObject2.TargetObject;
					}
				}
				try
				{
					ApplyPosition();
					ApplyRotation();
					ApplyEndToTargetScale();
					ApplyColorFade();
					WatchCheckFunc();
					ApplyDistanceCheck();
					ApplyTetherCheck();
					ApplyStatusCheck();
					ApplyCountCheck();
					ApplyKnockBackCheck();
					if (Owner != null && OnlyVisible)
					{
						IGameObject gameObject3 = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == _ownerAddress);
						ICharacter character = (ICharacter)((gameObject3 is ICharacter) ? gameObject3 : null);
						if (character != null && !character.IsCharacterVisible())
						{
							height = 0f;
						}
					}
					return;
				}
				finally
				{
					if (Vfx != null)
					{
						if (!Enable)
						{
							Vfx->Scale.Y = 0f;
							Vfx->Color.W = 0f;
						}
						else
						{
							Vfx->Scale.Y = height;
							Vfx->Color.W = ((height == 0f) ? 0f : Color.W);
						}
						base.Update();
					}
				}
			}
		}
		catch (Exception e)
		{
			e.Log();
		}
	}

	private unsafe void Create()
	{
		Vfx = (VfxStruct*)ClientOmenHooks.createStaticVfx(Path, "Client.System.Scheduler.Instance.VfxObject");
		if (Vfx != null && Vfx != (VfxStruct*)IntPtr.Zero)
		{
			VfxHandle = Vfx->Apricot->OmenVFXHandle;
			ClientOmenHooks.runStaticVfx((nint)Vfx, 0f, uint.MaxValue);
			ClientOmenHooks.TrackedVfxHandles.Add(((nint)Vfx, VfxHandle));
			UpdatePosition(Position);
			UpdateRotation(new Vector3(0f, 0f, Rotation.Rad));
			UpdateScale(Scale);
			UpdateColor(Color);
			base.Update();
			initTime = Environment.TickCount64;
		}
	}

	public bool IsWatchCheck(Vector3 TargetPos)
	{
		Vector3 vector = TargetPos - Owner.Position;
		vector.Y = 0f;
		if (Math.Abs(vector.X) < float.Epsilon && Math.Abs(vector.Z) < float.Epsilon)
		{
			return true;
		}
		float x = (float)Math.Sin(Owner.Rotation);
		float z = (float)Math.Cos(Owner.Rotation);
		Vector3 vector2 = new Vector3(x, 0f, z);
		float num = vector2.X * vector.X + vector2.Z * vector.Z;
		float num2 = (float)Math.Sqrt(vector.X * vector.X + vector.Z * vector.Z);
		return num >= num2 * 0.70710677f;
	}

	public void WatchCheckFunc()
	{
		if (WatchCheck != null && Owner != null)
		{
			if (IsWatchCheck(WatchCheck.WatchCheckPostion))
			{
				Color = WatchCheck.WatchWarnColor;
			}
			else
			{
				Color = WatchCheck.WatchSafeColor;
			}
		}
	}

	private Vector3 EffectiveTargetPosition()
	{
		if (TargetPositionCustomAction != null)
		{
			return TargetPositionCustomAction();
		}
		if (TargetPosition.X != float.MinValue)
		{
			return TargetPosition;
		}
		return Target?.RenderPosition() ?? Vector3.Zero;
	}

	private void ApplyPosition()
	{
		if (Owner != null)
		{
			if (Target != null && Svc.Objects.SearchById(Target.GameObjectId) == null)
			{
				return;
			}
			IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == _ownerAddress);
			if (gameObject != null)
			{
				Vector3 vector = gameObject.RenderPosition();
				Angle angle = (FixRotation ? Rotation : (gameObject.Rotation.Radians() + Rotation));
				if (Target != null)
				{
					Vector3 vector2 = Target.RenderPosition() - vector;
					angle = MathF.Atan2(vector2.X, vector2.Z).Radians();
				}
				else if (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null)
				{
					Vector3 vector3 = EffectiveTargetPosition() - vector;
					angle = MathF.Atan2(vector3.X, vector3.Z).Radians();
				}
				Vector3 vector4 = RotateOffset(Offset, angle);
				Vector3 position = (LastPosition = vector - vector4);
				UpdatePosition(position);
			}
		}
		else if (Target == null || Svc.Objects.SearchById(Target.GameObjectId) != null)
		{
			if (PositionCustomAction != null)
			{
				Position = PositionCustomAction();
			}
			Angle angle2 = (RotationCustomAction != null) ? RotationCustomAction() : Rotation;
			if (RotationCustomAction == null)
			{
				if (Target != null)
				{
					Vector3 vector6 = Target.RenderPosition() - Position;
					angle2 = MathF.Atan2(vector6.X, vector6.Z).Radians();
				}
				else if (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null)
				{
					Vector3 vector7 = EffectiveTargetPosition() - Position;
					angle2 = MathF.Atan2(vector7.X, vector7.Z).Radians();
				}
			}
			Vector3 vector8 = RotateOffset(Offset, angle2);
			Vector3 position2 = (LastPosition = Position - vector8);
			UpdatePosition(position2);
		}
	}

	private static Vector3 RotateOffset(Vector3 offset, Angle angle)
	{
		float num = MathF.Sin(angle.Rad);
		float num2 = MathF.Cos(angle.Rad);
		return new Vector3(offset.X * num2 + offset.Z * num, z: (0f - offset.X) * num + offset.Z * num2, y: offset.Y);
	}

	private void ApplyRotation()
	{
		if (Owner != null)
		{
			IGameObject gameObject = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == _ownerAddress);
			if (gameObject != null)
			{
				Vector3 vector = gameObject.RenderPosition();
				Angle angle = (FixRotation ? Rotation : (gameObject.Rotation.Radians() + Rotation));
				if (Target != null)
				{
					Vector3 vector2 = Target.RenderPosition() - vector;
					angle = MathF.Atan2(vector2.X, vector2.Z).Radians();
				}
				else if (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null)
				{
					Vector3 vector3 = EffectiveTargetPosition() - vector;
					angle = MathF.Atan2(vector3.X, vector3.Z).Radians();
				}
				UpdateRotation(new Vector3(0f, 0f, (angle + OffsetRotation).Rad));
			}
		}
		else
		{
			Angle angle2 = ((RotationCustomAction != null) ? RotationCustomAction() : Rotation);
			if (RotationCustomAction == null && Target != null)
			{
				Vector3 vector4 = Target.RenderPosition() - Position;
				angle2 = MathF.Atan2(vector4.X, vector4.Z).Radians();
			}
			else if (RotationCustomAction == null && (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null))
			{
				Vector3 vector5 = EffectiveTargetPosition() - Position;
				angle2 = MathF.Atan2(vector5.X, vector5.Z).Radians();
			}
			UpdateRotation(new Vector3(0f, 0f, (angle2 + OffsetRotation).Rad));
		}
	}

	private void ApplyEndToTargetScale()
	{
		if (EndToTarget && Target != null)
		{
			IGameObject gameObject = ((Owner != null) ? Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == _ownerAddress) : null);
			if (gameObject != null)
			{
				UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(gameObject.RenderPosition(), Target.RenderPosition())));
			}
			else
			{
				UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(Position, Target.RenderPosition())));
			}
		}
		if ((TargetPosition.X != float.MinValue || TargetPositionCustomAction != null) && EndToTarget)
		{
			Vector3 value = EffectiveTargetPosition();
			IGameObject gameObject2 = ((Owner != null) ? Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == _ownerAddress) : null);
			if (gameObject2 != null)
			{
				UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(gameObject2.RenderPosition(), value)));
			}
			else
			{
				UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(Position, value)));
			}
		}
	}

	private void ApplyColorFade()
	{
		if (HitCounter == null && WatchCheck == null)
		{
			if (_remainingTime >= 0)
			{
				float amount = 1f - (float)_remainingTime / (float)DrawTime;
				UpdateColor(_currentColor = Vector4.Lerp(Color, TargetColor, amount));
			}
		}
		else
		{
			UpdateColor(Color);
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

	public unsafe override void Remove()
	{
		FrameworkUpdateManager.StaticVfxs.Remove(this);
		ClientOmenHooks.drawOmenElementList.Remove(this);
		if (Vfx != null && ClientOmenHooks.removeStaticVfx != null)
		{
			ClientOmenHooks.removeStaticVfx((nint)Vfx, 10f);
			Vfx = null;
			VfxHandle = IntPtr.Zero;
		}
	}

	private void ApplyDistanceCheck()
	{
		if (DistanceCheck == null)
		{
			return;
		}
		float num = Scale.Y;
		switch (DistanceCheck.CheckType)
		{
		case 0:
			if (DistanceCheck.CheckObject != null && Target != null && !DistanceCheck.CheckObject.SortedByRange().Take(DistanceCheck.Count).Contains<IGameObject>(Target))
			{
				num = 0f;
			}
			break;
		case 1:
			if (DistanceCheck.CheckObject != null && Target != null && !DistanceCheck.CheckObject.SortedByRange().TakeLast(DistanceCheck.Count).Contains<IGameObject>(Target))
			{
				num = 0f;
			}
			break;
		case 2:
			if (DistanceCheck.CheckObject != null && Owner != null && !DistanceCheck.CheckObject.SortedByRange().Take(DistanceCheck.Count).Contains<IGameObject>(Owner))
			{
				num = 0f;
			}
			break;
		case 3:
			if (DistanceCheck.CheckObject != null && Owner != null && !DistanceCheck.CheckObject.SortedByRange().TakeLast(DistanceCheck.Count).Contains<IGameObject>(Owner))
			{
				num = 0f;
			}
			break;
		case 4:
			if (Target != null && !DistanceCheck.Position.SortedByRange().Take(DistanceCheck.Count).Contains<IGameObject>(Target))
			{
				num = 0f;
			}
			break;
		case 5:
			if (Target != null && !DistanceCheck.Position.SortedByRange().TakeLast(DistanceCheck.Count).Contains<IGameObject>(Target))
			{
				num = 0f;
			}
			break;
		case 6:
			if (Owner != null && !DistanceCheck.Position.SortedByRange().Take(DistanceCheck.Count).Contains<IGameObject>(Owner))
			{
				num = 0f;
			}
			break;
		case 7:
			if (Owner != null && !DistanceCheck.Position.SortedByRange().TakeLast(DistanceCheck.Count).Contains<IGameObject>(Owner))
			{
				num = 0f;
			}
			break;
		case 8:
			if (DistanceCheck.CheckObject != null && !DistanceCheck.Position.SortedByRange().Take(DistanceCheck.Count).Contains<IGameObject>(DistanceCheck.CheckObject))
			{
				num = 0f;
			}
			break;
		case 9:
			if (DistanceCheck.CheckObject != null && !DistanceCheck.Position.SortedByRange().TakeLast(DistanceCheck.Count).Contains<IGameObject>(DistanceCheck.CheckObject))
			{
				num = 0f;
			}
			break;
		}
		height = num;
	}

	private void ApplyTetherCheck()
	{
		if (TetherCheck == null || Owner == null)
		{
			return;
		}
		float num = Scale.Y;
		if (TetherCheck.CheckType == 0)
		{
			if (Target != null)
			{
				if (Data.TetherPlayer.FirstOrDefault((TetherInfo t) => t.From == Target.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) != null)
				{
					TetherInfo tetherInfo = Data.TetherPlayer.FirstOrDefault((TetherInfo t) => t.From == Target.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID));
					if (Owner.GameObjectId != tetherInfo.To)
					{
						num = 0f;
					}
				}
				else
				{
					num = 0f;
				}
			}
			else if (Data.TetherPlayer.FirstOrDefault((TetherInfo t) => t.From == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) == null)
			{
				num = 0f;
			}
		}
		if (TetherCheck.CheckType == 1)
		{
			if (Target != null)
			{
				if (Data.TetherPlayer.FirstOrDefault((TetherInfo t) => t.From == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) != null)
				{
					TetherInfo tetherInfo2 = Data.TetherPlayer.FirstOrDefault((TetherInfo t) => t.From == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID));
					if (Target.GameObjectId != tetherInfo2.To)
					{
						num = 0f;
					}
				}
				else
				{
					num = 0f;
				}
			}
			else if (Data.TetherPlayer.FirstOrDefault((TetherInfo t) => t.To == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) == null)
			{
				num = 0f;
			}
		}
		height = num;
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
		float num = Scale.Y;
		if (!StatusCheck.Reverse)
		{
			if (battleChara.StatusList.All((IStatus s) => s.StatusId != StatusCheck.Status))
			{
				num = 0f;
			}
		}
		else if (battleChara.StatusList.Any((IStatus s) => s.StatusId == StatusCheck.Status))
		{
			num = 0f;
		}
		height = num;
	}

	private void ApplyCountCheck()
	{
		if (CountCheck == null)
		{
			return;
		}
		float num = Scale.Y;
		int num2 = 0;
		foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
		{
			if (allPlayer != CountCheck?.CheckObject && CountCheck?.CheckObject.DistanceToTarget(allPlayer) < Scale.X)
			{
				num2++;
			}
		}
		if (num2 == CountCheck?.Count)
		{
			num *= CountCheck.SafeAlpha;
		}
		height = num;
	}

	private void ApplyKnockBackCheck()
	{
		if (KnockBackCheck == null || Owner == null)
		{
			return;
		}
		if (KnockBackCheck.OriginPos.HasValue)
		{
			Vector3 vector = Owner.Position - KnockBackCheck.OriginPos.Value;
			Angle angle = MathF.Atan2(vector.X, vector.Z).Radians();
			if (KnockBackCheck.Reverse)
			{
				angle += 180.Degrees();
			}
			UpdateRotation(new Vector3(0f, 0f, angle.Rad));
		}
		else if (KnockBackCheck.Angle.HasValue)
		{
			UpdateRotation(new Vector3(0f, 0f, KnockBackCheck.Angle.Value.Rad));
		}
		if (KnockBackCheck.Antiable && Owner.KnockBackAnti())
		{
			height = 0f;
		}
	}
}
