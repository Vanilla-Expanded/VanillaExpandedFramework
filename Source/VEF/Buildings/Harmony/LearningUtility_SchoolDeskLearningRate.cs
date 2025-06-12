using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Buildings;

[HarmonyPatch(typeof(LearningUtility), nameof(LearningUtility.SchoolDeskLearningRate))]
public static class VanillaExpandedFramework_LearningUtility_SchoolDeskLearningRate
{
    public static bool Prefix(Thing schoolDesk, ref float __result)
    {
        // Use our custom StatDef for the learning rate.
        __result = schoolDesk.GetStatValue(VEFDefOf.VEF_BuildingLearningRateOffset);

        // Skip the vanilla code, as it only checks for up to 3 blackboards and gives 0.2 bonus for each.
        // We gave them our custom StatDef, so other mods can freely customize max links and link bonus.
        return false;
    }
}