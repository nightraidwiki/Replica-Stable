using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Replica.Engine.ModuleSetup;

public static class ModuleConfig
{
	private static readonly Dictionary<string, object> Cache = new Dictionary<string, object>();

	private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true,
		Converters = { (JsonConverter)new JsonStringEnumConverter() }
	};

	public static bool IsEnabled(string? enableKey)
	{
		if (string.IsNullOrEmpty(enableKey))
		{
			return true;
		}
		Configuration config = Plugin.Config;
		if (config.ModuleEnabled.TryGetValue(enableKey, out var value))
		{
			return value;
		}
		return !config.DisabledMechanics.Contains(enableKey);
	}

	public static void SetEnabled(string? enableKey, bool enabled)
	{
		if (!string.IsNullOrEmpty(enableKey))
		{
			Configuration config = Plugin.Config;
			config.ModuleEnabled[enableKey] = enabled;
			if (enabled)
			{
				config.DisabledMechanics.Remove(enableKey);
			}
			else
			{
				config.DisabledMechanics.Add(enableKey);
			}
			config.Save();
		}
	}

	public static T Get<T>() where T : class, new()
	{
		string fullName = typeof(T).FullName;
		if (Cache.TryGetValue(fullName, out object value) && value is T result)
		{
			return result;
		}
		T val = new T();
		if (Plugin.Config.ModuleConfigs.TryGetValue(fullName, out string value2) && !string.IsNullOrEmpty(value2))
		{
			try
			{
				val = JsonSerializer.Deserialize<T>(value2, JsonOpts) ?? new T();
			}
			catch
			{
				val = new T();
			}
		}
		Cache[fullName] = val;
		return val;
	}

	public static void Set<T>(T value) where T : class, new()
	{
		string fullName = typeof(T).FullName;
		Cache[fullName] = value;
		Plugin.Config.ModuleConfigs[fullName] = JsonSerializer.Serialize(value, JsonOpts);
		Plugin.Config.Save();
	}

	public static void Save<T>() where T : class, new()
	{
		string fullName = typeof(T).FullName;
		T val;
		if (Cache.TryGetValue(fullName, out object value))
		{
			val = value as T;
			if (val != null)
			{
				goto IL_0039;
			}
		}
		val = Get<T>();
		goto IL_0039;
		IL_0039:
		Plugin.Config.ModuleConfigs[fullName] = JsonSerializer.Serialize(val, JsonOpts);
		Plugin.Config.Save();
	}

	public static void MigrateLegacyActive(string enableKey, bool legacyActive)
	{
		if (!string.IsNullOrEmpty(enableKey) && !Plugin.Config.ModuleEnabled.ContainsKey(enableKey) && legacyActive)
		{
			SetEnabled(enableKey, enabled: true);
		}
	}
}
