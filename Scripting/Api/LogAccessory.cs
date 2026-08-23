namespace Replica.Scripting.Api;

public sealed class LogAccessory
{
	private readonly string _guid;

	internal LogAccessory(string guid)
	{
		_guid = guid;
	}

	public void Debug(string message)
	{
		Plugin.Log.Debug("[script " + _guid + "] " + message);
	}

	public void Error(string message)
	{
		Plugin.Log.Error("[script " + _guid + "] " + message);
	}
}
