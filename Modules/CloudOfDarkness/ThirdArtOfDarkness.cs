using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.CloudOfDarkness;

public class ThirdArtOfDarkness : ISpecialAction
{
	public override string Name => "Third Art of Darkness";

	public override HashSet<uint> ActionID => new HashSet<uint> { 40480u, 40483u };

	public override uint Phase => 3u;

	public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
	{
		if (target.BaseId != 17951)
		{
			return;
		}
		base.NumCasts++;
		switch (icon)
		{
		case 239u:
		case 240u:
			DrawManager.Draw(new DrawElement
			{
				drawAvfx = "gl_fan180_1bf",
				Position = target.Position,
				drawOnObject = false,
				radiusX = 15f,
				radiusZ = 15f,
				refRotation = target.Rotation.Radians() + ((icon == 239) ? 90.Degrees() : (-90.Degrees())),
				destroyTime = ((base.NumCasts <= 2) ? 9500 : 3000),
				delayDrawTime = ((base.NumCasts > 2) ? 6500 : 0)
			});
			break;
		case 241u:
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bpxf",
					radiusX = 3f,
					radiusZ = 3f,
					drawOnObject = true,
					destroyTime = ((base.NumCasts <= 2) ? 9500 : 3000),
					delayDrawTime = ((base.NumCasts > 2) ? 6500 : 0),
					distanceCheck = new DistanceCheck
					{
						Position = target.Position,
						CheckType = 6,
						Count = 3
					}
				}, allPlayer);
			}
			break;
		}
		case 242u:
		{
			foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general02xf",
					Position = target.Position,
					drawOnObject = false,
					radiusX = 2.5f,
					radiusZ = 22f,
					target = allPlayer2,
					destroyTime = ((base.NumCasts <= 2) ? 9500 : 3000),
					delayDrawTime = ((base.NumCasts > 2) ? 6500 : 0),
					distanceCheck = new DistanceCheck
					{
						Position = target.Position,
						CheckType = 4,
						Count = 6
					}
				});
			}
			break;
		}
		}
	}

	public override void OnActionCast(ActorCastInfo info)
	{
		base.NumCasts = 0;
	}
}
