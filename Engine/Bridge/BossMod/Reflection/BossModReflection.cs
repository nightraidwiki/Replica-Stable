using System;
using System.Collections.Generic;
using System.Reflection;

namespace Replica.Engine.Bridge.BossMod.Reflection;

public static class BossModReflection
{
    private static readonly Dictionary<Type, Dictionary<string, FieldInfo?>> _fcache = new();
    private static readonly Dictionary<Type, FieldInfo[]> _allFieldCache = new();

    public static object? Get(object? obj, string name) => GetField(obj, name);

    public static object? GetField(object? obj, string name)
    {
        if (obj == null) return null;
        var t = obj.GetType();
        var fi = GetFieldInfo(t, name);
        if (fi != null)
        {
            try { return fi.GetValue(obj); } catch { }
        }
        var pi = t.GetProperty(name,
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (pi != null)
        {
            try { return pi.GetValue(obj); } catch { }
        }
        return null;
    }

    public static FieldInfo? GetFieldInfo(Type t, string name)
    {
        lock (_fcache)
        {
            if (!_fcache.TryGetValue(t, out var dict))
            {
                dict = new Dictionary<string, FieldInfo?>();
                _fcache[t] = dict;
            }
            if (dict.TryGetValue(name, out var cached)) return cached;

            FieldInfo? found = null;
            var cur = t;
            while (cur != null && cur != typeof(object))
            {
                found = cur.GetField(name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (found != null) break;
                cur = cur.BaseType;
            }
            dict[name] = found;
            return found;
        }
    }

    public static FieldInfo[] GetAllFieldsCached(Type t)
    {
        lock (_allFieldCache)
        {
            if (_allFieldCache.TryGetValue(t, out var cached)) return cached;
            var list = new List<FieldInfo>();
            var cur = t;
            while (cur != null && cur != typeof(object))
            {
                list.AddRange(cur.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                cur = cur.BaseType;
            }
            var arr = list.ToArray();
            _allFieldCache[t] = arr;
            return arr;
        }
    }

    public static bool HasField(Type t, string name)
    {
        foreach (var f in GetAllFieldsCached(t))
        {
            if (f.Name == name) return true;
        }
        return false;
    }

    public static float Ff(object? v, float def = 0f) => v is float f ? f : (v is double d ? (float)d : def);
    public static float FX(object? v) => Ff(GetField(v, "X"));
    public static float FZ(object? v) => Ff(GetField(v, "Z"));
    public static ulong UL(object? v) => v is ulong u ? u : (v is uint ui ? ui : (v is long l ? (ulong)l : (v is int i ? (ulong)i : 0UL)));
    public static uint EntityId(ulong id) => (uint)(id & 0xFFFFFFFF);
    public static float R1(float v) => MathF.Round(v, 1);
    public static float R2(float v) => MathF.Round(v, 2);
    public static float RotRad(object? r) => r == null ? 0f : (r is float f ? f : Ff(GetField(r, "Rad")));
}
