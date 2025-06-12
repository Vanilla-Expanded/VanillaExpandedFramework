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

namespace VEF.AnimalBehaviours
{
    /*This Harmony Prefix avoids the creature trying to eat normal food
    */
    [HarmonyPatch(typeof(JobGiver_GetFood))]
    [HarmonyPatch("TryGiveJob")]
    public static class VanillaExpandedFramework_JobGiver_GetFood_GetPriority_Patch
    {
        [HarmonyPrefix]
        public static bool StopEatingThings(Pawn pawn)

        {
          
            bool flagIsCreatureWeirdEater = StaticCollectionsClass.weirdEaters_animals.Contains(pawn);

            if (flagIsCreatureWeirdEater)
            {
               
                return false;
            }
            else return true;


        }
    }
}
