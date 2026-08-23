namespace Replica.Engine.Managers;

public static class HookManager
{
	public delegate long ProcessMapEffectFuncDelegate(long a1, uint a2, ushort a3, ushort a4);

	public static ProcessMapEffectFuncDelegate ProcessMapEffectFunc { get; set; } = (long _, uint _, ushort _, ushort _) => 0L;
}
