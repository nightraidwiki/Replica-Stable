using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.TEA;

public class Inception : ISpecialAction
{
	public override string Name => "Sacrament";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18526u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawElement obj = new DrawElement
		{
			drawAvfx = "general_x02f",
			radiusX = 8f,
			radiusZ = 100f,
			drawOnObject = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18527u }
			}
		};
		DrawManager.Draw(obj, info.Source);
		obj.refRotation = 90.Degrees();
		DrawManager.Draw(obj, info.Source);
	}
}
