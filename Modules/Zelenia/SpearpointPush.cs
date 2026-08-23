using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Enum;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Util;

namespace Replica.Modules.Zelenia;

public class SpearpointPush : ISpecialAction
{
	public override string Name => "SpearpointPush";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
	{
		if (source.BaseId == 18378 && id - 3216 <= 1)
		{
			DrawManager.Draw(new DrawElement
			{
				drawType = ElementType.Omen,
				drawAvfx = "general02xf",
				radiusX = 33f,
				radiusZ = 35f,
				drawOnObject = true,
				refRotation = ((id == 3217) ? 90 : (-90)).Degrees(),
				hitCounter = new HitCounter
				{
					ActionID = new HashSet<uint> { 43187u, 43188u }
				}
			}, source);
		}
	}
}
