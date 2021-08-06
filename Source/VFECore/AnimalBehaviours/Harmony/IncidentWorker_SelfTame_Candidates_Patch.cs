using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AnimalBehaviours
{




    [HarmonyPatch(typeof(IncidentWorker_SelfTame))]
    [HarmonyPatch("Candidates")]

    public static class VanillaExpandedFramework_IncidentWorker_SelfTame_Candidates_Patch
    {

        public static IEnumerable<Pawn> Postfix(IEnumerable<Pawn> values)
        {

            List<PawnKindDef> animalListResult = new List<PawnKindDef>();
            List<AnimalsUnaffectedBySelfTameDef> allUnaffectedLists = DefDatabase<AnimalsUnaffectedBySelfTameDef>.AllDefsListForReading;
            foreach (AnimalsUnaffectedBySelfTameDef individualList in allUnaffectedLists)
            {
                animalListResult.AddRange(individualList.unaffectedBySelfTamePawns);
            }

            if (animalListResult.Count > 0)
            {
                List<Pawn> resultingList = new List<Pawn>();

                foreach (Pawn pawn in values)
                {
                    if (!animalListResult.Contains(pawn.kindDef))
                    {
                        resultingList.Add(pawn);
                    }
                }
               
                return resultingList;

            }
            else {
               
                return values; 
            }
            

        }

    }





}
