using System;
using Replica.Engine.Managers;

namespace Replica.Engine.Helper;

public class TimeHelper
{
	private readonly long _startTick = Environment.TickCount64;

	private readonly long _delayMs;

	private readonly Action _action;

	public TimeHelper(long delay, Action action)
	{
		_action = action;
		_delayMs = delay;
		Data.DelayTasks.Add(this);
		FrameworkUpdateManager.TimeHelpers.Add(this);
	}

	public void Update()
	{
		if (Environment.TickCount64 - _startTick >= _delayMs)
		{
			FrameworkUpdateManager.TimeHelpers.Remove(this);
			Data.DelayTasks.Remove(this);
			_action();
		}
	}

	public void Stop()
	{
		FrameworkUpdateManager.TimeHelpers.Remove(this);
		Data.DelayTasks.Remove(this);
	}
}
