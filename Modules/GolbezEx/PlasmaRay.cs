using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.GolbezEx;

public class PlasmaRay : ISpecialAction
{
	public uint Car = 1u;

	public override string Name => "Plasma Ray";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnObjectCreatedEvent(IGameObject GameObject)
	{
		if (GameObject.BaseId != 19000 || GameObject.Position.Y != 0f)
		{
			return;
		}
		int num = 30;
		if (Car == 2)
		{
			if (GameObject.Position.X == 92.5f)
			{
				num = 20;
			}
			else if (GameObject.Position.X == 107.5f)
			{
				num = 10;
			}
		}
		SimpleElement.RectangleMdl(GameObject, num, 2.5f, 0f, GameObject.Rotation.Radians(), 3000f, 0f, new HitCounter
		{
			ActionID = new HashSet<uint> { 45671u, 45672u }
		});
	}

	public override void OnEnvControl(byte index, uint state)
	{
		switch (index)
		{
		case 4:
			if (state == 131073)
			{
				Car = 2u;
			}
			break;
		case 5:
			if (state == 131073)
			{
				Car = 3u;
			}
			break;
		}
	}

	public override void Reset()
	{
		Car = 1u;
		base.Reset();
	}
}
