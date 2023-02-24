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


    [HarmonyPatch(typeof(SchoolUtility))]
    [HarmonyPatch("CanTeachNow")]

    public static class VanillaExpandedFramework_SchoolUtility_CanTeachNow_Patch
    {
        [HarmonyPrefix]
        public static bool RemoveTeaching(Pawn teacher)

        {
            if (AnimalCollectionClass.draftable_animals.Contains(teacher))
            {
                return false;

            }
            else return true;
        }
    }


}
