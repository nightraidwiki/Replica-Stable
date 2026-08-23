namespace Replica.Engine;

internal static class FightRuntime
{
	public static uint WeatherId { get; private set; }

	public static void SetWeather(uint id)
	{
		WeatherId = id;
	}
}
