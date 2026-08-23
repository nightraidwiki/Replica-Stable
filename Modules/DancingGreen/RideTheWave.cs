using System;
using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.DancingGreen;

public class RideTheWave : ISpecialAction
{
	private int exaflaresStarted;

	public override string Name => "Ride the Wave";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42744u };

	public override void OnEnvControl(byte index, uint state)
	{
		if (index != 4)
		{
			return;
		}
		int[] array = Array.Empty<int>();
		switch (state)
		{
		case 67109888u:
			array = new int[7] { 0, 2, 3, 4, 5, 6, 7 };
			break;
		case 131074u:
		case 134219776u:
			array = new int[7] { 0, 1, 3, 4, 5, 6, 7 };
			break;
		case 1048592u:
		case 268439552u:
			array = new int[7] { 0, 1, 2, 4, 5, 6, 7 };
			break;
		case 2097184u:
		case 536879104u:
			array = new int[7] { 0, 1, 2, 3, 5, 6, 7 };
			break;
		case 4194368u:
		case 1073758208u:
			array = new int[7] { 0, 1, 2, 3, 4, 6, 7 };
			break;
		case 2147516416u:
			array = new int[7] { 0, 1, 2, 3, 4, 5, 7 };
			break;
		case 524292u:
			exaflaresStarted++;
			break;
		}
		if (array.Length != 0)
		{
			for (int i = 0; i < 7; i++)
			{
				List<StaticVfx> list = aoes;
				Vector3 pos = new Vector3(82.5f + (float)(array[i] * 5), 0f, 70f);
				HitCounter hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42744u },
					TargetHitCount = 15
				};
				list.Add(SimpleElement.Rectangle(pos, 15f, 2.5f, 0f, default(Angle), 3000f, 0f, hitCounter));
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		int numCasts = base.NumCasts + 1;
		base.NumCasts = numCasts;
		if (base.NumCasts > 15)
		{
			aoes.ForEach(delegate(StaticVfx x)
			{
				x.Remove();
			});
			aoes.Clear();
			return;
		}
		int num = ((exaflaresStarted == 1) ? 7 : 14);
		for (int num2 = 0; num2 < num; num2++)
		{
			StaticVfx staticVfx = aoes[num2];
			Vector3 position = staticVfx.Position;
			staticVfx.Position = new Vector3(position.X, 0f, position.Z + 5f);
		}
	}

	public override void Reset()
	{
		exaflaresStarted = 0;
		base.Reset();
	}
}
