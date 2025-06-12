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
   
    [HarmonyPatch(typeof(Pawn_FilthTracker))]
    [HarmonyPatch("Notify_EnteredNewCell")]
    public static class VanillaExpandedFramework_Pawn_FilthTracker_Notify_EnteredNewCell_Patch
    {
        [HarmonyPrefix]
        public static bool DontDealWithFilth(Pawn ___pawn)

        {

            

            if (StaticCollectionsClass.nofilth_animals.Contains(___pawn))
            {

                return false;
            }
            else return true;


        }
    }
}
