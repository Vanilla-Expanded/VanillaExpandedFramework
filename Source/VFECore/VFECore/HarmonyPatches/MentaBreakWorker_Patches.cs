using HarmonyLib;
using System.Linq;
using VanillaGenesExpanded;
using Verse;
using Verse.AI;

namespace VFECore
{
    [HarmonyPatch]
    public static class MentaBreakWorker_Patches
    {
        [HarmonyPatch(typeof(MentalBreakWorker), nameof(MentalBreakWorker.CommonalityFor))]
        [HarmonyPostfix]
        public static void CommonalityFor_PostFix(ref float __result, MentalBreakWorker __instance, Pawn pawn)
        {
            if (pawn?.genes != null && __instance.def.mentalState == VFEDefOf.Binging_Food)
            {
                var foodBingeList = pawn.genes.GetActiveGeneExtensions()
                    .Where(x => x.foodBingeMentalBreakSelectionChanceFactor != 1)
                    .Select(x => x.foodBingeMentalBreakSelectionChanceFactor);
                if (foodBingeList.Any())
                {
                    foreach (var factor in foodBingeList)
                    {
                        __result *= factor;
                    }
                }
            }
        }

        /// <summary>
        /// Disallow Mental Breaks not of type Food Binge if the sum is greater than 20
        /// </summary>
        [HarmonyPatch(typeof(MentalBreakWorker), nameof(MentalBreakWorker.BreakCanOccur))]
        [HarmonyPostfix]
        public static void BreakCanOccur_PostFix(ref bool __result, MentalBreakWorker __instance, Pawn pawn)
        {
            if (pawn?.genes != null && __instance.def.mentalState != VFEDefOf.Binging_Food)
            {
                var foodBingeList = pawn.genes.GetActiveGeneExtensions()
                    .Where(x => x.foodBingeMentalBreakSelectionChanceFactor != 1)
                    .Select(x => x.foodBingeMentalBreakSelectionChanceFactor);
                if (foodBingeList.Sum(x => x) > 20)
                {
                    __result = false;
                }
            }
        }
    }
}
