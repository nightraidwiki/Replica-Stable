using Replica.Scripting.Host;

namespace Replica.Scripting.Api;

public sealed class ScriptAccessory
{
	public DataAccessory Data { get; }

	public MethodAccessory Method { get; }

	public LogAccessory Log { get; }

	internal ScriptAccessory(ScriptRuntime runtime, string scriptGuid)
	{
		Data = new DataAccessory();
		Method = new MethodAccessory(runtime, scriptGuid);
		Log = new LogAccessory(scriptGuid);
	}
}
