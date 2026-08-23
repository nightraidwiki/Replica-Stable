using System.Collections.Generic;

namespace Replica.Engine.ModuleSetup;

public abstract class BaseModule
{
	public bool Enabled { get; private set; }

	public abstract ModuleInfo ModuleInfo { get; }

	public virtual string Name { get; protected set; } = "Unknown";

	public virtual string Author => "Null";

	public virtual string Description => string.Empty;

	public virtual HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)>();

	public virtual bool DisableWeatherReset => false;

	public virtual HashSet<uint> NoLogActionID => new HashSet<uint>();

	public List<ISpecialAction> SpecialActions { get; init; } = new List<ISpecialAction>();

	public virtual bool UseAutoDraw => false;

	public virtual Dictionary<uint, HashSet<uint>> BlockOmenMap => new Dictionary<uint, HashSet<uint>>();

	public virtual Dictionary<uint, HashSet<string>> BlockOmenPathMap => new Dictionary<uint, HashSet<string>>();

	public virtual bool HasConfig => false;

	public virtual void DrawConfig()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void Setup()
	{
		foreach (ISpecialAction specialAction in SpecialActions)
		{
			specialAction.Setup();
		}
	}

	public virtual void Enable()
	{
		Enabled = true;
	}

	public virtual void Disable()
	{
		Enabled = false;
	}
}
