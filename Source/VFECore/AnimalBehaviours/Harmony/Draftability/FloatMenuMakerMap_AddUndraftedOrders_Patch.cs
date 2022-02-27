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

    /*This Harmony Prefix makes jobs not return an error if the player right clicks something with a drafted animal
         */
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("AddUndraftedOrders")]
    public static class VanillaExpandedFramework_FloatMenuMakerMap_AddUndraftedOrders_Patch
    {
        [HarmonyPrefix]
        public static bool AvoidGeneralErrorIfPawnIsAnimal(Pawn pawn)

        {
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.Contains(pawn);

            if (flagIsCreatureDraftable)
            {
                return false;
            }
            else return true;

        }
    }


}
