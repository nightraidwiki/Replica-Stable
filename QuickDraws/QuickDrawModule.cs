using System;
using System.Collections.Generic;

namespace Replica.QuickDraws;

public sealed class QuickDrawModule
{
	public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public bool Enabled { get; set; } = true;

	public string Name { get; set; } = "New pack";

	public string Category { get; set; } = "General";

	public string Author { get; set; } = "";

	public bool BuiltIn { get; set; }

	public List<QuickDrawDef> Draws { get; set; } = new List<QuickDrawDef>();
}
