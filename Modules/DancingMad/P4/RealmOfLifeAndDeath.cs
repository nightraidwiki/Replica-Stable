using System.Collections.Generic;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.DancingMad.P4;

public class RealmOfLifeAndDeath : ISpecialAction
{
	public override string Name => "Realm of Life and Death";

	public override uint Phase => 4u;

	public override HashSet<uint> ActionID => new HashSet<uint>();
}
