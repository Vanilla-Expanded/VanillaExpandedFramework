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

namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GeneratePawnRelations")]
    public static class VanillaGenesExpanded_PawnGenerator_GeneratePawnRelations_Patch
    {
        [HarmonyPrefix]
        public static bool DisableRelations(Pawn pawn)
        {
            if (StaticCollectionsClass.swappedgender_gene_pawns.Contains(pawn))
            {
                return false;
            }
            else return true;
        }
    }
}
