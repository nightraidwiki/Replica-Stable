using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Dalamud.Utility;
using Replica.Engine.Interop;
using Replica.Engine.Properties;
using Replica.Engine.Vfx;

namespace Replica.Engine.Memory;

internal static class OmenResourceCache
{
	private static readonly Mutex ResourceLock = new Mutex();

	private static string _circlePath = string.Empty;

	private static string _rectPath = string.Empty;

	private static string _rect2Path = string.Empty;

	private static readonly HashSet<float> _cachedFans = new HashSet<float>();

	private static readonly HashSet<float> _cachedDonuts = new HashSet<float>();

	private static readonly HashSet<string> _cachedPaths = new HashSet<string>();

	public unsafe static void AddResource(string path, byte[] data)
	{
		Plugin.DebugLog("ResourceAdd: " + path);
		ResourceLock.WaitOne();
		try
		{
			uint num = 8u;
			uint num2 = 1635149432u;
			Crc32 crc = new Crc32();
			byte[] bytes = Encoding.UTF8.GetBytes(path);
			crc.Append(bytes);
			uint num3 = BitConverter.ToUInt32(crc.GetCurrentHash());
			if (ClientOmenHooks.ResourceManagerAddress == 0)
			{
				Svc.Log.Warning("ResourceManagerAddress is 0x0, skipping resource add.");
				return;
			}
			nint num4 = Marshal.StringToHGlobalAnsi(path);
			try
			{
				nint num5 = ClientOmenHooks.getResource(ClientOmenHooks.ResourceManagerAddress, (nint)(&num), (nint)(&num2), (nint)(&num3), num4, IntPtr.Zero);
				if (num5 == IntPtr.Zero)
				{
					Svc.Log.Warning("Failed to get resource.");
					return;
				}
				Marshal.WriteByte(num5 + 168, 2);
				Marshal.WriteByte(num5 + 169, 7);
				void* ptr = ((IntPtr)IntPtr.Add(num5, 192)).ToPointer();
				nint num6 = Marshal.AllocHGlobal(data.Length);
				try
				{
					Marshal.Copy(data, 0, num6, data.Length);
					ClientOmenHooks.loadResource(Marshal.ReadIntPtr((nint)ptr), num6, (uint)data.Length, num5);
					ClientOmenHooks.finalizeResource(num5);
				}
				finally
				{
					Marshal.FreeHGlobal(num6);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(num4);
			}
		}
		catch (Exception ex)
		{
			Svc.Log.Error("Error in ResourceAdd: " + ex);
		}
		finally
		{
			ResourceLock.ReleaseMutex();
		}
	}

	public static void GetFan(float radian, out string path)
	{
		path = "vfx/omen/eff/yd/customFan" + $"{radian}".Replace(".", "_") + ".avfx";
		if (!_cachedFans.Contains(radian))
		{
			byte[] data = BuildFan(Resources.tmp_fan, radian);
			AddResource(path, data);
			_cachedFans.Add(radian);
		}
	}

	public static void GetDonut(float radius, out string path, float? angle = null)
	{
		path = "vfx/omen/eff/yd/customDonut" + $"{radius}".Replace(".", "_") + ".avfx";
		if (!_cachedDonuts.Contains(radius))
		{
			byte[] data = BuildDonut(Resources.tmp_donut, radius, angle);
			AddResource(path, data);
			_cachedDonuts.Add(radius);
		}
	}

	public static void GetCircle(out string path)
	{
		path = "vfx/omen/eff/yd/customCircle.avfx";
		if (_circlePath.IsNullOrEmpty())
		{
			byte[] tmp_circle = Resources.tmp_circle;
			AddResource(path, tmp_circle);
			_circlePath = path;
		}
	}

	public static void GetRect(out string path)
	{
		path = "vfx/omen/eff/yd/customRect.avfx";
		if (_rectPath.IsNullOrEmpty())
		{
			byte[] tmp_rect = Resources.tmp_rect;
			AddResource(path, tmp_rect);
			_rectPath = path;
		}
	}

	public static void GetRect2(out string path)
	{
		path = "vfx/omen/eff/yd/customRect2.avfx";
		if (_rect2Path.IsNullOrEmpty())
		{
			byte[] tmp_rect = Resources.tmp_rect2;
			AddResource(path, tmp_rect);
			_rect2Path = path;
		}
	}

	public static void RegisterRaw(byte[] data, string path)
	{
		if (!_cachedPaths.Contains(path))
		{
			AddResource(path, data);
			_cachedPaths.Add(path);
		}
	}

	public static byte[] BuildFan(byte[] template, float radian)
	{
		byte[] bytes = BitConverter.GetBytes((float)((1.0 - Math.Cos(radian / 2f)) / 2.0));
		byte[] bytes2 = BitConverter.GetBytes(0.45333326f - 10f / (float)Math.PI * radian);
		byte[] bytes3 = BitConverter.GetBytes(5.407703f + 14.222406f * radian);
		byte[] array = template.ToArray();
		Buffer.BlockCopy(bytes, 0, array, 6076, bytes.Length);
		Buffer.BlockCopy(bytes2, 0, array, 6800, bytes2.Length);
		Buffer.BlockCopy(bytes2, 0, array, 7284, bytes2.Length);
		Buffer.BlockCopy(bytes, 0, array, 9588, bytes.Length);
		Buffer.BlockCopy(bytes3, 0, array, 10312, bytes3.Length);
		Buffer.BlockCopy(bytes3, 0, array, 10796, bytes3.Length);
		Buffer.BlockCopy(bytes, 0, array, 13100, bytes.Length);
		return array;
	}

	public static byte[] BuildDonut(byte[] template, float radius, float? angle = null)
	{
		byte[] bytes = BitConverter.GetBytes(angle.HasValue ? ((float)((1.0 - Math.Cos(angle.Value / 2f)) / 2.0)) : 1f);
		float num = 0.5f * (1f - radius) / (1f + radius);
		byte[] bytes2 = BitConverter.GetBytes(num);
		byte[] bytes3 = BitConverter.GetBytes(1f / (0.5f + num));
		byte[] array = new byte[template.Length];
		template.CopyTo(array, 0);
		Buffer.BlockCopy(bytes3, 0, array, 388, bytes3.Length);
		Buffer.BlockCopy(bytes3, 0, array, 412, bytes3.Length);
		Buffer.BlockCopy(bytes, 0, array, 6044, bytes.Length);
		Buffer.BlockCopy(bytes2, 0, array, 6088, bytes2.Length);
		Buffer.BlockCopy(bytes, 0, array, 8772, bytes.Length);
		Buffer.BlockCopy(bytes2, 0, array, 8816, bytes2.Length);
		Buffer.BlockCopy(bytes, 0, array, 11500, bytes.Length);
		Buffer.BlockCopy(bytes2, 0, array, 11544, bytes2.Length);
		return array;
	}
}
