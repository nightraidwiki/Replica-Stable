using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;
using Replica.Engine.Vfx;

namespace Replica.Modules.M8S;

public class ChampionsCircuit : ISpecialAction
{
	private enum Mechanic
	{
		None,
		Clockwise,
		Counterclockwise
	}

	private static readonly HashSet<uint> ChampionsCircuitFirst = new HashSet<uint> { 42105u, 42106u, 42107u, 42108u, 42109u };

	private static readonly HashSet<uint> ChampionsCircuitRest = new HashSet<uint> { 42110u, 42111u, 42112u, 42113u, 42114u };

	private Mechanic mechanic;

	public override string Name => "Champion's Circuit";

	public override uint Phase => 2u;

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (uint item in ChampionsCircuitFirst)
			{
				hashSet.Add(item);
			}
			foreach (uint item2 in ChampionsCircuitRest)
			{
				hashSet.Add(item2);
			}
			return hashSet;
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (ChampionsCircuitFirst.Contains(info.ActionId) && mechanic != Mechanic.None)
		{
			string drawAvfx = Svc.Data.GetExcelSheet<Action>().GetRow(info.ActionId).Omen.Value.Path.ExtractText();
			DrawElement drawElement = new DrawElement();
			drawElement.drawAvfx = drawAvfx;
			DrawElement drawElement2 = drawElement;
			drawElement2.radiusX = info.ActionId switch
			{
				42105u => 6, 
				42106u => 13, 
				42107u => 28, 
				42108u => 22, 
				42109u => 28, 
				_ => 0, 
			};
			drawElement2 = drawElement;
			drawElement2.radiusZ = info.ActionId switch
			{
				42105u => 30, 
				42106u => 13, 
				42107u => 28, 
				42108u => 22, 
				42109u => 28, 
				_ => 0, 
			};
			drawElement.drawOnObject = true;
			drawElement.refRotation = info.Rotation + ((mechanic == Mechanic.Clockwise) ? (-72.Degrees()) : 72.Degrees());
			drawElement.fixRotation = true;
			DrawElement drawElement3 = drawElement;
			HitCounter hitCounter = new HitCounter();
			HitCounter hitCounter2 = hitCounter;
			hitCounter2.ActionID = info.ActionId switch
			{
				42105u => new HashSet<uint> { 42110u }, 
				42106u => new HashSet<uint> { 42111u }, 
				42107u => new HashSet<uint> { 42112u }, 
				42108u => new HashSet<uint> { 42113u }, 
				42109u => new HashSet<uint> { 42114u }, 
				_ => new HashSet<uint> { 0u }, 
			};
			hitCounter.TargetHitCount = 4;
			drawElement3.hitCounter = hitCounter;
			DrawElement drawElement4 = drawElement;
			if (info.ActionId == 42106)
			{
				Vector2 offset = new Vector2(info.Source.Position.X, info.Source.Position.Z) - new Vector2(100f, 100f);
				Vector2 vector = new Vector2(100f, 100f) + offset.RotationDegress(72f, mechanic == Mechanic.Clockwise);
				drawElement4.Position = new Vector3(vector.X, -150f, vector.Y);
				drawElement4.drawOnObject = false;
			}
			aoes.Add(DrawManager.Draw(drawElement4, info.Source));
		}
		else
		{
			if (!ChampionsCircuitRest.Contains(info.ActionId) || mechanic == Mechanic.None)
			{
				return;
			}
			base.NumCasts++;
			if (base.NumCasts % 5 != 0)
			{
				return;
			}
			int count = aoes.Count;
			for (int i = 0; i < count; i++)
			{
				StaticVfx staticVfx = aoes[i];
				if (staticVfx.Owner == null)
				{
					Vector2 offset2 = new Vector2(staticVfx.Position.X, staticVfx.Position.Z) - new Vector2(100f, 100f);
					Vector2 vector2 = new Vector2(100f, 100f) + offset2.RotationDegress(72f, mechanic == Mechanic.Clockwise);
					staticVfx.Position = new Vector3(vector2.X, -150f, vector2.Y);
				}
				staticVfx.Rotation += ((mechanic == Mechanic.Clockwise) ? (-72.Degrees()) : 72.Degrees());
			}
		}
	}

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (icon - 501 <= 1 && target.BaseId == 18222)
		{
			if (icon == 501)
			{
				mechanic = Mechanic.Clockwise;
			}
			else
			{
				mechanic = Mechanic.Counterclockwise;
			}
		}
	}

	public override void Reset()
	{
		mechanic = Mechanic.None;
		base.Reset();
	}
}
