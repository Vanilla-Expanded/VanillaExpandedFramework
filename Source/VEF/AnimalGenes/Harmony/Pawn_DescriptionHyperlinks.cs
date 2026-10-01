using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.AI;
using RimWorld.Planet;
using System;


namespace VEF.AnimalGenes
{

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("DescriptionHyperlinks")]
    [HarmonyPatch(MethodType.Getter)]

    public static class VEF_AnimalGenes_Pawn_DescriptionHyperlinks_Patch
    {
        [HarmonyPostfix]
        public static IEnumerable<DefHyperlink> Postfix(IEnumerable<DefHyperlink> values, Pawn __instance)
        {
  
            foreach (var value in values)
            {
                yield return value;
            }
            if(__instance.TryGetComp<CompAnimalGenes>() is CompAnimalGenes comp)
            {
                foreach (AnimalGeneDef gene in comp.genes)
                {
                    yield return new DefHyperlink(gene);
                }
            }
        }


    }

}
