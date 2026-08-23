using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingMad.P2;

public class ForsakenEschaton : ISpecialAction
{
	public Dictionary<uint, Vector3> towers = new Dictionary<uint, Vector3>
	{
		{
			1u,
			new Vector3(100f, 0f, 92f)
		},
		{
			2u,
			new Vector3(105.657f, 0f, 94.343f)
		},
		{
			3u,
			new Vector3(108f, 0f, 100f)
		},
		{
			4u,
			new Vector3(105.657f, 0f, 105.657f)
		},
		{
			5u,
			new Vector3(100f, 0f, 108f)
		},
		{
			6u,
			new Vector3(94.343f, 0f, 105.657f)
		},
		{
			7u,
			new Vector3(92f, 0f, 100f)
		},
		{
			8u,
			new Vector3(94.343f, 0f, 94.343f)
		}
	};

	public Dictionary<IGameObject, uint> GameobjectIcon = new Dictionary<IGameObject, uint>();

	public List<Vector3> currTowers = new List<Vector3>();

	public override string Name => "Forsaken Eschaton";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47804u, 47806u, 47808u, 47809u, 47810u };

	public override void Update()
	{
		if (currTowers.Count == 0)
		{
			if (aoes.Count == 0)
			{
				return;
			}
			{
				foreach (StaticVfx aoe in aoes)
				{
					aoe.Enable = false;
				}
				return;
			}
		}
		foreach (StaticVfx aoe2 in aoes)
		{
			bool enable = false;
			foreach (Vector3 currTower in currTowers)
			{
				if ((aoe2.Owner.Position - currTower).Length() > 4f)
				{
					continue;
				}
				enable = true;
				if (aoe2.Path == "gl_fan090_1bf".Omen())
				{
					IGameObject gameObject = (aoe2.Target = aoe2.Owner.SortedByRange().FirstOrDefault());
					if (aoe2.Owner != Svc.Objects.LocalPlayer && gameObject != Svc.Objects.LocalPlayer)
					{
						enable = false;
					}
				}
				break;
			}
			aoe2.Enable = enable;
		}
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if (icon - 715 <= 2)
		{
			GameobjectIcon[Source] = icon;
			StaticVfx staticVfx = aoes.FirstOrDefault((StaticVfx x) => x.Owner == Source);
			if (staticVfx != null)
			{
				staticVfx.Remove();
				aoes.Remove(staticVfx);
			}
			switch (icon)
			{
			case 715u:
			{
				DrawElement element3 = new DrawElement
				{
					Enable = false,
					drawAvfx = "general_1bpxf",
					radiusX = 5f,
					radiusZ = 5f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47806u },
						TargetHitCount = 16
					}
				};
				aoes.Add(DrawManager.Draw(element3, Source));
				break;
			}
			case 716u:
			{
				DrawElement element2 = new DrawElement
				{
					Enable = false,
					drawAvfx = "general_1bxf",
					radiusX = 5f,
					radiusZ = 5f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47806u },
						TargetHitCount = 16
					}
				};
				aoes.Add(DrawManager.Draw(element2, Source));
				break;
			}
			case 717u:
			{
				DrawElement element = new DrawElement
				{
					Enable = false,
					drawAvfx = "gl_fan090_1bf",
					radiusX = 40f,
					radiusZ = 40f,
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 47806u },
						TargetHitCount = 16
					}
				};
				aoes.Add(DrawManager.Draw(element, Source));
				break;
			}
			}
		}
	}

	public override void OnEnvControl(byte index, uint state)
	{
		if (index >= 1 && index <= 8 && state == 524292)
		{
			currTowers.Add(towers[index]);
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId == 47804)
		{
			GameobjectIcon.Clear();
			currTowers.Clear();
		}
		if (info.ActionId == 47806 && currTowers.Count > 0)
		{
			currTowers.RemoveAt(0);
		}
	}

	public override void Reset()
	{
		GameobjectIcon.Clear();
		currTowers.Clear();
		base.Reset();
	}
}
