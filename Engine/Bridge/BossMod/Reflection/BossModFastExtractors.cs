using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Replica.Engine.Bridge.BossMod.Vfx;

namespace Replica.Engine.Bridge.BossMod.Reflection;

public static class BossModFastExtractors
{
    public delegate void ActiveAoesExtractor(object comp, int slot, object? actor, Action<object> emit);
    public delegate void ActiveTowersExtractor(object comp, int slot, object? actor, Action<object> emit);
    public delegate void ActiveEyesExtractor(object comp, int slot, object? actor, Action<object> emit);

    private static readonly Dictionary<Type, ActiveAoesExtractor?> _activeAoesExtractors = new();
    private static readonly Dictionary<Type, ActiveTowersExtractor?> _activeTowersExtractors = new();
    private static readonly Dictionary<Type, ActiveEyesExtractor?> _activeEyesExtractors = new();

    public static ActiveAoesExtractor? GetActiveAoesExtractor(Type compType)
    {
        lock (_activeAoesExtractors)
        {
            if (_activeAoesExtractors.TryGetValue(compType, out var cached))
                return cached;

            ActiveAoesExtractor? extractor = null;
            try
            {
                MethodInfo? targetMethod = null;
                int paramCount = 0;
                foreach (var m in compType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (m.Name == "ActiveAOEs" || m.Name == "AOEs")
                    {
                        var ps = m.GetParameters();
                        if (ps.Length == 2 && (ps[0].ParameterType == typeof(int) || ps[0].ParameterType == typeof(short)))
                        {
                            targetMethod = m;
                            paramCount = 2;
                            break;
                        }
                        else if (ps.Length == 1 && (ps[0].ParameterType == typeof(int) || ps[0].ParameterType == typeof(short)))
                        {
                            targetMethod = m;
                            paramCount = 1;
                        }
                        else if (ps.Length == 0 && targetMethod == null)
                        {
                            targetMethod = m;
                            paramCount = 0;
                        }
                    }
                }

                if (targetMethod != null)
                {
                    var retType = targetMethod.ReturnType;
                    if (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>))
                    {
                        var aoeItemType = retType.GetGenericArguments()[0];
                        var getItem = retType.GetMethod("get_Item", [typeof(int)])!;
                        var getLength = retType.GetProperty("Length")!.GetGetMethod()!;

                        var dm = new DynamicMethod(
                            "ActiveAOEsExtractor_" + compType.Name + "_" + Guid.NewGuid().ToString("N"),
                            typeof(void),
                            [typeof(object), typeof(int), typeof(object), typeof(Action<object>)],
                            typeof(BossModFastExtractors).Module,
                            true);

                        var il = dm.GetILGenerator();
                        var spanLocal = il.DeclareLocal(retType);
                        var lenLocal = il.DeclareLocal(typeof(int));
                        var iLocal = il.DeclareLocal(typeof(int));

                        var loopStart = il.DefineLabel();
                        var loopCheck = il.DefineLabel();

                        // Call comp.ActiveAOEs(slot, actor)
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Castclass, compType);
                        if (paramCount >= 1)
                        {
                            il.Emit(OpCodes.Ldarg_1);
                        }
                        if (paramCount >= 2)
                        {
                            var actorParamType = targetMethod.GetParameters()[1].ParameterType;
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Castclass, actorParamType);
                        }
                        if (targetMethod.IsVirtual)
                            il.Emit(OpCodes.Callvirt, targetMethod);
                        else
                            il.Emit(OpCodes.Call, targetMethod);

                        il.Emit(OpCodes.Stloc, spanLocal);

                        // len = spanLocal.Length
                        il.Emit(OpCodes.Ldloca, spanLocal);
                        il.Emit(OpCodes.Call, getLength);
                        il.Emit(OpCodes.Stloc, lenLocal);

                        // i = 0
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Stloc, iLocal);
                        il.Emit(OpCodes.Br, loopCheck);

                        // Loop start
                        il.MarkLabel(loopStart);

                        // Load emit action (Arg 3)
                        il.Emit(OpCodes.Ldarg_3);

                        // Load item ref: spanLocal[i]
                        il.Emit(OpCodes.Ldloca, spanLocal);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Call, getItem);

