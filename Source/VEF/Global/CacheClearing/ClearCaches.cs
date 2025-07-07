using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace VEF.CacheClearing;

public static class ClearCaches
{
    private static readonly Type[] TypesWithClearMethod = [typeof(ICollection<>), typeof(Queue<>), typeof(Stack<>)];

    public static HashSet<Type> clearCacheTypes = new();
    public static event Action<HashSet<object>> OnClearCache;

    internal static void ClearCache()
    {
        foreach (var type in clearCacheTypes)
            ClearFields(type, null, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        if (OnClearCache != null)
        {
            var instancesToClear = new HashSet<object>();
            OnClearCache(instancesToClear);

            foreach (var instance in instancesToClear.Where(x => x != null))
                ClearFields(instance.GetType(), instance, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    public static void ClearFields(Type type, object instance, BindingFlags flags)
    {
        try
        {
            foreach (var field in type.GetFields(flags))
            {
                if (field.HasAttribute<NoCacheClearingAttribute>())
                    return;

                var fieldType = field.FieldType;
                if (typeof(IDictionary).IsAssignableFrom(fieldType))
                    (field.GetValue(instance) as IDictionary)?.Clear();
                else if (typeof(IList).IsAssignableFrom(fieldType))
                    (field.GetValue(instance) as IList)?.Clear();
                else if (typeof(Queue).IsAssignableFrom(fieldType))
                    (field.GetValue(instance) as Queue)?.Clear();
                else if (typeof(Stack).IsAssignableFrom(fieldType))
                    (field.GetValue(instance) as Stack)?.Clear();
                else if (fieldType.IsGenericType && fieldType.GetGenericArguments().Length == 1)
                {
                    if (TypesWithClearMethod.Any(typeWithClear => typeWithClear.MakeGenericType(fieldType.GetGenericArguments()).IsAssignableFrom(fieldType)))
                    {
                        var value = field.GetValue(instance);
                        if (value != null)
                            AccessTools.Method(value.GetType(), "Clear", [])?.Invoke(value, []);
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (instance == null)
                Log.ErrorOnce($"Failed clearing cache for type {type.FullDescription()}, exception:\n{e}", type.GetHashCode());
            else
                Log.ErrorOnce($"Failed clearing cache for type {type.FullDescription()} with instance {instance}, exception:\n{e}", Gen.HashCombineInt(type.GetHashCode(), instance.GetHashCode()));
        }
    }
}