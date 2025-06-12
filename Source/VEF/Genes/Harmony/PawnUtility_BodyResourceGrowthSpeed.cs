using VEF.AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using VEF;

namespace VEF.Genes
{

    [HarmonyPatch(typeof(PawnUtility))]
    [HarmonyPatch("BodyResourceGrowthSpeed")]
    public static class VanillaExpandedFramework_PawnUtility_BodyResourceGrowthSpeed_Patch
    {

        [HarmonyPostfix]
        public static void MultiplyPregnancy(ref float __result, Pawn pawn)

        {
            if (StaticCollectionsClass.pregnancySpeedFactor_gene_pawns.ContainsKey(pawn))
            {
                __result = __result * StaticCollectionsClass.pregnancySpeedFactor_gene_pawns[pawn];
            }
        }
    }

}
