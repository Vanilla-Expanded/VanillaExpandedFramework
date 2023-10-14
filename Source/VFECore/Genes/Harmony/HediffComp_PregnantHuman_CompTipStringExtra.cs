using HarmonyLib;
using RimWorld;
using Verse.Grammar;
using Verse;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Verse.AI;
using AnimalBehaviours;

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(HediffComp_PregnantHuman))]
    [HarmonyPatch("CompTipStringExtra", MethodType.Getter)]
    public static class VanillaGenesExpanded_HediffComp_PregnantHuman_CompTipStringExtra_Patch
    {



        [HarmonyPostfix]
        public static void AddGeneMultiplierExplanation(HediffWithComps ___parent, ref string __result)

        {


            if (StaticCollectionsClass.pregnancySpeedFactor_gene_pawns.ContainsKey(___parent.pawn))
            {
                __result = __result + "\n" + "VGE_PregnancyFactor".Translate(StaticCollectionsClass.pregnancySpeedFactor_gene_pawns[___parent.pawn]);
            }



        }


    }
}
