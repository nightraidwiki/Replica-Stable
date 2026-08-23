using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M3S;

public class BrutalImpactKnockback : ISpecialAction
{
	private enum State
	{
		None,
		Ready,
		NextNS,
		NextEW,
		NextCorners,
		NextCenter,
		Done
	}

	private State curState;

	public override string Name => "Brutal Impact (knockback)";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37883u, 38542u, 37884u, 38543u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Where((StaticVfx aoe) => Svc.Objects.LocalPlayer?.Position.AlmostEqual(aoe.KnockBackCheck.OriginPos.Value, 4f) ?? false);

	public override void OnEnvControl(byte index, uint state)
	{
		bool flag = curState == State.Ready;
		if (flag)
		{
			flag = (uint)(index - 14) <= 1u;
		}
		if (flag)
		{
			SetState((index == 14) ? State.NextNS : State.NextEW);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 37883u:
			curState = State.Ready;
			break;
		case 38542u:
			SetState(State.NextCorners);
			break;
		case 37884u:
			SetState(State.NextCenter);
			break;
		case 38543u:
			SetState(State.Done);
			break;
		}
	}

	private void SetState(State state)
	{
		if (curState != state)
		{
			curState = state;
			switch (state)
			{
			case State.NextNS:
			{
				DrawElement drawElement3 = new DrawElement
				{
					Enable = false,
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 22f,
					KnockBackCheck = new KnockBackCheck
					{
						OriginPos = new Vector3(100f, 0f, 89f)
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 38542u }
					}
				};
				aoes.Add(DrawManager.Draw(drawElement3, Svc.Objects.LocalPlayer));
				drawElement3.KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3(100f, 0f, 111f)
				};
				aoes.Add(DrawManager.Draw(drawElement3, Svc.Objects.LocalPlayer));
				break;
			}
			case State.NextEW:
			{
				DrawElement drawElement2 = new DrawElement
				{
					Enable = false,
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 22f,
					KnockBackCheck = new KnockBackCheck
					{
						OriginPos = new Vector3(89f, 0f, 100f)
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 38542u }
					}
				};
				aoes.Add(DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer));
				drawElement2.KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3(111f, 0f, 100f)
				};
				aoes.Add(DrawManager.Draw(drawElement2, Svc.Objects.LocalPlayer));
				break;
			}
			case State.NextCorners:
			{
				DrawElement drawElement = new DrawElement
				{
					Enable = false,
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 18f,
					KnockBackCheck = new KnockBackCheck
					{
						OriginPos = new Vector3(89f, 0f, 89f)
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 37884u }
					}
				};
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
				drawElement.KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3(89f, 0f, 111f)
				};
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
				drawElement.KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3(111f, 0f, 89f)
				};
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
				drawElement.KnockBackCheck = new KnockBackCheck
				{
					OriginPos = new Vector3(111f, 0f, 111f)
				};
				aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
				break;
			}
			case State.NextCenter:
			{
				DrawElement element = new DrawElement
				{
					Enable = false,
					drawAvfx = "e5d1_b1_kblaser_t1",
					radiusX = 1f,
					radiusZ = 14f,
					KnockBackCheck = new KnockBackCheck
					{
						OriginPos = new Vector3(100f, 0f, 100f)
					},
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 38543u }
					}
				};
				aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
				break;
			}
			}
		}
	}

	public override void Reset()
	{
		curState = State.None;
		base.Reset();
	}
}
