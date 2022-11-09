using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Verse.AI;
using RimWorld.Planet;



namespace VanillaGenesExpanded
{



    [HarmonyPatch(typeof(MassUtility))]
    [HarmonyPatch("Capacity")]
    public static class VanillaGenesExpanded_MassUtility_Capacity_Patch
    {
        [HarmonyPostfix]
        public static void GenesAffectCarryCapacity(Pawn p, ref float __result)

        {
            if (StaticCollectionsClass.caravanCarryingFactor_gene_pawns.ContainsKey(p))
            {
                __result = __result*StaticCollectionsClass.caravanCarryingFactor_gene_pawns[p];
            }

        }
    }













}

