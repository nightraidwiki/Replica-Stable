using System;
using System.Collections.Generic;
using Replica.QuickDraws;

namespace Replica.Strats;

public sealed class StratSlide
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public string Name { get; set; } = "New step";

	public TriggerMatch On { get; set; } = TriggerMatch.Cast;

	public string Pattern { get; set; } = "";

	public uint DataId { get; set; }

	public bool MatchById { get; set; }

	public float DelaySeconds { get; set; }

	public List<StratBranch> Branches { get; set; } = new List<StratBranch>();

	public StratSlide Clone()
	{
		StratSlide obj = (StratSlide)MemberwiseClone();
		obj.Branches = Branches.ConvertAll((StratBranch b) => b.Clone());
		return obj;
	}
}
