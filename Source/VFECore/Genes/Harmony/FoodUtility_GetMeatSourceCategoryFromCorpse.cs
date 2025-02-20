using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGenesExpanded;

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetMeatSourceCategoryFromCorpse))]
public static class FoodUtility_GetMeatSourceCategoryFromCorpse
{
    private static bool Prefix(Thing thing, ref MeatSourceCategory __result)
    {
        if (ThingIngestingPatches.extraHumanMeatDefs != null &&
            thing is Corpse corpse &&
            ThingIngestingPatches.extraHumanMeatDefs.Contains(corpse.InnerPawn.RaceProps.meatDef))
        {
            __result = MeatSourceCategory.Humanlike;
            return false;
        }

        return true;
    }
}