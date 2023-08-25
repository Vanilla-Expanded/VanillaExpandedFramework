using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;
using VFEMech;

namespace AnimalBehaviours
{


    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("WorkTypeIsDisabled")]
    public static class VanillaExpandedFramework_Pawn_WorkTypeIsDisabled_Patch
    {
        [HarmonyPostfix]
        static void RemoveTendFromAnimals(WorkTypeDef w, Pawn __instance, ref bool __result)
        {
            if (w == WorkTypeDefOf.Doctor && AnimalCollectionClass.draftable_animals.Contains(__instance) 
                && !(__instance is Machine) && __instance.RaceProps.IsMechanoid is false)
            {
                __result = true;
            }
        }
    }
}
