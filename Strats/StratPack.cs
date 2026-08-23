using System;
using System.Collections.Generic;

namespace Replica.Strats;

public sealed class StratPack
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public bool Enabled { get; set; } = true;

	public string Name { get; set; } = "New strat";

	public string FightKey { get; set; } = "";

	public uint Territory { get; set; }

	public string Author { get; set; } = "";

	public bool BuiltIn { get; set; }

	public byte ArenaShape { get; set; }

	public float ArenaRadius { get; set; } = 20f;

	public float ArenaCenterX { get; set; } = 100f;

	public float ArenaCenterZ { get; set; } = 100f;

	public List<StratSlide> Slides { get; set; } = new List<StratSlide>();

	public StratPack Clone()
	{
		StratPack obj = (StratPack)MemberwiseClone();
		obj.Slides = Slides.ConvertAll((StratSlide s) => s.Clone());
		return obj;
	}
}
