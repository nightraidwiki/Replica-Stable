using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.ShishuDeep;

public class FairyPellyEWFourfoldFlameChase : ISpecialAction
{
	private enum SpinDir
	{
		Clockwise,
		Counterclockwise
	}

	private bool isLeft;

	private readonly List<SpinDir> direction = new List<SpinDir>();

	public override string Name => "Fairy Pelly E/W Fourfold Flame (chase)";

	public override HashSet<uint> ActionID
	{
		get
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			hashSet.Add(45461u);
			hashSet.Add(45462u);
			foreach (uint spinDanceId in spinDanceIds)
			{
				hashSet.Add(spinDanceId);
			}
			return hashSet;
		}
	}

	private static HashSet<uint> spinDanceIds => new HashSet<uint> { 45463u, 45464u, 45465u, 45466u };

	public override void OnActionCast(ActorCastInfo info)
	{
		if ((uint)(info.ActionId - 45461) <= 1u)
		{
			isLeft = info.ActionId == 45462;
			direction.Clear();
		}
	}

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (spinDanceIds.Contains(info.ActionId) && direction.Count > 0)
		{
			SpinDir spinDir = direction[0];
			direction.RemoveAt(0);
			if (aoes.Count > 0 && direction.Count > 0 && direction[0] != spinDir)
			{
				aoes[0].Rotation += 180.Degrees();
			}
		}
	}

	public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
	{
		if ((icon - 624 <= 1 || icon - 644 <= 1) && Source.BaseId == 19053)
		{
			switch (icon)
			{
			case 625u:
			case 645u:
				direction.Add(SpinDir.Counterclockwise);
				break;
			case 624u:
			case 644u:
				direction.Add(SpinDir.Clockwise);
				break;
			}
			if (direction.Count == 1)
			{
				Angle refRotation = direction[0] switch
				{
					SpinDir.Clockwise => isLeft ? 0.Degrees() : 180.Degrees(), 
					SpinDir.Counterclockwise => isLeft ? 180.Degrees() : 0.Degrees(), 
					_ => 0.Degrees(), 
				};
				DrawElement element = new DrawElement
				{
					drawAvfx = "gl_fan180_1bf",
					radiusX = 40f,
					radiusZ = 40f,
					refRotation = refRotation,
					hitCounter = new HitCounter
					{
						ActionID = spinDanceIds,
						TargetHitCount = 4
					}
				};
				aoes.Add(DrawManager.Draw(element, Source));
			}
		}
	}
}
