using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.FRU;

public class DiamondDustFacingGuide : ISpecialAction
{
	public override string Name => "Diamond Dust (facing guide)";

	public override uint Phase => 2u;

	public override uint WeatherID => 35u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 40185u };

	public unsafe override void Update()
	{
		if (aoes.Count != 0)
		{
			CameraEx* camera = (CameraEx*)CameraManager.Instance()->Camera;
			float radians = PlayerHelper.CameraDirHToCharaRotation(camera->DirH);
			aoes[0].Rotation = radians.Radians();
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		DrawElement element = new DrawElement
		{
			drawAvfx = "e5d1_b1_kblaser_t1",
			radiusX = 1f,
			radiusZ = 32f,
			drawOnObject = true,
			fixRotation = true,
			hitCounter = new HitCounter
			{
				ActionID = new HashSet<uint> { 40193u, 40194u, 40195u, 40196u },
				TargetHitCount = 2
			}
		};
		aoes.Add(DrawManager.Draw(element, Svc.Objects.LocalPlayer));
	}
}
