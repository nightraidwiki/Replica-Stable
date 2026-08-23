using System.Collections.Generic;
using System.Numerics;
using Replica.Engine.Element;
using Replica.Engine.Helper;
using Replica.Engine.ModuleSetup;
using Replica.Engine.Struct;

namespace Replica.Modules.DancingMad.P3;

public class Uplift : ISpecialAction
{
	private readonly List<Vector3> _pos = new List<Vector3>();

	public override string Name => "Uplift";

	public override uint Phase => 3u;

	public override HashSet<uint> ActionID => new HashSet<uint> { 47875u };

	public override void OnAbilityCast(ActorAbilityInfo info)
	{
		if (info.ActionId != 47875 || info.Target == null)
		{
			return;
		}
		_pos.Add(info.Target.Position);
		if (_pos.Count != 2)
		{
			return;
		}
		foreach (Vector3 po in _pos)
		{
			SimpleElement.Circle(po, 6f, 3000f, 0f, new HitCounter
			{
				ActionID = new HashSet<uint> { 47878u }
			});
		}
		_pos.Clear();
	}

	public override void Reset()
	{
		_pos.Clear();
		base.Reset();
	}
}
