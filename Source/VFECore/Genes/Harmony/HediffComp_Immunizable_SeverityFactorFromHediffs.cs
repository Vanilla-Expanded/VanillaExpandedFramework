using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;


namespace VanillaGenesExpanded
{


    [HarmonyPatch(typeof(HediffComp_Immunizable))]
    [HarmonyPatch("SeverityFactorFromHediffs", MethodType.Getter)]
    public static class VanillaExpandedFramework_HediffComp_Immunizable_SeverityFactorFromHediffs_Patch
    {
        [HarmonyPostfix]
        static void AddDiseaseFactor(HediffComp_Immunizable __instance, ref float __result)
        {
            if (StaticCollectionsClass.diseaseProgressionFactor_gene_pawns.ContainsKey(__instance.Pawn))
            {
                __result = __result * StaticCollectionsClass.diseaseProgressionFactor_gene_pawns[__instance.Pawn];
            }
        } 
    }
}
