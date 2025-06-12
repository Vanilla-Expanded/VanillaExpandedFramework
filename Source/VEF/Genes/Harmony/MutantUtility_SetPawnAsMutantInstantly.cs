using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using VEF.Genes;

namespace VEF.Genes
{


    [HarmonyPatch(typeof(MutantUtility))]
    [HarmonyPatch("SetPawnAsMutantInstantly")]
    public static class VanillaExpandedFramework_MutantUtility_SetPawnAsMutantInstantly_Patch
    {
        [HarmonyPostfix]
        static void ActivateGhoulGenes(Pawn pawn, MutantDef mutant)
        {
            if (mutant== MutantDefOf.Ghoul && pawn?.genes?.GenesListForReading != null)
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene is Gene_Ghoul)
                    {
                        GeneUtils.ApplyGeneEffects(gene);
                    }

                }
            }




        }
    }








}
