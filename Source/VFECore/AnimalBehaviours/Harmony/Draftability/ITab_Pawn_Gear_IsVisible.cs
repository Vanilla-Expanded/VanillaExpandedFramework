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


    [HarmonyPatch(typeof(ITab_Pawn_Gear))]
    [HarmonyPatch("ShouldShowEquipment")]
    public static class VanillaExpandedFramework_ITab_Pawn_Gear_IsVisible_Patch
    {
        [HarmonyPostfix]
        static void RemoveTab(Pawn p, ref bool __result)
        {
            if (AnimalCollectionClass.draftable_animals.Contains(p) && !(p is Machine))
            {
                __result = false;
            }
              
        }
    }
}
