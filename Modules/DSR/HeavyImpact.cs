using System.Collections.Generic;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DSR;

public class HeavyImpact : ISpecialAction
{
	public override string Name => "Heavy Impact";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 25558u, 25559u, 25560u, 25561u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		switch (info.ActionId)
		{
		case 25558u:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customDonut",
				radiusX = 12f,
				radiusZ = 12f,
				refRadian = 0.5f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25559u }
				}
			}, info.Source);
			break;
		case 25559u:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customDonut",
				radiusX = 18f,
				radiusZ = 18f,
				refRadian = 2f / 3f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25560u }
				}
			}, info.Source);
			break;
		case 25560u:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customDonut",
				radiusX = 24f,
				radiusZ = 24f,
				refRadian = 0.75f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25561u }
				}
			}, info.Source);
			break;
		case 25561u:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "customDonut",
				radiusX = 30f,
				radiusZ = 30f,
				refRadian = 0.8f,
				drawOnObject = true,
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 25562u }
				}
			}, info.Source);
			break;
		}
	}
}
