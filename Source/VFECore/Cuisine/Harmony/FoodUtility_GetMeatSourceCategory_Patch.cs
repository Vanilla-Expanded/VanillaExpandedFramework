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

namespace VanillaCookingExpanded
{
    /*This Harmony Prefix avoids the creature trying to eat normal food
    */
    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch("GetMeatSourceCategory")]
    public static class VanillaExpandedFramework_FoodUtility_GetMeatSourceCategory_Patch
    {
        [HarmonyPrefix]
        public static bool DontCrapTheBedWithIngredientsWithoutNutrition(ThingDef source)

        {
           
            if (source.ingestible == null)
            {

                return false;
            }
            else return true;


        }
    }
}
