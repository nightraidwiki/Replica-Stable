using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.JeunoArc1;

public class CataclysmicSpiral : ISpecialAction
{
	private Angle increment;

	private readonly List<Angle> rotation = new List<Angle>(3);

	public override string Name => "Cataclysmic Spiral";

	public override HashSet<uint> ActionID => new HashSet<uint> { 41070u };

	public override uint Phase => 3u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		increment = icon switch
		{
			167u => -30f.Degrees(), 
			168u => 30f.Degrees(), 
			_ => default(Angle), 
		};
		InitIfReady();
	}

	private void InitIfReady()
	{
		if (rotation.Count != 3 || !(increment != default(Angle)))
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 1; j < 8; j++)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan030_1bf",
					Position = new Vector3(865f, -750f, -820f),
					drawOnObject = false,
					radiusX = 30f,
					radiusZ = 30f,
					refRotation = rotation[i] + j * increment,
					fixRotation = true,
					destroyTime = ((j == 1) ? 6700 : 1200),
					delayDrawTime = ((j != 1) ? (5500 + (j - 1) * 1200) : 0)
				}, Svc.Objects.LocalPlayer);
			}
		}
		rotation.Clear();
		increment = default(Angle);
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		rotation.Add(info.Facing);
		InitIfReady();
	}
}
