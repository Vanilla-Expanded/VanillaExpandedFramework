using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Genes;

[HarmonyPatch]
public static class ThingIngestingPatches
{
    public static Pawn pawn;
    public static List<ThingDef> extraHumanMeatDefs;

    private static IEnumerable<MethodBase> TargetMethods()
    {
        // Can't specify both using two [HarmonyPatch] attributes
        // as it seems to bug out the patch and cause the __state
        // to be shared between all the patched methods, rather
        // than keeping it for the same method only. Using TargetMethods()
        // or having two separate patch classes fixes that issue.
        yield return AccessTools.DeclaredMethod(typeof(Thing), nameof(Thing.Ingested));
        yield return AccessTools.DeclaredMethod(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting));
    }

    private static void Prefix(Pawn ingester, out bool __state)
    {
        // Only do the setup if a pawn wasn't setup earlier.
        // This will handle situations where the methods
        // are called recursively, or one calls the other.
        if (pawn == null && ingester != null)
        {
            __state = true;
            pawn = ingester;
            StaticCollectionsClass.defs_treated_as_human_meat.TryGetValue(ingester, out extraHumanMeatDefs);
        }
        else
        {
            __state = false;
        }
    }

    // Use finalizer over a postfix to ensure the cleanup always happens, even if we get exceptions
    private static void Finalizer(bool __state)
    {
        if (__state)
        {
            pawn = null;
            extraHumanMeatDefs = null;
        }
    }
}