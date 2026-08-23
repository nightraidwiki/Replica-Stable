using System;
using System.Collections.Generic;

namespace Replica.QuickDraws;

public sealed class QuickDrawDef
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public bool Enabled { get; set; } = true;

	public string Name { get; set; } = "New quick draw";

	public string Group { get; set; } = "";

	public TriggerMatch On { get; set; } = TriggerMatch.Cast;

	public string Pattern { get; set; } = "";

	public bool UseRegex { get; set; }

	public SourceFilter Source { get; set; }

	public bool OnlyOnSelf { get; set; }

	public bool MatchById { get; set; }

	public uint DataId { get; set; }

	public uint IconId { get; set; }

	public RoleFilter SourceRole { get; set; }

	public RoleFilter TargetRole { get; set; }

	public string SourceName { get; set; } = "";

	public string TargetName { get; set; } = "";

	public List<NumCond> NumConds { get; set; } = new List<NumCond>();

	public List<VarCond> VarConds { get; set; } = new List<VarCond>();

	public List<VarAction> SetVars { get; set; } = new List<VarAction>();

	public List<StatusGate> StatusGates { get; set; } = new List<StatusGate>();

	public float Cooldown { get; set; }

	public bool NoReentry { get; set; }

	public Concurrency Concurrency { get; set; } = Concurrency.Replace;

	public ClearRule ClearOn { get; set; } = new ClearRule();

	public bool AnyZone { get; set; } = true;

	public List<uint> Zones { get; set; } = new List<uint>();

	public float DelaySeconds { get; set; }

	public bool DrawEnabled { get; set; } = true;

	public DrawSpec Draw { get; set; } = new DrawSpec();

	public List<DrawSpec> ExtraShapes { get; set; } = new List<DrawSpec>();

	public List<FollowUpStep> FollowUps { get; set; } = new List<FollowUpStep>();

	public QuickDrawDef Clone()
	{
		QuickDrawDef obj = (QuickDrawDef)MemberwiseClone();
		obj.FollowUps = FollowUps.ConvertAll((FollowUpStep s) => s.Clone());
		obj.Zones = new List<uint>(Zones);
		obj.NumConds = NumConds.ConvertAll((NumCond x) => x.Clone());
		obj.VarConds = VarConds.ConvertAll((VarCond x) => x.Clone());
		obj.SetVars = SetVars.ConvertAll((VarAction x) => x.Clone());
		obj.StatusGates = StatusGates.ConvertAll((StatusGate x) => x.Clone());
		obj.ClearOn = ClearOn.Clone();
		obj.Draw = Draw.Clone();
		obj.ExtraShapes = ExtraShapes.ConvertAll((DrawSpec x) => x.Clone());
		return obj;
	}
}
