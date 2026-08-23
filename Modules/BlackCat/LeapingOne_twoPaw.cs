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

public class LeapingOne_twoPaw : ISpecialAction
{
	public override string Name => "Leaping One-two Paw";

	public override uint Phase => 1u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 37663u, 37664u, 37665u, 37666u };

	public override void OnActionCast(ActorCastInfo info)
	{
		new TimeHelper(500L, delegate
		{
			IGameObject gameObject = Svc.Objects.Where((IGameObject o) => o.BaseId == 17054).FirstOrDefault();
			if (gameObject != null)
			{
				switch (info.ActionId)
				{
				case 37663:
				case 37665:
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 180, 90.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37668u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 180, -90.Degrees(), 3000f, 7300f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37669u }
					});
					break;
				case 37664:
				case 37666:
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 180, -90.Degrees(), 3000f, 0f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37672u }
					});
					SimpleElement.Fan(new Vector3(gameObject.Position.X, 0f, gameObject.Position.Z), 60f, 180, 90.Degrees(), 3000f, 7300f, new HitCounter
					{
						ActionID = new HashSet<uint> { 37671u }
					});
					break;
				}
			}
		});
	}
}
