using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Genes;

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetMeatSourceCategory))]
public static class VanillaExpandedFramework_FoodUtility_GetMeatSourceCategory
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

        if (ThingIngestingPatches.extraHumanMeatTypes != null && source.ingestible != null)
        {
            for (var i = 0; i < ThingIngestingPatches.extraHumanMeatTypes.Count; i++)
            {
                if (source.ingestible.foodType.HasFlag(ThingIngestingPatches.extraHumanMeatTypes[i]))
                {
                    __result = MeatSourceCategory.Humanlike;
                    return false;
                }
            }
        }

        return true;
    }
}