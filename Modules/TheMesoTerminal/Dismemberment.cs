using System;
using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.TheMesoTerminal;

public class Dismemberment : ISpecialAction
{
	public override string Name => "Dismemberment";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43587u };

	public override uint Phase => 2u;

	public override IEnumerable<StaticVfx> ActiveAOEs
	{
		get
		{
			int count = aoes.Count;
			if (count == 0)
			{
				return Array.Empty<StaticVfx>();
			}
			long drawTime = aoes[0].DrawTime;
			int i;
			for (i = 0; i < count && aoes[i].DrawTime - drawTime < 1000; i++)
			{
			}
			return aoes.Slice(0, i);
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		DrawElement element = new DrawElement
		{
			Enable = false,
			Position = info.Pos,
			drawOnObject = false,
			drawAvfx = "general02xf",
			radiusX = 2f,
			radiusZ = 16f,
			refRotation = info.Facing,
			fixRotation = true,
			destroyTime = Environment.TickCount64
		};
		aoes.Add(DrawManager.Draw(element));
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (aoes.Count > 0)
		{
			aoes[0].Remove();
			aoes.RemoveAt(0);
		}
	}
}
