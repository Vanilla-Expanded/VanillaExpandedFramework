using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Genes;

[HarmonyPatch]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_Building_GeneExtractor_Patch
{
    private static bool Prepare(MethodBase method)
    {
        // Only check for DefModExtension on first pass (method is null) and only patch if any
        // def in game uses our DefModExtension and wants to disable gene extraction for its genes.
        // Slight downside, the patch won't be applied when hot-loading defs, only at startup.
        return method != null || DefDatabase<GeneDef>.AllDefs.Any(def => def.GetModExtension<GeneExtension>() is { disableGeneExtraction: true });
    }

    private static MethodBase TargetMethod()
    {
        return typeof(Building_GeneExtractor)
            .FirstInner(x => x.DeclaredField("genesToAdd") != null)
            .FirstMethod(x => 
                x.Name.Contains("<Finish>") &&
                x.ReturnType == typeof(float) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(Gene));
    }

    private static void Postfix(Gene g, ref float __result)
    {
        // Only do anything if current weight is already bigger than 0.
        // Lower than 0 will cause an errors, but it's not our issue if it somehow happened.
        if (__result > 0 && g.def.GetModExtension<GeneExtension>() is { disableGeneExtraction: true })
            __result = 0f;
    }
}