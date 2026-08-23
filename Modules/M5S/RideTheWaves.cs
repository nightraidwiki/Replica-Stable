using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Vfx;

namespace Replica.Modules.M5S;

public class RideTheWaves : ISpecialAction
{
	private enum AOEShapeRect
	{
		Full,
		Mid,
		Short
	}

	public override string Name => "Ride the Waves";

	public override HashSet<uint> ActionID => new HashSet<uint> { 42837u };

	public override void OnEnvControl(byte index, uint state)
	{
		if (index == 4 && aoes.Count == 0)
		{
			base.NumCasts = 0;
			(AOEShapeRect, float, float)[] array = new(AOEShapeRect, float, float)[8]
			{
				(AOEShapeRect.Full, 82.5f, 80f),
				(AOEShapeRect.Full, 87.5f, 80f),
				(AOEShapeRect.Mid, 92.5f, 100f),
				(AOEShapeRect.Short, 97.5f, 80f),
				(AOEShapeRect.Full, 102.5f, 80f),
				(AOEShapeRect.Mid, 107.5f, 100f),
				(AOEShapeRect.Short, 112.5f, 80f),
				(AOEShapeRect.Full, 117.5f, 80f)
			};
			for (int i = 0; i < 8; i++)
			{
				ref(AOEShapeRect, float, float) reference = ref array[i];
				float x = state switch
				{
					33554944u => reference.Item2, 
					8388736u => 200f - reference.Item2, 
					_ => 0f, 
				};
				DrawElement drawElement = new DrawElement();
				drawElement.drawAvfx = "general02wf";
				drawElement.Position = new Vector3(x, 0f, reference.Item3 - 35f);
				drawElement.drawOnObject = false;
				drawElement.radiusX = 2.5f;
				DrawElement drawElement2 = drawElement;
				drawElement2.radiusZ = reference.Item1 switch
				{
					AOEShapeRect.Full => 40f, 
					AOEShapeRect.Mid => 20f, 
					AOEShapeRect.Short => 15f, 
					_ => 0f, 
				};
				drawElement.hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 42837u },
					TargetHitCount = 16
				};
				DrawElement element = drawElement;
				aoes.Add(DrawManager.Draw(element));
			}
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		base.NumCasts++;
		if (base.NumCasts > 15)
		{
			aoes.Clear();
		}
		else if (aoes.Count > 0)
		{
			for (int i = 0; i < aoes.Count; i++)
			{
				StaticVfx staticVfx = aoes[i];
				Vector3 position = staticVfx.Position;
				staticVfx.Position = new Vector3(position.X, 0f, position.Z + 5f);
			}
		}
	}
}
