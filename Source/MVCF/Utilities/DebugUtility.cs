using System.Collections.Generic;
using Verse;

namespace MVCF.Utilities;

public static class DebugUtility
{
    public static IEnumerable<T> LogInline<T>(this IEnumerable<T> source, string header = null)
    {
        if (!header.NullOrEmpty()) Log.Message($"{header}:");
        foreach (var item in source)
        {
            Log.Message($"    {item}");
            yield return item;
        }
    }
}
