using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System;
using Verse.AI;
using RimWorld.Planet;


namespace AnimalBehaviours
{

    /*This Harmony Prefix makes jobs not return an error if the player right clicks something with a drafted animal
         */
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("AddDraftedOrders")]
    public static class VanillaExpandedFramework_FloatMenuMakerMap_AddDraftedOrders_Patch
    {
        [HarmonyPrefix]
        public static bool AvoidGeneralErrorIfPawnIsAnimal(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts, bool suppressAutoTakeableGoto)

        {
            bool flagIsCreatureDraftable = AnimalCollectionClass.draftable_animals.ContainsKey(pawn);

            if (flagIsCreatureDraftable)
            {
                IntVec3 clickCell = IntVec3.FromVector3(clickPos);
                // AddJobGiverWorkOrders(clickPos, pawn, opts, drafted: true);
                FloatMenuOption floatMenuOption3 = GotoLocationOption(clickCell, pawn, suppressAutoTakeableGoto);
                if (floatMenuOption3 != null)
                {
                    opts.Add(floatMenuOption3);
                }

                return false;
            }
            else return true;

        }

        private static FloatMenuOption GotoLocationOption(IntVec3 clickCell, Pawn pawn, bool suppressAutoTakeableGoto)
        {
            if (suppressAutoTakeableGoto)
            {
                return null;
            }
            IntVec3 curLoc = CellFinder.StandableCellNear(clickCell, pawn.Map, 2.9f);
            if (curLoc.IsValid && curLoc != pawn.Position)
            {
                if (!pawn.CanReach(curLoc, PathEndMode.OnCell, Danger.Deadly))
                {
                    return new FloatMenuOption("CannotGoNoPath".Translate(), null);
                }
                Action action = delegate
                {
                    FloatMenuMakerMap.PawnGotoAction(clickCell, pawn, RCellFinder.BestOrderedGotoDestNear(curLoc, pawn));
                };
                return new FloatMenuOption("GoHere".Translate(), action, MenuOptionPriority.GoHere)
                {
                    autoTakeable = true,
                    autoTakeablePriority = 10f
                };
            }
            return null;
        }


    }




}
