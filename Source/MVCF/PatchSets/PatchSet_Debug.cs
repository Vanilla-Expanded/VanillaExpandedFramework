using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace MVCF.PatchSets;

public class PatchSet_Debug : PatchSet
{
    public override IEnumerable<Patch> GetPatches()
    {
        var prefix = AccessTools.Method(GetType(), nameof(Debug_Prefix));
        var postfix = AccessTools.Method(GetType(), nameof(Debug_Postfix));
        var postfixVoid = AccessTools.Method(GetType(), nameof(Debug_Postfix_Void));
        foreach (var methodBase in GetToPatch())
            yield return new Patch(methodBase, prefix, methodBase is not MethodInfo { ReturnType: var t } || t == typeof(void) ? postfixVoid : postfix);
    }

    public virtual IEnumerable<MethodBase> GetToPatch()
    {
        foreach (var methodInfo in AccessTools.GetDeclaredMethods(typeof(Targeter)).Where(m => m.Name == "BeginTargeting")) yield return methodInfo;
    }

    public static void Debug_Prefix(object[] __args, MethodBase __originalMethod)
    {
        MVCF.Log(FullName(__originalMethod) + $"({__args.Join(arg => arg?.ToString() ?? "null")})");
    }

    public static void Debug_Postfix(object[] __args, MethodBase __originalMethod, object __result, bool __runOriginal)
    {
        MVCF.Log(FullName(__originalMethod) + $"({__args.Join(arg => arg?.ToString() ?? "null")})" +
                 $" -> {__result} ({(__runOriginal ? "original ran" : "original was skipped")})");
    }

    public static void Debug_Postfix_Void(object[] __args, MethodBase __originalMethod, bool __runOriginal)
    {
        MVCF.Log(FullName(__originalMethod) + $"({__args.Join(arg => arg?.ToString() ?? "null")})" +
                 $" -> void ({(__runOriginal ? "original ran" : "original was skipped")})");
    }

    private static string FullName(MethodBase method) => $"{method.DeclaringType?.Namespace}.{method.DeclaringType?.Name}.{method.Name}";
}
