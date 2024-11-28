using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using VanillaGenesExpanded;

namespace VanillaGenesExpanded
{


    [HarmonyPatch(typeof(Hediff_Shambler))]
    [HarmonyPatch("PostMake")]
    public static class VanillaGenesExpanded_Hediff_Shambler_PostMake_Patch
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
