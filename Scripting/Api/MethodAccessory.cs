using System;
using Replica.Engine.Util;
using Replica.Scripting.Host;

namespace Replica.Scripting.Api;

public sealed class MethodAccessory
{
	private readonly ScriptRuntime _runtime;

	private readonly string _guid;

	internal MethodAccessory(ScriptRuntime runtime, string guid)
	{
		_runtime = runtime;
		_guid = guid;
	}

	public void SendDraw(DrawModeEnum mode, DrawTypeEnum type, DrawPropertiesEdit props)
	{
		_runtime.Draw.Send(_guid, mode, type, props);
	}

	public void RemoveDraw(string nameRegex)
	{
		_runtime.Draw.Remove(_guid, nameRegex);
	}

	public void TTS(string message)
	{
		_runtime.Speak(message);
	}

	public void TextInfo(string message)
	{
		_runtime.ShowText(message);
	}

	public void SendChat(string message)
	{
		ChatSender.Send(message);
	}

	public string RegistFrameworkUpdateAction(Action action, bool onMainThread = true, bool deactivateExisting = false)
	{
		return _runtime.RegisterTick(_guid, action, deactivateExisting);
	}

	public void UnregistFrameworkUpdateAction(string id)
	{
		_runtime.UnregisterTick(id);
	}

	public void ClearFrameworkUpdateAction()
	{
		_runtime.ClearTicks(_guid);
	}

	public void RunOnMainThreadAsync(Action action)
	{
		Plugin.Framework.RunOnFrameworkThread(action);
	}
}
