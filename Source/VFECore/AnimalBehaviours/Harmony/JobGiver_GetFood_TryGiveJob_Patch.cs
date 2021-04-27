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

namespace AnimalBehaviours
{
    /*This Harmony Prefix avoids the creature trying to eat normal food
    */
    [HarmonyPatch(typeof(JobGiver_GetFood))]
    [HarmonyPatch("TryGiveJob")]
    public static class AlphaAnimals_JobGiver_GetFood_GetPriority_Patch
    {
        [HarmonyPrefix]
        public static bool StopEatingThings(Pawn pawn)

        {
            CompEatWeirdFood comp = pawn.TryGetComp<CompEatWeirdFood>();

            if (comp != null)
            {
               
                return false;
            }
            else return true;


        }
    }
}
