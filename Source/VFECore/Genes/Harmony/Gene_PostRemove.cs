using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(Gene), "PostRemove")]
    public static class VanillaGenesExpanded_Gene_PostRemove_Patch
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