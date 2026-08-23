using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.Enuo;

public class AddsPhase : ISpecialAction
{
	public override string Name => "Adds Phase & Line Stacks";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>
	{
		49982u, // LoomingEmptinessKnockback (KB 20f)
		49369u, // LoomingEmptinessKillzone (Circle 8f)
		50013u, // EmptyShadow (Tower 7f)
		50038u, // VoidalTurbulenceCone (Cone 60f, angle 60)
		50022u, // DemonEyeGaze (Gaze 20f)
		50021u, // WeightOfNothing (Rect 100x4 halfwidth)
		50017u, // Nothingness (Rect 100x2 halfwidth)
		50048u  // DimensionZeroHits (Rect 60x4 halfwidth)
	};

	public override void OnActionCast(ActorCastInfo info)
	{
		if (info.ActionId == 49982u)
		{
			// Looming Emptiness Knockback
			SimpleElement.KnockBack(info, 20f);
		}
		else if (info.ActionId == 49369u)
		{
			// Looming Emptiness Killzone
			SimpleElement.Circle(info, 8f);
		}
		else if (info.ActionId == 50013u)
		{
			// Empty Shadow Tower (6f or 7f radius)
			SimpleElement.Circle(info, 7f);
		}
		else if (info.ActionId == 50038u)
		{
			// Voidal Turbulence Cone (60 degrees)
			SimpleElement.Fan(info, 60);
		}
		else if (info.ActionId == 50022u)
		{
			// Demon Eye Gaze (20f radius eye warning)
			SimpleElement.Circle(info, 20f);
		}
		else if (info.ActionId == 50021u)
		{
			// Weight Of Nothing line stack (range 100, width 8 -> halfwidth 4)
			SimpleElement.Rectangle(info, 100f, 4f);
		}
		else if (info.ActionId == 50017u)
		{
			// Nothingness line (range 100, width 4 -> halfwidth 2)
			SimpleElement.Rectangle(info, 100f, 2f);
		}
		else if (info.ActionId == 50048u)
		{
			// Dimension Zero (range 60, width 8 -> halfwidth 4)
			SimpleElement.Rectangle(info, 60f, 4f);
		}
	}
}
