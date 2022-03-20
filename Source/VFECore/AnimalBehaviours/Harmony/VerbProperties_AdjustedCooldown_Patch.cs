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

    [HarmonyPatch(typeof(VerbProperties))]
    [HarmonyPatch("AdjustedCooldown")]
    [HarmonyPatch(new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing) })]
    public static class VanillaExpandedFramework_VerbProperties_AdjustedCooldown_Patch
    {
        [HarmonyPostfix]
        public static void LastStand(Tool tool, Pawn attacker, Thing equipment, ref float __result)

        {
            if (attacker != null) {
                if (AnimalCollectionClass.IsLastStandAnimal(attacker))
                {
                    float health = attacker.health.summaryHealth.SummaryHealthPercent;
                    float factor = ((AnimalCollectionClass.LastStandAnimalRate(attacker) - 1) * (1 - health)) + 1;

                    __result = __result / factor;


                }
            }
            

        }
    }













}

