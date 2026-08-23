using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ForkedTower;

public class CriticalAxeblow : ISpecialAction
{
	private readonly (Vector3 pos, float rot)[] groundPos = new(Vector3, float)[3]
	{
		(new Vector3(700.0001f, -476f, -659.5043f), -0.7854581f),
		(new Vector3(712.5536f, -476.00006f, -681.2478f), -0.26188326f),
		(new Vector3(687.4426f, -476f, -681.25f), 1.8325121f)
	};

	public override string Name => "Critical Axeblow/Lanceblow";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41543u, 41547u };

	public override uint Phase => 4u;

	public override void OnActionCast(ActorCastInfo info)
	{
		switch (info.ActionId)
		{
		case 41543:
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = info.Pos,
				drawOnObject = false,
				radiusX = 20f,
				radiusZ = 20f,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41545u }
				}
			});
			(Vector3, float)[] array2 = groundPos;
			for (int j = 0; j < array2.Length; j++)
			{
				(Vector3, float) tuple2 = array2[j];
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_x02pf",
					Position = tuple2.Item1,
					drawOnObject = false,
					radiusX = 10f,
					radiusZ = 10f,
					refRotation = tuple2.Item2.Radians(),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 41545u }
					}
				});
			}
			break;
		}
		case 41547:
		{
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customDonut",
				Position = info.Pos,
				drawOnObject = false,
				refRadian = 0.3125f,
				radiusX = 32f,
				radiusZ = 32f,
				refColor = GroundOmen.enemyColor,
				refTargetColor = GroundOmen.enemyColor,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 41550u }
				}
			});
			(Vector3, float)[] array = groundPos;
			for (int i = 0; i < array.Length; i++)
			{
				(Vector3, float) tuple = array[i];
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_x02f",
					Position = tuple.Item1,
					drawOnObject = false,
					radiusX = 10f,
					radiusZ = 10f,
					refRotation = tuple.Item2.Radians(),
					hitCounter = new HitCounter
					{
						ActionID = new HashSet<uint> { 41549u }
					}
				});
			}
			break;
		}
		}
	}
}
