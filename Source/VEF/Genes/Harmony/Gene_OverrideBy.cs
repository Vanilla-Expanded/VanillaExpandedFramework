using HarmonyLib;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VEF.Genes
{

    [HarmonyPatch(typeof(Gene), "OverrideBy")]
    public static class VanillaExpandedFramework_Gene_OverrideBy_Patch
    {
        public static void Postfix(Gene __instance, Gene overriddenBy)
        {
            if (overriddenBy!=null)
            {
                GeneUtils.RemoveGeneEffects(__instance);
            } else GeneUtils.ApplyGeneEffects(__instance);
        }
    }
}