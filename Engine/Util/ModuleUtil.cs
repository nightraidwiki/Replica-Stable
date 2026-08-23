using System;
using System.Collections.Generic;
using System.Linq;
using Replica.Engine.ModuleSetup;

namespace Replica.Engine.Util;

public static class ModuleUtil
{
	public static T? GetSpecialAction<T>() where T : ISpecialAction
	{
		return ModuleRegistry.AllMechanics.OfType<T>().FirstOrDefault();
	}

	public static void SortBy<TValue, TKey>(this List<TValue> list, Func<TValue, TKey> proj) where TKey : notnull, IComparable
	{
		list.Sort((TValue l, TValue r) => proj(l).CompareTo(proj(r)));
	}
}
