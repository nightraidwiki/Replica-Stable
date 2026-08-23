using System.Numerics;
using Replica.QuickDraws;

namespace Replica.Strats;

public static class StratLibrary
{
	private static readonly Vector4 Defa = new Vector4(0.45f, 0.4f, 1f, 0.55f);

	private static readonly Vector4 Stack = new Vector4(0.1f, 0.75f, 0.4f, 0.55f);

	private static readonly Vector4 Safe = new Vector4(0.2f, 0.9f, 1f, 0.55f);

	private static readonly Vector4 Tower = new Vector4(0.2f, 0.95f, 0.35f, 0.6f);

	private static readonly Vector4 Bait = new Vector4(1f, 0.8f, 0.1f, 0.45f);

	public static StratPack BuildIdyllicExample(uint territory)
	{
		return new StratPack
		{
			Name = "M12S Idyllic Dream (example)",
			Territory = territory,
			Author = "Null",
			ArenaShape = 0,
			ArenaRadius = 20f,
			ArenaCenterX = 100f,
			ArenaCenterZ = 100f,
			Slides = 
			{
				SpreadStep(),
				ConeSplitStep(),
				TetherStep(),
				TowerRoleStep(),
				NearFarStep()
			}
		};
	}

	private static StratSlide SpreadStep()
	{
		StratSlide stratSlide = NewSlide("Idyllic Dream — spread", 46345u);
		StratBranch stratBranch = new StratBranch
		{
			Name = "Spread"
		};
		Vector3[] array = new Vector3[8]
		{
			new Vector3(100f, 0f, 86f),
			new Vector3(110f, 0f, 90f),
			new Vector3(114f, 0f, 100f),
			new Vector3(110f, 0f, 110f),
			new Vector3(100f, 0f, 114f),
			new Vector3(90f, 0f, 110f),
			new Vector3(86f, 0f, 100f),
			new Vector3(90f, 0f, 90f)
		};
		for (int i = 0; i < 8; i++)
		{
			stratBranch.Spots.Add(Spot((StratRole)i, array[i], QuickShape.Circle, Safe, 1.2f));
		}
		stratSlide.Branches.Add(stratBranch);
		return stratSlide;
	}

	private static StratSlide ConeSplitStep()
	{
		StratSlide stratSlide = NewSlide("Cone cleave — boss N/S", 46352u);
		StratBranch stratBranch = new StratBranch
		{
			Name = "Boss North"
		};
		stratBranch.Conditions.Add(new StratCondition
		{
			Kind = CondKind.BossSide,
			BossSide = Compass.N
		});
		FillUniform(stratBranch, new Vector3(100f, 0f, 110f), QuickShape.Circle, Safe, 1.5f);
		StratBranch stratBranch2 = new StratBranch
		{
			Name = "Boss South"
		};
		stratBranch2.Conditions.Add(new StratCondition
		{
			Kind = CondKind.BossSide,
			BossSide = Compass.S
		});
		FillUniform(stratBranch2, new Vector3(100f, 0f, 90f), QuickShape.Circle, Safe, 1.5f);
		StratBranch stratBranch3 = new StratBranch
		{
			Name = "Default"
		};
		FillUniform(stratBranch3, new Vector3(100f, 0f, 100f), QuickShape.Circle, Safe, 1.5f);
		stratSlide.Branches.Add(stratBranch);
		stratSlide.Branches.Add(stratBranch2);
		stratSlide.Branches.Add(stratBranch3);
		return stratSlide;
	}

	private static StratSlide TetherStep()
	{
		StratSlide stratSlide = NewSlide("Tether resolve — grab my clone", 0u);
		stratSlide.On = TriggerMatch.Tether;
		stratSlide.MatchById = false;
		StratBranch stratBranch = new StratBranch
		{
			Name = "Defamation on me"
		};
		stratBranch.Conditions.Add(new StratCondition
		{
			Kind = CondKind.TetherOnMe,
			TetherId = 368u,
			TetherName = "Defamation"
		});
		FillTether(stratBranch, 368u, QuickShape.Donut, Defa, 4f);
		StratBranch stratBranch2 = new StratBranch
		{
			Name = "Stack on me"
		};
		stratBranch2.Conditions.Add(new StratCondition
		{
			Kind = CondKind.TetherOnMe,
			TetherId = 369u,
			TetherName = "Stack"
		});
		FillTether(stratBranch2, 369u, QuickShape.Circle, Stack, 3f);
		stratSlide.Branches.Add(stratBranch);
		stratSlide.Branches.Add(stratBranch2);
		return stratSlide;
	}

