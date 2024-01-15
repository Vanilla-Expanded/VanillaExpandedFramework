using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(Gene), "ExposeData")]
    public static class VanillaGenesExpanded_Gene_ExposeData_Patch
    {
        public static void Postfix(Gene __instance)
        {
            if (__instance.pawn != null && PawnGenerator.IsBeingGenerated(__instance.pawn) is false && __instance.Active)
            {
                GeneUtils.ApplyGeneEffects(__instance);
            }
        }
    }
}