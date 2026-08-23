using System.Numerics;

namespace Replica.Engine.Helper;

public class WatchCheck
{
	public Vector3 WatchCheckPostion { get; set; }

	public Vector4 WatchWarnColor { get; set; } = Vector4.One;

	public Vector4 WatchSafeColor { get; set; } = Vector4.One;
}
