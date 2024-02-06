using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;


namespace VanillaGenesExpanded
{

    [HarmonyPatch(typeof(Pawn), "ButcherProducts")]
    public static class VanillaGenesExpanded_Pawn_ButcherProducts_Patch
    {
        public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn __instance)
        {
            var pawn = __instance;

            if (StaticCollectionsClass.meat_gene_pawns.ContainsKey(pawn))
            {
                foreach (Thing thing in __result)
                {
                    if (thing.def == ThingDefOf.Meat_Human)
                    {
                        Thing thingReplacing = ThingMaker.MakeThing(StaticCollectionsClass.meat_gene_pawns[pawn]);
                        thingReplacing.stackCount = thing.stackCount;
                        yield return thingReplacing;
                    }
                    else { yield return thing; }
                    
                }
            }
            

           
        }
    }
}
