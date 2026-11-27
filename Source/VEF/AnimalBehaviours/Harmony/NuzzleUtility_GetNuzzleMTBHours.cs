using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.AnimalBehaviours
{
    [HarmonyPatch(typeof(NuzzleUtility))]
    [HarmonyPatch("GetNuzzleMTBHours")]
    public static class VanillaExpandedFramework_NuzzleUtility_GetNuzzleMTBHours_Patch
    {
        [HarmonyPostfix]
        public static void ApplyNuzzleFactorStat(Pawn pawn, ref float __result)
        {
            __result *= pawn.GetStatValue(InternalDefOf.VEF_NuzzleIntervalFactor, cacheStaleAfterTicks: 60);
        }
    }
}
