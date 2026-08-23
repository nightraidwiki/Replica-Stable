using System.Collections.Generic;
using Lumina.Excel.Sheets;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.LockWyvernEx;

public class DragonSVoiceDonut : ISpecialAction
{
	public override string Name => "Dragon's Voice (donut)";

	public override HashSet<uint> ActionID => new HashSet<uint> { 43911u, 43912u, 43913u, 43914u };

	public override void OnActionCast(ActorCastInfo info)
	{
		float num3;
		float num4;
		(num3, num4) = info.ActionId switch
		{
			43911 => (7500, 0), 
			43912 => (2000, 7500), 
			43913 => (2000, 9500), 
			43914 => (2000, 11500), 
			_ => default((int, int)), 
		};
		if (num3 != 0f || num4 != 0f)
		{
			Action row = Svc.Data.GetExcelSheet<Action>().GetRow(info.ActionId);
			DrawElement drawElement = new DrawElement();
			Action action = row;
			Omen value = action.Omen.Value;
			Omen omen = value;
			drawElement.drawAvfx = omen.Path.ToString();
			drawElement.Position = info.Pos;
			drawElement.drawOnObject = false;
			action = row;
			drawElement.radiusX = (int)action.EffectRange;
			action = row;
			drawElement.radiusZ = (int)action.EffectRange;
			drawElement.delayDrawTime = num4;
			drawElement.destroyTime = num3;
			DrawManager.Draw(drawElement);
		}
	}
}
