using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Interface.Textures.TextureWraps;

namespace Replica.Windows;

public static class Assets
{
	private static IDalamudTextureWrap? _logo;

	private static bool _started;

	public static IDalamudTextureWrap? Logo
	{
		get
		{
			if (!_started)
			{
				_started = true;
				LoadLogo();
			}
			return _logo;
		}
	}

	private static async void LoadLogo()
	{
		try
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string text = executingAssembly.GetManifestResourceNames().FirstOrDefault((string n) => n.EndsWith("replica_logo.png", StringComparison.OrdinalIgnoreCase));
			if (text != null)
			{
				using Stream s = executingAssembly.GetManifestResourceStream(text);
				byte[] array = new byte[s.Length];
				s.ReadExactly(array);
				_logo = await Plugin.TextureProvider.CreateFromImageAsync(array);
			}
		}
		catch
		{
		}
	}

	public static void Dispose()
	{
		_logo?.Dispose();
		_logo = null;
		_started = false;
	}
}
