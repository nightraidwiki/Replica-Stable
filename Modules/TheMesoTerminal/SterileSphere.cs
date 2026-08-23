using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.TheMesoTerminal;

public class SterileSphere : ISpecialAction
{
	public override string Name => "Sterile Sphere";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnEnvControl(byte index, uint state)
	{
		if (state != 131073)
		{
			return;
		}
		int num;
		switch (index)
		{
		case 11:
		case 12:
		case 13:
		case 14:
			num = 15;
			break;
		case 15:
		case 16:
		case 17:
		case 18:
			num = 8;
			break;
		default:
			num = 0;
			break;
		}
		if (num != 0)
		{
			Vector3 pos;
			switch (index)
			{
			case 11:
			case 15:
				pos = new Vector3(260f, -582.5f, 2f);
				break;
			case 12:
			case 16:
				pos = new Vector3(280f, -582.5f, 2f);
				break;
			case 13:
			case 17:
				pos = new Vector3(260f, -582.5f, 22f);
				break;
			default:
				pos = new Vector3(280f, -582.5f, 22f);
				break;
			}
			SimpleElement.Circle(pos, num, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 43805u, 43806u }
			});
		}
	}
}
