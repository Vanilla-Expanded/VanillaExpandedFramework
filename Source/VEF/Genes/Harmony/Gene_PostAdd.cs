using HarmonyLib;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VEF.Genes
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
    public static class VanillaExpandedFramework_PawnGenerator_GenerateGenes_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.genes != null)
            {
                List<Gene> genes = pawn.genes.GenesListForReading;
                foreach (Gene gene in genes)
                {
                    if (gene.Active)
                    {
                        GeneUtils.ApplyGeneEffects(gene);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(Gene), "PostAdd")]
    public static class VanillaExpandedFramework_Gene_PostAdd_Patch
    {
        public static void Postfix(Gene __instance)
        {
            if (PawnGenerator.IsBeingGenerated(__instance.pawn) is false && __instance.Active)
            {
                GeneUtils.ApplyGeneEffects(__instance);
            }
        }
    }
}