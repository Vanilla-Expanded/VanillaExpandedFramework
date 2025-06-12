using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VEF.Genes
{

    [HarmonyPatch(typeof(Gene), "PostRemove")]
    public static class VanillaExpandedFramework_Gene_PostRemove_Patch
    {
        public static void Postfix(Gene __instance)
        {
            if (PawnGenerator.IsBeingGenerated(__instance.pawn) is false && __instance.Active)
            {
                GeneUtils.RemoveGeneEffects(__instance);
            }
        }
    }
}