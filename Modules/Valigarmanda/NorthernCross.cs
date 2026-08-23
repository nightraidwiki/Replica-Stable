using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.Valigarmanda;

public class NorthernCross : ISpecialAction
{
	public override string Name => "Northern Cross";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnEnvControl(byte index, uint state)
	{
		if (index == 3)
		{
			Angle angle = state switch
			{
				2097168u => -90.Degrees(), 
				131073u => 90.Degrees(), 
				_ => default(Angle), 
			};
			if (!(angle == default(Angle)))
			{
				SimpleElement.Rectangle(new Vector3(100f, 0f, 100f), 25f, 30f, 0f, -127.Degrees() + angle, 3000f, 0f, new HitCounter
				{
					ActionID = new HashSet<uint> { 36827u, 36828u }
				});
			}
		}
	}
}
