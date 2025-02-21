using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGenesExpanded;

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetMeatSourceCategory))]
public static class FoodUtility_GetMeatSourceCategory
{
    private static bool Prefix(ThingDef source, ref MeatSourceCategory __result)
    {
        // Source can't be null here.
        if (ThingIngestingPatches.extraHumanMeatDefs != null &&
            ThingIngestingPatches.extraHumanMeatDefs.Contains(source))
        {
            __result = MeatSourceCategory.Humanlike;
            return false;
        }

        return true;
    }
}