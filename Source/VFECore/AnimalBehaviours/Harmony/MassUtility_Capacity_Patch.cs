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



namespace AnimalBehaviours
{
  

    /*This Harmony Prefix makes the creature carry more weight*/

    [HarmonyPatch(typeof(MassUtility))]
    [HarmonyPatch("Capacity")]
    public static class VanillaExpandedFramework_MassUtility_Capacity_Patch
    {
        [HarmonyPostfix]
        public static void MakeGigantelopesCarryMore(Pawn p, ref float __result)

        {
            bool flagIsCreatureMine = p.Faction != null && p.Faction.IsPlayer;
            bool flagDoesCreatureHaveTheHediffs = (p.TryGetComp<CompInitialHediff>() != null);
            bool flagCanCreatureCarryMore = false;
            if (flagDoesCreatureHaveTheHediffs)
            {
                flagCanCreatureCarryMore = (p.TryGetComp<CompInitialHediff>().Props.hediffname == "AA_CarryWeight");
            }

            if (flagIsCreatureMine && flagCanCreatureCarryMore)
            {
                int factor = p.TryGetComp<CompInitialHediff>().phase;
                __result = (p.BodySize * MassUtility.MassCapacityPerBodySize) + factor * factor * 1.5f;
            }

        }
    }













}

