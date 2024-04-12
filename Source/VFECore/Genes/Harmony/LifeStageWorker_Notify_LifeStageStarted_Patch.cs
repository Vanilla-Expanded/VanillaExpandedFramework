using HarmonyLib;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(LifeStageWorker), "Notify_LifeStageStarted")]
    public static class LifeStageWorker_Notify_LifeStageStarted_Patch
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
}