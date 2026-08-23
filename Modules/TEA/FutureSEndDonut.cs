using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.TEA;

public class FutureSEndDonut : ISpecialAction
{
	public override string Name => "Future's Endβ Donut";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 18590u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "customDonut",
			refRadian = 2f / 15f,
			radiusX = 60f,
			radiusZ = 60f,
			drawOnObject = true,
			refColor = Vector4.One,
			refTargetColor = Vector4.One,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 18566u }
			}
		};
		DrawQueue.Enqueue((new HashSet<uint> { 18861u, 18862u }, new(IGameObject, DrawElement[])[1] { (info.Source, new DrawElement[1] { drawElement }) }));
	}
}
