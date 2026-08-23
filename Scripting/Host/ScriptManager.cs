using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Replica.Logging;
using Replica.Scripting.Api;

namespace Replica.Scripting.Host;

public sealed class ScriptManager : IDisposable
{
	private readonly Configuration _config;

	private readonly ScriptRuntime _runtime = new ScriptRuntime();

	private readonly List<LoadedScript> _scripts = new List<LoadedScript>();

	private readonly List<string> _errors = new List<string>();

	private readonly object _gate = new object();

	private uint _territory;

	private bool _disposed;

	public IReadOnlyList<LoadedScript> Scripts => _scripts;

	public IReadOnlyList<string> LoadErrors => _errors;

	public string ScriptsPath => DefaultFolder();

	public ScriptManager(Configuration config)
	{
		_config = config;
		Plugin.ClientState.TerritoryChanged += OnTerritoryChanged;
		_territory = Plugin.ClientState.TerritoryType;
	}

	public void Update()
	{
		_runtime.Tick();
	}

	public void Reload()
	{
		lock (_gate)
		{
			foreach (LoadedScript script in _scripts)
			{
				_runtime.Draw.ClearAll(script.Guid);
			}
			_runtime.ClearAllTicks();
			_scripts.Clear();
			_errors.Clear();
			foreach (string item in SourceFiles())
			{
				LoadFile(item);
			}
		}
		InitScripts();
	}

	public void OnLogEvent(LogEvent raw)
	{
		if (!_config.ScriptsEnabled || _disposed || _scripts.Count == 0)
		{
			return;
		}
		Event obj = EventBridge.Translate(raw);
		if (obj == null)
		{
			return;
		}
		LoadedScript[] array;
		lock (_gate)
		{
			array = _scripts.ToArray();
		}
		DateTime now = DateTime.Now;
		LoadedScript[] array2 = array;
		foreach (LoadedScript loadedScript in array2)
		{
			if (!loadedScript.MatchesTerritory(_territory) || _config.DisabledScripts.Contains(loadedScript.Guid))
			{
				continue;
			}
			foreach (ScriptAction action in loadedScript.Actions)
			{
				if (action.Attribute.EventType == obj.Type && !_config.DisabledMethods.Contains(MethodKey(loadedScript, action)) && (action.Attribute.Suppress == 0 || !((now - action.LastFired).TotalMilliseconds < (double)action.Attribute.Suppress)) && obj.Match(action.Attribute.EventCondition))
				{
					action.LastFired = now;
					Invoke(loadedScript, action, obj.Clone());
				}
			}
		}
	}

	public void SetScriptEnabled(string guid, bool on)
	{
		if (on)
		{
			_config.DisabledScripts.Remove(guid);
		}
		else
		{
			_config.DisabledScripts.Add(guid);
			_runtime.Draw.ClearAll(guid);
			_runtime.ClearTicks(guid);
		}
		_config.Save();
	}

	public void SetMethodEnabled(LoadedScript script, ScriptAction action, bool on)
	{
		string item = MethodKey(script, action);
		if (on)
		{
			_config.DisabledMethods.Remove(item);
		}
		else
		{
			_config.DisabledMethods.Add(item);
		}
		_config.Save();
	}

	public static string MethodKey(LoadedScript script, ScriptAction action)
	{
		return script.Guid + "/" + action.MethodName;
	}

	public void Dispose()
	{
		_disposed = true;
		Plugin.ClientState.TerritoryChanged -= OnTerritoryChanged;
		lock (_gate)
		{
			foreach (LoadedScript script in _scripts)
			{
				_runtime.Draw.ClearAll(script.Guid);
			}
			_runtime.ClearAllTicks();
			_scripts.Clear();
		}
	}

	private void OnTerritoryChanged(uint territory)
	{
		_territory = territory;
		_runtime.ClearAllTicks();
		InitScripts();
	}

	private void InitScripts()
	{
		LoadedScript[] array;
		lock (_gate)
		{
			array = _scripts.ToArray();
		}
		LoadedScript[] array2 = array;
		foreach (LoadedScript loadedScript in array2)
		{
			if (!(loadedScript.InitMethod == null) && loadedScript.MatchesTerritory(_territory) && !_config.DisabledScripts.Contains(loadedScript.Guid))
			{
				try
				{
					loadedScript.InitMethod.Invoke(loadedScript.Instance, new object[1] { loadedScript.Accessory });
				}
				catch (Exception ex)
				{
					Plugin.Log.Error("[script " + loadedScript.Name + "] init failed: " + Root(ex));
				}
			}
		}
	}

