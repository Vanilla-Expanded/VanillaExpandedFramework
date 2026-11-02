using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Genes;

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.GetMeatSourceCategoryFromCorpse))]
public static class VanillaExpandedFramework_FoodUtility_GetMeatSourceCategoryFromCorpse
{
    private static bool Prefix(Thing thing, ref MeatSourceCategory __result)
    {
        if (thing is Corpse corpse)
        {
            if (ThingIngestingPatches.extraHumanMeatDefs != null &&
                ThingIngestingPatches.extraHumanMeatDefs.Contains(corpse.InnerPawn.RaceProps.meatDef))
            {
                __result = MeatSourceCategory.Humanlike;
                return false;
            }

            if (ThingIngestingPatches.extraHumanMeatTypes != null)
            {
                var meat = corpse.InnerPawn.RaceProps.meatDef;
                if (meat.ingestible != null)
                {
                    for (var i = 0; i < ThingIngestingPatches.extraHumanMeatTypes.Count; i++)
                    {
                        if (meat.ingestible.foodType.HasFlag(ThingIngestingPatches.extraHumanMeatTypes[i]))
                        {
                            __result = MeatSourceCategory.Humanlike;
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }
}