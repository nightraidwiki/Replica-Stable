using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Replica.Engine.ModuleSetup;

namespace Replica.Engine;

public static class ModuleRegistry
{
	public sealed record LoadedFight(BaseModule Host, List<ISpecialAction> Mechanics);

	internal static IReadOnlyList<ISpecialAction> AllMechanics { get; private set; } = Array.Empty<ISpecialAction>();

	public static IReadOnlyList<LoadedFight> LoadAll()
	{
		Assembly assembly = typeof(ModuleRegistry).Assembly;
		List<Type> list = (from t in assembly.GetTypes()
			where (object)t != null && !t.IsAbstract && t.IsClass && typeof(DrawModule).IsAssignableFrom(t) && t != typeof(DrawModule) && t != typeof(BaseModule)
			select t).OrderBy<Type, string>((Type t) => t.Namespace, StringComparer.Ordinal).ThenBy<Type, string>((Type t) => t.Name, StringComparer.Ordinal).ToList();
		List<LoadedFight> list2 = new List<LoadedFight>();
		foreach (Type item in list)
		{
			try
			{
				if (!(Activator.CreateInstance(item) is DrawModule drawModule))
				{
					continue;
				}
				string ns = item.Namespace;
				if (string.IsNullOrEmpty(ns))
				{
					continue;
				}
				List<ISpecialAction> list3 = (from ISpecialAction m in from m in (from t in assembly.GetTypes()
							where (object)t != null && !t.IsAbstract && t.IsClass && typeof(ISpecialAction).IsAssignableFrom(t) && t.Namespace != null && (t.Namespace == ns || t.Namespace.StartsWith(ns + ".", StringComparison.Ordinal))
							select t).OrderBy<Type, string>((Type t) => t.Namespace, StringComparer.Ordinal).ThenBy<Type, string>((Type t) => t.Name, StringComparer.Ordinal).Select(delegate(Type t)
						{
							try
							{
								return Activator.CreateInstance(t) as ISpecialAction;
							}
							catch
							{
								return (ISpecialAction)null;
							}
						})
						where m != null
						select m
					where m.Registered
					select m).ToList();
				foreach (ISpecialAction item2 in list3)
				{
					try
					{
						item2.Setup();
					}
					catch
					{
					}
					drawModule.SpecialActions.Add(item2);
				}
				list2.Add(new LoadedFight(drawModule, list3));
			}
			catch
			{
			}
		}
		AllMechanics = list2.SelectMany((LoadedFight f) => f.Mechanics).ToList();
		return list2;
	}
}
