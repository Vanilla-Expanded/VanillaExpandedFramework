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

    /*This Harmony Postfix makes the creature respond to clicks on the map screen, so it can be controlled
         */
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("CanTakeOrder")]
    public static class VanillaExpandedFramework_FloatMenuMakerMap_CanTakeOrder_Patch
    {
        [HarmonyPostfix]
        public static void MakePawnControllable(Pawn pawn, ref bool __result)

        {
            bool flagIsCreatureMine = pawn.Faction != null && pawn.Faction?.IsPlayer==true;
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.Contains(pawn);

            if (flagIsCreatureDraftable && flagIsCreatureMine)
            {
                //Log.Message("You should be controllable now");
                __result = true;
            }

        }
    }


}
