using System;
using System.Collections.Generic;

namespace Replica.QuickDraws;

public sealed class FollowUpStep
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public FollowUpOn On { get; set; }

	public float Seconds { get; set; } = 9f;

	public string Pattern { get; set; } = "";

	public uint DataId { get; set; }

	public bool OnlyOnSelf { get; set; } = true;

	public bool RequireAll { get; set; } = true;

	public List<FollowCond> Conditions { get; set; } = new List<FollowCond>();

	public bool DrawEnabled { get; set; } = true;

	public DrawSpec Draw { get; set; } = new DrawSpec();

	public List<DrawSpec> ExtraShapes { get; set; } = new List<DrawSpec>();

	public bool IsConditional => On != FollowUpOn.Timer;

	public void EnsureConditions()
	{
		if (On != FollowUpOn.Timer && Conditions.Count <= 0)
		{
			List<FollowCond> conditions = Conditions;
			FollowCond followCond = new FollowCond();
			followCond.Pattern = Pattern;
			followCond.DataId = DataId;
			FollowCond followCond2 = followCond;
			bool flag = DataId != 0;
			if (flag)
			{
				FollowUpOn followUpOn = On;
				bool flag2 = followUpOn - 4 <= FollowUpOn.Cast;
				flag = flag2;
			}
			followCond2.MatchById = flag;
			followCond.OnlyOnSelf = OnlyOnSelf;
			conditions.Add(followCond);
		}
	}

	public FollowUpStep Clone()
	{
		FollowUpStep obj = (FollowUpStep)MemberwiseClone();
		obj.Conditions = Conditions.ConvertAll((FollowCond x) => x.Clone());
		obj.Draw = Draw.Clone();
		obj.ExtraShapes = ExtraShapes.ConvertAll((DrawSpec x) => x.Clone());
		return obj;
	}
}