                        if (aoeItemType.IsValueType)
                        {
                            il.Emit(OpCodes.Ldobj, aoeItemType);
                            il.Emit(OpCodes.Box, aoeItemType);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldind_Ref);
                        }

                        il.Emit(OpCodes.Callvirt, typeof(Action<object>).GetMethod("Invoke", [typeof(object)])!);

                        // ++i
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc, iLocal);

                        // Loop check
                        il.MarkLabel(loopCheck);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Ldloc, lenLocal);
                        il.Emit(OpCodes.Blt, loopStart);

                        il.Emit(OpCodes.Ret);

                        extractor = (ActiveAoesExtractor)dm.CreateDelegate(typeof(ActiveAoesExtractor));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(retType))
                    {
                        int pCount = paramCount;
                        extractor = (comp, slot, actor, emit) =>
                        {
                            try
                            {
                                object?[] args = pCount switch
                                {
                                    2 => [slot, actor],
                                    1 => [slot],
                                    _ => []
                                };
                                var res = targetMethod.Invoke(comp, args) as IEnumerable;
                                if (res != null)
                                {
                                    foreach (var item in res)
                                    {
                                        if (item != null)
                                        {
                                            var origin = BossModReflection.GetField(item, "origin") ?? BossModReflection.GetField(item, "Origin") ?? BossModReflection.GetField(item, "Item1");
                                            var rot = BossModReflection.GetField(item, "rot") ?? BossModReflection.GetField(item, "Rotation") ?? BossModReflection.GetField(item, "Item2");
                                            var safe = BossModReflection.GetField(item, "safe") ?? BossModReflection.GetField(item, "Safe") ?? BossModReflection.GetField(item, "Item4");
                                            var shape = BossModReflection.GetField(item, "shape") ?? BossModReflection.GetField(item, "Shape") ?? BossModReflection.GetField(comp, "_shape") ?? BossModReflection.GetField(comp, "Shape");
                                            if (origin != null && shape != null)
                                            {
                                                bool isSafe = safe is bool s && s;
                                                emit(new SyntheticAOE(origin, rot, shape, isSafe));
                                            }
                                            else
                                            {
                                                emit(item);
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        };
                    }
                }
            }
            catch { }

            _activeAoesExtractors[compType] = extractor;
            return extractor;
        }
    }

    public static ActiveTowersExtractor? GetActiveTowersExtractor(Type compType)
    {
        lock (_activeTowersExtractors)
        {
            if (_activeTowersExtractors.TryGetValue(compType, out var cached))
                return cached;

            ActiveTowersExtractor? extractor = null;
            try
            {
                MethodInfo? targetMethod = null;
                int paramCount = 0;
                foreach (var m in compType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (m.Name == "ActiveTowers" || m.Name == "Towers")
                    {
                        var ps = m.GetParameters();
                        if (ps.Length == 2 && (ps[0].ParameterType == typeof(int) || ps[0].ParameterType == typeof(short)))
                        {
                            targetMethod = m;
                            paramCount = 2;
                            break;
                        }
                        else if (ps.Length == 1 && (ps[0].ParameterType == typeof(int) || ps[0].ParameterType == typeof(short)))
                        {
                            targetMethod = m;
                            paramCount = 1;
                        }
                        else if (ps.Length == 0 && targetMethod == null)
                        {
                            targetMethod = m;
                            paramCount = 0;
                        }
                    }
                }

                if (targetMethod != null)
                {
                    var retType = targetMethod.ReturnType;
                    if (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>))
                    {
                        var itemType = retType.GetGenericArguments()[0];
                        var getItem = retType.GetMethod("get_Item", [typeof(int)])!;
                        var getLength = retType.GetProperty("Length")!.GetGetMethod()!;

                        var dm = new DynamicMethod(
                            "ActiveTowersExtractor_" + compType.Name + "_" + Guid.NewGuid().ToString("N"),
                            typeof(void),
                            [typeof(object), typeof(int), typeof(object), typeof(Action<object>)],
                            typeof(BossModFastExtractors).Module,
                            true);

                        var il = dm.GetILGenerator();
                        var spanLocal = il.DeclareLocal(retType);
                        var lenLocal = il.DeclareLocal(typeof(int));
                        var iLocal = il.DeclareLocal(typeof(int));

                        var loopStart = il.DefineLabel();
                        var loopCheck = il.DefineLabel();

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Castclass, compType);
                        if (paramCount >= 1)
                        {
                            il.Emit(OpCodes.Ldarg_1);
                        }
                        if (paramCount >= 2)
                        {
                            var actorParamType = targetMethod.GetParameters()[1].ParameterType;
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Castclass, actorParamType);
                        }
                        if (targetMethod.IsVirtual)
                            il.Emit(OpCodes.Callvirt, targetMethod);
                        else
                            il.Emit(OpCodes.Call, targetMethod);

                        il.Emit(OpCodes.Stloc, spanLocal);

                        il.Emit(OpCodes.Ldloca, spanLocal);
                        il.Emit(OpCodes.Call, getLength);
                        il.Emit(OpCodes.Stloc, lenLocal);

                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Stloc, iLocal);
                        il.Emit(OpCodes.Br, loopCheck);

                        il.MarkLabel(loopStart);

                        il.Emit(OpCodes.Ldarg_3);
                        il.Emit(OpCodes.Ldloca, spanLocal);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Call, getItem);

                        if (itemType.IsValueType)
                        {
                            il.Emit(OpCodes.Ldobj, itemType);
                            il.Emit(OpCodes.Box, itemType);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldind_Ref);
                        }

                        il.Emit(OpCodes.Callvirt, typeof(Action<object>).GetMethod("Invoke", [typeof(object)])!);

                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc, iLocal);

                        il.MarkLabel(loopCheck);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Ldloc, lenLocal);
                        il.Emit(OpCodes.Blt, loopStart);

                        il.Emit(OpCodes.Ret);

                        extractor = (ActiveTowersExtractor)dm.CreateDelegate(typeof(ActiveTowersExtractor));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(retType))
                    {
                        int pCount = paramCount;
                        extractor = (comp, slot, actor, emit) =>
                        {
                            try
                            {
                                object?[] args = pCount switch
                                {
                                    2 => [slot, actor],
                                    1 => [slot],
                                    _ => []
                                };
                                var res = targetMethod.Invoke(comp, args) as IEnumerable;
                                if (res != null)
                                {
                                    foreach (var item in res)
                                        if (item != null) emit(item);
                                }
                            }
                            catch { }
                        };
                    }
                }
            }
            catch { }

            _activeTowersExtractors[compType] = extractor;
            return extractor;
        }
    }

    public static ActiveEyesExtractor? GetActiveEyesExtractor(Type compType)
    {
        lock (_activeEyesExtractors)
        {
            if (_activeEyesExtractors.TryGetValue(compType, out var cached))
                return cached;

            ActiveEyesExtractor? extractor = null;
            try
            {
                MethodInfo? targetMethod = null;
                int paramCount = 0;
                foreach (var m in compType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (m.Name == "ActiveEyes" || m.Name == "Eyes")
                    {
                        var ps = m.GetParameters();
                        if (ps.Length == 2 && (ps[0].ParameterType == typeof(int) || ps[0].ParameterType == typeof(short)))
                        {
                            targetMethod = m;
                            paramCount = 2;
                            break;
                        }
                        else if (ps.Length == 1 && (ps[0].ParameterType == typeof(int) || ps[0].ParameterType == typeof(short)))
                        {
                            targetMethod = m;
                            paramCount = 1;
                        }
                        else if (ps.Length == 0 && targetMethod == null)
                        {
                            targetMethod = m;
                            paramCount = 0;
                        }
                    }
                }

                if (targetMethod != null)
                {
                    var retType = targetMethod.ReturnType;
                    if (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(ReadOnlySpan<>))
                    {
                        var itemType = retType.GetGenericArguments()[0];
                        var getItem = retType.GetMethod("get_Item", [typeof(int)])!;
                        var getLength = retType.GetProperty("Length")!.GetGetMethod()!;

                        var dm = new DynamicMethod(
                            "ActiveEyesExtractor_" + compType.Name + "_" + Guid.NewGuid().ToString("N"),
                            typeof(void),
                            [typeof(object), typeof(int), typeof(object), typeof(Action<object>)],
                            typeof(BossModFastExtractors).Module,
                            true);

                        var il = dm.GetILGenerator();
                        var spanLocal = il.DeclareLocal(retType);
                        var lenLocal = il.DeclareLocal(typeof(int));
                        var iLocal = il.DeclareLocal(typeof(int));

                        var loopStart = il.DefineLabel();
                        var loopCheck = il.DefineLabel();

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Castclass, compType);
                        if (paramCount >= 1)
                        {
                            il.Emit(OpCodes.Ldarg_1);
                        }
                        if (paramCount >= 2)
                        {
                            var actorParamType = targetMethod.GetParameters()[1].ParameterType;
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Castclass, actorParamType);
                        }
                        if (targetMethod.IsVirtual)
                            il.Emit(OpCodes.Callvirt, targetMethod);
                        else
                            il.Emit(OpCodes.Call, targetMethod);

                        il.Emit(OpCodes.Stloc, spanLocal);

                        il.Emit(OpCodes.Ldloca, spanLocal);
                        il.Emit(OpCodes.Call, getLength);
                        il.Emit(OpCodes.Stloc, lenLocal);

                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Stloc, iLocal);
                        il.Emit(OpCodes.Br, loopCheck);

                        il.MarkLabel(loopStart);

                        il.Emit(OpCodes.Ldarg_3);
                        il.Emit(OpCodes.Ldloca, spanLocal);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Call, getItem);

                        if (itemType.IsValueType)
                        {
                            il.Emit(OpCodes.Ldobj, itemType);
                            il.Emit(OpCodes.Box, itemType);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldind_Ref);
                        }

                        il.Emit(OpCodes.Callvirt, typeof(Action<object>).GetMethod("Invoke", [typeof(object)])!);

                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Add);
                        il.Emit(OpCodes.Stloc, iLocal);

                        il.MarkLabel(loopCheck);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Ldloc, lenLocal);
                        il.Emit(OpCodes.Blt, loopStart);

                        il.Emit(OpCodes.Ret);

                        extractor = (ActiveEyesExtractor)dm.CreateDelegate(typeof(ActiveEyesExtractor));
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(retType))
                    {
                        int pCount = paramCount;
                        extractor = (comp, slot, actor, emit) =>
                        {
                            try
                            {
                                object?[] args = pCount switch
                                {
                                    2 => [slot, actor],
                                    1 => [slot],
                                    _ => []
                                };
                                var res = targetMethod.Invoke(comp, args) as IEnumerable;
                                if (res != null)
                                {
                                    foreach (var item in res)
                                        if (item != null) emit(item);
                                }
                            }
                            catch { }
                        };
                    }
                }
            }
            catch { }

            _activeEyesExtractors[compType] = extractor;
            return extractor;
        }
    }
}