	private void Invoke(LoadedScript script, ScriptAction action, Event ev)
	{
		try
		{
			action.Method.Invoke(script.Instance, new object[2] { ev, script.Accessory });
		}
		catch (Exception ex)
		{
			Plugin.Log.Error($"[script {script.Name}/{action.MethodName}] {Root(ex)}");
		}
	}

	private IEnumerable<string> SourceFiles()
	{
		List<string> list = new List<string> { DefaultFolder() };
		list.AddRange(_config.ScriptFolders.Where((string d) => !string.IsNullOrWhiteSpace(d)));
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in list)
		{
			if (!Directory.Exists(item))
			{
				continue;
			}
			foreach (string item2 in Directory.EnumerateFiles(item, "*.cs", SearchOption.AllDirectories))
			{
				if (seen.Add(item2))
				{
					yield return item2;
				}
			}
		}
	}

	private string DefaultFolder()
	{
		string text = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "Scripts");
		try
		{
			Directory.CreateDirectory(text);
		}
		catch
		{
		}
		return text;
	}

	private void LoadFile(string path)
	{
		string source;
		try
		{
			source = File.ReadAllText(path);
		}
		catch (Exception ex)
		{
			_errors.Add(Path.GetFileName(path) + ": " + ex.Message);
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
		CompileResult compileResult = ScriptCompiler.Compile($"YapScript_{fileNameWithoutExtension}_{Guid.NewGuid():N}", source, path);
		if (!compileResult.Ok)
		{
			foreach (string error in compileResult.Errors)
			{
				_errors.Add(Path.GetFileName(path) + ": " + error);
			}
			return;
		}
		Type[] array;
		try
		{
			array = compileResult.Assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex2)
		{
			array = ex2.Types.Where((Type t) => t != null).ToArray();
			if (array.Length == 0)
			{
				_errors.Add(Path.GetFileName(path) + ": " + Root(ex2));
				return;
			}
		}
		catch (Exception ex3)
		{
			_errors.Add(Path.GetFileName(path) + ": " + Root(ex3));
			return;
		}
		bool flag = false;
		Type[] array2 = array;
		foreach (Type type in array2)
		{
			try
			{
				ScriptTypeAttribute customAttribute = type.GetCustomAttribute<ScriptTypeAttribute>();
				if (customAttribute != null)
				{
					flag = true;
					Register(type, customAttribute, path);
				}
			}
			catch (Exception ex4)
			{
				_errors.Add(Path.GetFileName(path) + ": " + Root(ex4));
			}
		}
		if (!flag)
		{
			_errors.Add(Path.GetFileName(path) + ": no script class found");
		}
	}

	private void Register(Type type, ScriptTypeAttribute meta, string path)
	{
		object instance;
		try
		{
			instance = Activator.CreateInstance(type);
		}
		catch (Exception ex)
		{
			_errors.Add(Path.GetFileName(path) + ": " + Root(ex));
			return;
		}
		List<ScriptAction> list = new List<ScriptAction>();
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
		foreach (MethodInfo methodInfo in methods)
		{
			ScriptMethodAttribute customAttribute = methodInfo.GetCustomAttribute<ScriptMethodAttribute>();
			if (customAttribute != null)
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length != 2 || parameters[0].ParameterType != typeof(Event) || parameters[1].ParameterType != typeof(ScriptAccessory))
				{
					_errors.Add(meta.Name + "." + methodInfo.Name + ": expected (Event, ScriptAccessory)");
					continue;
				}
				list.Add(new ScriptAction
				{
					MethodName = methodInfo.Name,
					Attribute = customAttribute,
					Method = methodInfo
				});
			}
		}
		MethodInfo methodInfo2 = type.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
		if (methodInfo2 != null)
		{
			ParameterInfo[] parameters2 = methodInfo2.GetParameters();
			if (parameters2.Length != 1 || parameters2[0].ParameterType != typeof(ScriptAccessory))
			{
				methodInfo2 = null;
			}
		}
		_scripts.Add(new LoadedScript
		{
			Guid = meta.Guid,
			Name = meta.Name,
			Version = meta.Version,
			Author = meta.Author,
			Note = meta.Note,
			Territorys = meta.Territorys,
			SourcePath = path,
			Instance = instance,
			Accessory = new ScriptAccessory(_runtime, meta.Guid),
			Actions = list,
			InitMethod = methodInfo2
		});
	}

	private static string Root(Exception ex)
	{
		Exception ex2 = ex;
		while (ex2.InnerException != null)
		{
			ex2 = ex2.InnerException;
		}
		return ex2.Message;
	}
}
