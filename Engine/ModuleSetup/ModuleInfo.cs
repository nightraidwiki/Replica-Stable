namespace Replica.Engine.ModuleSetup;

public class ModuleInfo
{
	public required Category Category { get; set; } = Category.Count;

	public required GroupType GroupType { get; set; }

	public required uint GroupID { get; set; }

	public uint NameID { get; set; }
}
