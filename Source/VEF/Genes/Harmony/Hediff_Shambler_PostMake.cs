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


    [HarmonyPatch(typeof(Hediff_Shambler))]
    [HarmonyPatch("PostMake")]
    public static class VanillaExpandedFramework_Hediff_Shambler_PostMake_Patch
    {
        [HarmonyPostfix]
        static void ActivateShamblerGenes(Hediff_Shambler __instance)
        {
            if (__instance.pawn?.genes?.GenesListForReading != null)
            {
                foreach (Gene gene in __instance.pawn.genes.GenesListForReading)
                {
                    if(gene is Gene_Shambler)
                    {
                        GeneUtils.ApplyGeneEffects(gene);
                    }

                }
            }
            
            


        }
    }








}
