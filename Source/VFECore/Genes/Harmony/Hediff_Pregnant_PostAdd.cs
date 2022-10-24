using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(Hediff_Pregnant), "PostAdd")]
    public static class VanillaGenesExpanded_Hediff_Pregnant_PostAdd_Patch
    {
        [HarmonyPostfix]
        public static void CauseEggFertilization(Hediff_Pregnant __instance,Pawn ___father)
        {
          
            foreach (Hediff hediff in __instance.pawn?.health?.hediffSet?.hediffs)
            {
                if(hediff.TryGetComp<HediffComp_HumanEggLayer>() != null)
                {
                  
                    HediffComp_HumanEggLayer comp = hediff?.TryGetComp<HediffComp_HumanEggLayer>();
                    comp.DisableNormalPregnancy();
                    if (!comp.FullyFertilized) {
                        comp.Fertilize(___father);

                        if (___father?.genes !=null) {
                            foreach (Gene gene in ___father.genes.Endogenes)
                            {
                                comp.fatherGenes.Add(gene.def);
                            } 
                        }
                        if (__instance.pawn.genes != null)
                        {
                            foreach (Gene gene in __instance.pawn.genes.Endogenes)
                            {
                                comp.motherGenes.Add(gene.def);
                            }
                        }

                    }
                    


                }
            }



        }
    }
}