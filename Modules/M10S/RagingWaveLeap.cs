using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Managers;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M10S;

public class RagingWaveLeap : ISpecialAction
{
	private enum MarkerDirection
	{
		Up,
		Middle,
		Down
	}

	private static readonly Dictionary<uint, Vector3> PositionToCoord = new Dictionary<uint, Vector3>
	{
		{
			14u,
			new Vector3(87f, 0f, 87f)
		},
		{
			15u,
			new Vector3(100f, 0f, 87f)
		},
		{
			16u,
			new Vector3(113f, 0f, 87f)
		},
		{
			17u,
			new Vector3(87f, 0f, 100f)
		},
		{
			18u,
			new Vector3(100f, 0f, 100f)
		},
		{
			19u,
			new Vector3(113f, 0f, 100f)
		},
		{
			20u,
			new Vector3(87f, 0f, 113f)
		},
		{
			21u,
			new Vector3(100f, 0f, 113f)
		},
		{
			22u,
			new Vector3(113f, 0f, 113f)
		}
	};

	private static readonly Dictionary<ushort, MarkerDirection> Data2ToDirection = new Dictionary<ushort, MarkerDirection>
	{
		{
			2,
			MarkerDirection.Down
		},
		{
			32,
			MarkerDirection.Middle
		},
		{
			128,
			MarkerDirection.Up
		},
		{
			512,
			MarkerDirection.Down
		},
		{
			2048,
			MarkerDirection.Middle
		},
		{
			8192,
			MarkerDirection.Up
		}
	};

	private static readonly HashSet<ushort> FireMarkerData2 = new HashSet<ushort> { 512, 2048, 8192 };

	public override string Name => "Raging Wave Leap";

	public override HashSet<uint> ActionID => new HashSet<uint>();

	public override void OnMapEffect(uint a2, ushort a3, ushort a4)
	{
		if (a2 < 14 || a2 > 22 || !Data2ToDirection.TryGetValue(a4, out var _))
		{
			return;
		}
		Vector3 position = PositionToCoord[a2];
		switch (Data2ToDirection[a4])
		{
		case MarkerDirection.Up:
		{
			foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "general_1bzt",
					radiusX = 6f,
					radiusZ = 6f,
					drawOnObject = true,
					delayDrawTime = 2000f,
					distanceCheck = new DistanceCheck
					{
						CheckType = 8,
						Position = position,
						Count = 1,
						CheckObject = allPlayer
					},
					hitCounter = new HitCounter
					{
						ActionID = (FireMarkerData2.Contains(a4) ? new HashSet<uint> { 46585u } : new HashSet<uint> { 46586u })
					}
				}, allPlayer);
			}
			break;
		}
		case MarkerDirection.Middle:
		{
			foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan045_1bpxf",
					Position = position,
					drawOnObject = false,
					radiusX = 60f,
					radiusZ = 60f,
					target = allPlayer2,
					delayDrawTime = 2000f,
					distanceCheck = new DistanceCheck
					{
						CheckType = 8,
						Position = position,
						Count = 1,
						CheckObject = allPlayer2
					},
					hitCounter = new HitCounter
					{
						ActionID = (FireMarkerData2.Contains(a4) ? new HashSet<uint> { 46581u } : new HashSet<uint> { 46582u })
					}
				}, allPlayer2);
			}
			break;
		}
		case MarkerDirection.Down:
		{
			foreach (IGameObject allPlayer3 in PlayerHelper.AllPlayers)
			{
				DrawManager.Draw(new DrawElement
				{
					drawAvfx = "gl_fan045_1bf",
					Position = position,
					drawOnObject = false,
					target = allPlayer3,
					radiusZ = 60f,
					radiusX = 60f,
					delayDrawTime = 2000f,
					distanceCheck = new DistanceCheck
					{
						CheckType = 8,
						Position = position,
						Count = 4,
						CheckObject = allPlayer3
					},
					hitCounter = new HitCounter
					{
						ActionID = (FireMarkerData2.Contains(a4) ? new HashSet<uint> { 46577u } : new HashSet<uint> { 46578u })
					}
				}, allPlayer3);
			}
			break;
		}
		}
	}
}
