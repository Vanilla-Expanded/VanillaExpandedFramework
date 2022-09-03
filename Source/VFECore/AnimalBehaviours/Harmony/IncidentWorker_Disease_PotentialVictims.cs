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




    [HarmonyPatch(typeof(IncidentWorker_Disease))]
    [HarmonyPatch("PotentialVictims")]

    public static class VanillaExpandedFramework_IncidentWorker_Disease_PotentialVictims_Patch
    {

        public static IEnumerable<Pawn> Postfix(IEnumerable<Pawn> values)
        {
            List<Pawn> resultingList = new List<Pawn>();

            foreach (Pawn pawn in values)
            {
                if (!AnimalCollectionClass.nodisease_animals.Contains(pawn))
                {
                    resultingList.Add(pawn);
                }
            }
            return resultingList;

           
            

        }

    }





}
