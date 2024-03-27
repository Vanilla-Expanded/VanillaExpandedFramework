using HarmonyLib;
using Verse;
using VFECore;

namespace VanillaGenesExpanded
{
    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(Pawn_AgeTracker), "GrowthPointsPerDayAtLearningLevel")] //"LearnRateFactor"
    public static class GrowthPointPerDayAtLearningLevel_Patch
    {
        public static void Postfix(ref float __result, Pawn ___pawn)
        {
            if (PawnDataCache.GetPawnDataCache(___pawn) is CachedPawnData data)
            {
                __result *= data.growthPointMultiplier;
            }
        }
    }
}
