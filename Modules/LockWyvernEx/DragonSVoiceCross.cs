using System;
using System.Collections.Generic;
using System.Linq;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.LockWyvernEx;

public class DragonSVoiceCross : ISpecialAction
{
	public override string Name => "Dragon's Voice (cross)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43944u, 43945u, 43946u };

	public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(4);

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 43944)
		{
			switch ((int)info.Facing.Deg)
			{
			case 44:
			{
				Angle item3 = 44.998f.Degrees();
				Angle item4 = -45.003f.Degrees();
				(WPos, Angle)[] array2 = new(WPos, Angle)[58]
				{
					(new WPos(85.832f, 85.832f), item3),
					(new WPos(114.122f, 85.832f), item4),
					(new WPos(109.88f, 81.59f), item4),
					(new WPos(90.074f, 81.59f), item3),
					(new WPos(81.59f, 90.074f), item3),
					(new WPos(118.364f, 90.074f), item4),
					(new WPos(121.202f, 92.912f), item4),
					(new WPos(107.042f, 78.782f), item4),
					(new WPos(78.782f, 92.912f), item3),
					(new WPos(92.912f, 78.782f), item3),
					(new WPos(95.75f, 75.944f), item3),
					(new WPos(124.041f, 95.75f), item4),
					(new WPos(104.234f, 75.944f), item4),
					(new WPos(75.944f, 95.75f), item3),
					(new WPos(126.848f, 98.558f), item4),
					(new WPos(101.396f, 73.106f), item4),
					(new WPos(98.558f, 73.106f), item3),
					(new WPos(73.106f, 98.558f), item3),
					(new WPos(126.848f, 98.558f), item4),
					(new WPos(101.396f, 73.106f), item4),
					(new WPos(98.558f, 73.106f), item3),
					(new WPos(73.106f, 98.558f), item3),
					(new WPos(104.234f, 75.944f), item4),
					(new WPos(124.041f, 95.75f), item4),
					(new WPos(95.75f, 75.944f), item3),
					(new WPos(75.944f, 95.75f), item3),
					(new WPos(78.782f, 92.912f), item3),
					(new WPos(107.042f, 78.782f), item4),
					(new WPos(121.202f, 92.912f), item4),
					(new WPos(92.912f, 78.782f), item3),
					(new WPos(109.88f, 81.59f), item4),
					(new WPos(81.59f, 90.074f), item3),
					(new WPos(118.364f, 90.074f), item4),
					(new WPos(90.074f, 81.59f), item3),
					(new WPos(112.718f, 84.428f), item4),
					(new WPos(115.526f, 87.266f), item4),
					(new WPos(87.266f, 84.428f), item3),
					(new WPos(84.428f, 87.266f), item3),
					(new WPos(84.428f, 87.266f), item3),
					(new WPos(115.526f, 87.266f), item4),
					(new WPos(112.718f, 84.428f), item4),
					(new WPos(87.266f, 84.428f), item3),
					(new WPos(109.88f, 81.59f), item4),
					(new WPos(118.364f, 90.074f), item4),
					(new WPos(90.074f, 81.59f), item3),
					(new WPos(81.59f, 90.074f), item3),
					(new WPos(121.202f, 92.912f), item4),
					(new WPos(78.782f, 92.912f), item3),
					(new WPos(107.042f, 78.782f), item4),
					(new WPos(92.912f, 78.782f), item3),
					(new WPos(104.234f, 75.944f), item4),
					(new WPos(124.041f, 95.75f), item4),
					(new WPos(95.75f, 75.944f), item3),
					(new WPos(75.944f, 95.75f), item3),
					(new WPos(73.106f, 98.558f), item3),
					(new WPos(126.848f, 98.558f), item4),
					(new WPos(101.396f, 73.106f), item4),
					(new WPos(98.558f, 73.106f), item3)
				};
				AddAOEs(array2);
				break;
			}
			case -89:
			{
				Angle item = -179.984f.Degrees();
				Angle item2 = -89.982f.Degrees();
				(WPos, Angle)[] array = new(WPos, Angle)[58]
				{
					(new WPos(99.992f, 119.982f), item),
					(new WPos(119.982f, 99.992f), item2),
					(new WPos(106.004f, 119.982f), item),
					(new WPos(93.98f, 119.982f), item),
					(new WPos(119.982f, 105.974f), item2),
					(new WPos(119.982f, 93.98f), item2),
					(new WPos(89.982f, 119.982f), item),
					(new WPos(110.002f, 119.982f), item),
					(new WPos(119.982f, 109.972f), item2),
					(new WPos(119.982f, 89.982f), item2),
					(new WPos(85.985f, 119.982f), item),
					(new WPos(119.982f, 85.985f), item2),
					(new WPos(114f, 119.982f), item),
					(new WPos(119.982f, 113.97f), item2),
					(new WPos(119.982f, 81.987f), item2),
					(new WPos(81.987f, 119.982f), item),
					(new WPos(117.998f, 119.982f), item),
					(new WPos(119.982f, 117.968f), item2),
					(new WPos(81.987f, 119.982f), item),
					(new WPos(117.998f, 119.982f), item),
					(new WPos(119.982f, 117.968f), item2),
					(new WPos(119.982f, 81.987f), item2),
					(new WPos(85.985f, 119.982f), item),
					(new WPos(114f, 119.982f), item),
					(new WPos(119.982f, 85.985f), item2),
					(new WPos(119.982f, 113.97f), item2),
					(new WPos(110.002f, 119.982f), item),
					(new WPos(119.982f, 89.982f), item2),
					(new WPos(89.982f, 119.982f), item),
					(new WPos(119.982f, 109.972f), item2),
					(new WPos(106.004f, 119.982f), item),
					(new WPos(119.982f, 105.974f), item2),
					(new WPos(93.98f, 119.982f), item),
					(new WPos(119.982f, 93.98f), item2),
					(new WPos(102.007f, 119.982f), item),
					(new WPos(97.978f, 119.982f), item),
					(new WPos(119.982f, 97.978f), item2),
					(new WPos(119.982f, 101.976f), item2),
					(new WPos(119.982f, 101.976f), item2),
					(new WPos(102.007f, 119.982f), item),
					(new WPos(97.978f, 119.982f), item),
					(new WPos(119.982f, 97.978f), item2),
					(new WPos(93.98f, 119.982f), item),
					(new WPos(119.982f, 93.98f), item2),
					(new WPos(106.004f, 119.982f), item),
					(new WPos(119.982f, 105.974f), item2),
					(new WPos(89.982f, 119.982f), item),
					(new WPos(119.982f, 109.972f), item2),
					(new WPos(119.982f, 89.982f), item2),
					(new WPos(110.002f, 119.982f), item),
					(new WPos(85.985f, 119.982f), item),
					(new WPos(114f, 119.982f), item),
					(new WPos(119.982f, 113.97f), item2),
					(new WPos(119.982f, 85.985f), item2),
					(new WPos(81.987f, 119.982f), item),
					(new WPos(119.982f, 117.968f), item2),
					(new WPos(117.998f, 119.982f), item),
					(new WPos(119.982f, 81.987f), item2)
				};
				AddAOEs(array);
				break;
			}
			}
		}
		void AddAOEs((WPos pos, Angle rot)[] aoes)
		{
			for (int i = 0; i < 58; i++)
			{
				ref(WPos, Angle) reference = ref aoes[i];
				double num = ((i >= 26) ? ((i >= 42) ? ((i >= 50) ? ((i >= 54) ? 35.9 : 33.3) : ((i >= 46) ? 30.7 : 28.1)) : ((i >= 34) ? ((i >= 38) ? 25.5 : 22.9) : ((i >= 30) ? 20.3 : 17.7))) : ((i >= 10) ? ((i >= 18) ? ((i >= 22) ? 15.1 : 13.0) : ((i >= 14) ? 10.4 : 7.8)) : ((i < 2) ? 0.0 : ((i >= 6) ? 5.2 : 2.6))));
				if (i < 2)
				{
					this.aoes.Add(SimpleElement.Rectangle(reference.Item1.ToVec3(), 40f, 4f, 0f, reference.Item2, (float)((double)Environment.TickCount64 + num * 1000.0)));
				}
				else
				{
					this.aoes.Add(SimpleElement.Rectangle(reference.Item1.ToVec3(), 40f, 2f, 0f, reference.Item2, (float)((double)Environment.TickCount64 + num * 1000.0)));
				}
				this.aoes.SortBy((StaticVfx c) => c.DrawTime);
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts++;
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}
