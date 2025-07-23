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



namespace VEF.AnimalBehaviours
{


    [HarmonyPatch(typeof(TrainableUtility))]
    [HarmonyPatch("TamenessCanDecay")]

    public static class VanillaExpandedFramework_TrainableUtility_TamenessCanDecay_Patch
    {
        [HarmonyPrefix]
        public static bool RemoveTamenessDecayCheck(ThingDef def)

        {
            if (StaticCollectionsClass.IsNoTamingDecayAnimal(def))
            {
                return false;

            }
            else return true;
        }
    }

    /*
     * 
    [HarmonyPatch(typeof(TrainableUtility))]
    [HarmonyPatch("TamenessCanDecay")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    public static class VanillaExpandedFramework_TrainableUtility_TamenessCanDecay_Patch
    {
        [HarmonyPrefix]
        public static bool RemoveTamenessDecayCheck(Pawn pawn)

        {
            if (StaticCollectionsClass.IsNoTamingDecayAnimal(pawn.def))
            {
                return false;

            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(TrainableUtility))]
    [HarmonyPatch("TamenessCanDecay")]
    [HarmonyPatch(new Type[] { typeof(ThingDef) })]
    public static class VanillaExpandedFramework_TrainableUtility_TamenessCanDecay_ForThingDef_Patch
    {
        [HarmonyPrefix]
        public static bool RemoveTamenessDecayCheck(ThingDef def)

        {
            if (StaticCollectionsClass.IsNoTamingDecayAnimal(def))
            {
                return false;

            }
            else return true;
        }
    }
*/


}
