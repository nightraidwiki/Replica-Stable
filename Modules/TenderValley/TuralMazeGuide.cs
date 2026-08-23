using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Vfx;

namespace Replica.Modules.TenderValley;

public class TuralMazeGuide : ISpecialAction
{
	public override string Name => "Tural Maze Guide";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override uint Phase => 3u;

	public override void OnEnvControl(byte index, uint state)
	{
		if (index != 1)
		{
			return;
		}
		DrawElement drawElement = new DrawElement
		{
			drawAvfx = "share_trap01k1",
			drawOnObject = false,
			radiusX = 2f,
			radiusY = 5f,
			radiusZ = 2f,
			destroyTime = 10000f
		};
		switch (state)
		{
		case 16777344u:
			drawElement.Position = new Vector3(-124f, -170f, -552f);
			aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
			break;
		case 67109376u:
			drawElement.Position = new Vector3(-128f, -170f, -560f);
			aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
			break;
		case 268437504u:
			drawElement.Position = new Vector3(-132f, -170f, -548f);
			aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
			break;
		case 131073u:
			drawElement.Position = new Vector3(-136f, -170f, -556f);
			aoes.Add(DrawManager.Draw(drawElement, Svc.Objects.LocalPlayer));
			break;
		case 524292u:
		case 1048580u:
		case 2097156u:
		case 4194308u:
			aoes.ForEach(delegate(StaticVfx a)
			{
				a.Remove();
			});
			aoes.Clear();
			break;
		}
	}
}
