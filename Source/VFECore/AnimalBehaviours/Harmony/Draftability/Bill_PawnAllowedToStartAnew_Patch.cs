using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;


namespace AnimalBehaviours
{

    /*This Harmony Prefix makes bills not return an error if the player right clicks a workbench with a drafted animal
         */
    [HarmonyPatch(typeof(Bill))]
    [HarmonyPatch("PawnAllowedToStartAnew")]
    public static class VanillaExpandedFramework_Bill_PawnAllowedToStartAnew_Patch
    {
        [HarmonyPrefix]
        public static bool AvoidBillErrorIfPawnIsAnimal(Pawn p)

        {
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.Contains(p);

            if (flagIsCreatureDraftable)
            {
                return false;
            }
            else return true;

        }
    }


}
