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


    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("AddJobGiverWorkOrders")]
    public static class VanillaExpandedFramework_FloatMenuMakerMap_AddJobGiverWorkOrders_Patch
    {
        [HarmonyPrefix]
        public static bool SkipIfAnimal(Pawn pawn)

        {

            if (AnimalCollectionClass.draftable_animals.Contains(pawn))
            {
                return false;
            }
            return true;

        }
    }


}
