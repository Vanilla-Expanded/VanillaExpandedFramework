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
   
    [HarmonyPatch(typeof(Pawn_MindState))]
    [HarmonyPatch("StartFleeingBecauseOfPawnAction")]
    public static class VanillaExpandedFramework_Pawn_MindState_StartFleeingBecauseOfPawnAction_Patch
    {
        [HarmonyPrefix]
        public static bool DontFlee(Pawn_MindState __instance)

        {

            bool flagDoesCReatureNotFlee = AnimalCollectionClass.nofleeing_animals.Contains(__instance.pawn);

            if (flagDoesCReatureNotFlee)
            {

                return false;
            }
            else return true;


        }
    }
}
