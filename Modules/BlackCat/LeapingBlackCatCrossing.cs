using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.Interop;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;
using Replica.Engine.Util;

namespace Replica.Modules.BlackCat;

public class LeapingBlackCatCrossing : ISpecialAction
{
	private enum Pattern
	{
		None,
		Cardinal,
		Diagonal
	}

	private Pattern currentPattern;

	public override string Name => "Leaping Black Cat Crossing";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37673u, 38928u };

	public override void OnActionCast(ActorCastInfo info)
	{
		currentPattern = Pattern.None;
		new TimeHelper(500L, delegate
		{
			IGameObject gameObject = Svc.Objects.Where((IGameObject o) => o.BaseId == 17054).FirstOrDefault();
			if (gameObject != null)
			{
				if (currentPattern == Pattern.Cardinal)
				{
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 0.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 90.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 180.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 270.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 45.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 135.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 225.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 315.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
				}
				else if (currentPattern == Pattern.Diagonal)
				{
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 45.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 135.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 225.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 315.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37676u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 0.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 90.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 180.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 45, 270.Degrees(), 3000f, 7500f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37677u }
					});
				}
			}
		});
	}

	public override void OnAddStatus(ActorStatusChangeInfo info)
	{
		if (currentPattern == Pattern.None)
		{
			switch (info.StatusID)
			{
			case 2056u:
				currentPattern = Pattern.Cardinal;
				break;
			case 2193u:
				currentPattern = Pattern.Diagonal;
				break;
			}
		}
	}
}
