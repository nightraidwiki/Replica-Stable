using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.AlexandriaDt;

public class SphereShatter : ISpecialAction
{
	public override string Name => "Sphere Shatter";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42545u, 43135u };

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

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId == 18322)
		{
			DrawElement element = new DrawElement
			{
				drawAvfx = "general_1bxf",
				Position = GameObject.Position,
				drawOnObject = false,
				radiusX = 6f,
				radiusZ = 6f,
				destroyTime = Environment.TickCount64
			};
			aoes.Add(DrawManager.Draw(element));
		}
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
