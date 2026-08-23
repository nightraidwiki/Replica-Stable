using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.ModuleSetup;

namespace Replica.Modules.M8S;

public class HowlingBlade : DrawModule
{
	public static readonly Vector2[] EndArenaPlatforms = new Vector2[5]
	{
		new Vector2(100f, 117.5f),
		new Vector2(83.357f, 105.408f),
		new Vector2(89.714f, 85.842f),
		new Vector2(110.286f, 85.842f),
		new Vector2(116.643f, 105.408f)
	};

	public override ModuleInfo ModuleInfo => new ModuleInfo
	{
		Category = Category.Savage,
		GroupType = GroupType.CFC,
		GroupID = 1026u
	};

	public override string Author => "Null";

	public override HashSet<uint> NoLogActionID => new HashSet<uint> { 42222u, 42225u, 42226u, 41871u, 42227u, 42228u };
}