	private static StratSlide TowerRoleStep()
	{
		StratSlide stratSlide = NewSlide("Towers — light / non-light", 46367u);
		StratBranch stratBranch = new StratBranch
		{
			Name = "DPS (light)"
		};
		stratBranch.Conditions.Add(new StratCondition
		{
			Kind = CondKind.MyRole,
			Role = RoleCat.Dps
		});
		FillUniform(stratBranch, new Vector3(81.757f, 0f, 95.757f), QuickShape.Tower, Tower, 2.5f);
		StratBranch stratBranch2 = new StratBranch
		{
			Name = "Tank/Healer (non-light)"
		};
		stratBranch2.RequireAll = false;
		stratBranch2.Conditions.Add(new StratCondition
		{
			Kind = CondKind.MyRole,
			Role = RoleCat.Tank
		});
		stratBranch2.Conditions.Add(new StratCondition
		{
			Kind = CondKind.MyRole,
			Role = RoleCat.Healer
		});
		FillUniform(stratBranch2, new Vector3(90.243f, 0f, 95.757f), QuickShape.Tower, Tower, 2.5f);
		stratSlide.Branches.Add(stratBranch);
		stratSlide.Branches.Add(stratBranch2);
		return stratSlide;
	}

	private static StratSlide NearFarStep()
	{
		StratSlide stratSlide = NewSlide("Near / Far bait", 46324u);
		StratBranch stratBranch = new StratBranch
		{
			Name = "Given Far"
		};
		stratBranch.Conditions.Add(new StratCondition
		{
			Kind = CondKind.MyStatus,
			StatusId = 4766u,
			StatusName = "Given Far"
		});
		FillUniform(stratBranch, new Vector3(110.152f, 0f, 98.237f), QuickShape.Circle, Bait, 2f);
		StratBranch stratBranch2 = new StratBranch
		{
			Name = "Given Near"
		};
		stratBranch2.Conditions.Add(new StratCondition
		{
			Kind = CondKind.MyStatus,
			StatusId = 4767u,
			StatusName = "Given Near"
		});
		FillUniform(stratBranch2, new Vector3(106.973f, 0f, 94.048f), QuickShape.Circle, Bait, 2f);
		StratBranch stratBranch3 = new StratBranch
		{
			Name = "Default (taken)"
		};
		FillUniform(stratBranch3, new Vector3(114.708f, 0f, 109.144f), QuickShape.Circle, Bait, 2f);
		stratSlide.Branches.Add(stratBranch);
		stratSlide.Branches.Add(stratBranch2);
		stratSlide.Branches.Add(stratBranch3);
		return stratSlide;
	}

	private static StratSlide NewSlide(string name, uint actionId)
	{
		return new StratSlide
		{
			Name = name,
			On = TriggerMatch.Cast,
			MatchById = true,
			DataId = actionId,
			Pattern = name
		};
	}

	private static RoleSpot Spot(StratRole role, Vector3 pos, QuickShape shape, Vector4 color, float radius)
	{
		return new RoleSpot
		{
			Role = role,
			Position = pos,
			Shape = shape,
			Color = color,
			Radius = radius,
			Duration = 10f,
			ShowLeash = true
		};
	}

	private static void FillUniform(StratBranch b, Vector3 pos, QuickShape shape, Vector4 color, float radius)
	{
		for (int i = 0; i < 8; i++)
		{
			b.Spots.Add(Spot((StratRole)i, pos, shape, color, radius));
		}
	}

	private static void FillTether(StratBranch b, uint tetherId, QuickShape shape, Vector4 color, float radius)
	{
		for (int i = 0; i < 8; i++)
		{
			RoleSpot roleSpot = Spot((StratRole)i, new Vector3(100f, 0f, 100f), shape, color, radius);
			roleSpot.Anchor = SpotAnchor.TetheredToMe;
			roleSpot.TetherId = tetherId;
			b.Spots.Add(roleSpot);
		}
	}
}
