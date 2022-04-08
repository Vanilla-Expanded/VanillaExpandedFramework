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

    /*This first Harmony postfix deals with adding a Pawn_DraftController if it detects the creature
     * belongs to the player and to the custom class CompDraftable. It also adds a Pawn_EquipmentTracker
     * because some ugly errors are produced otherwise, though it is basically unused
     * 
     */
    [HarmonyPatch(typeof(PawnComponentsUtility))]
    [HarmonyPatch("AddAndRemoveDynamicComponents")]
    public static class VanillaExpandedFramework_PawnComponentsUtility_AddAndRemoveDynamicComponents_Patch
    {
        [HarmonyPostfix]
        static void AddDraftability(Pawn pawn)
        {
            //These two flags detect if the creature is part of the colony and if it has the custom class
            bool flagIsCreatureMine = pawn.Faction != null && pawn.Faction.IsPlayer;
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.Contains(pawn);


            if (flagIsCreatureMine && flagIsCreatureDraftable)
            {
                //Log.Message("Patching "+ pawn.kindDef.ToString() + " with a draft controller and equipment tracker");
                //If everything goes well, add drafter and equipment to the pawn 
                if (pawn.drafter is null)
                {
                    pawn.drafter = new Pawn_DraftController(pawn);
                }
                if (pawn.equipment is null)
                {
                    pawn.equipment = new Pawn_EquipmentTracker(pawn);
                }
            }
        }
    }



}
