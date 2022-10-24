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



    public static class PawnUtility_Mated
    {

        [HarmonyPatch(typeof(PawnUtility))]
        [HarmonyPatch(nameof(PawnUtility.Mated))]
        public static class VanillaExpandedFramework_PawnUtility_Mated_Patch
        {

            public static bool Prefix(Pawn male, Pawn female)
            {

                if (!female.ageTracker.CurLifeStage.reproductive)
                {
                    return false;
                }
                CompExplodingEggLayer compEggLayer = female.TryGetComp<CompExplodingEggLayer>();
                if (compEggLayer != null)
                {
                    compEggLayer.Fertilize(male);
                    return false;
                }

                



                return true;

            }

        }

    }



}
