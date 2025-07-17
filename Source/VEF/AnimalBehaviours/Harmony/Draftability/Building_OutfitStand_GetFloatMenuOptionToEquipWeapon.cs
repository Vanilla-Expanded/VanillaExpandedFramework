using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;
using RimWorld.Planet;

namespace VEF.AnimalBehaviours
{


    [HarmonyPatch(typeof(Building_OutfitStand))]
    [HarmonyPatch("GetFloatMenuOptionToEquipWeapon")]
    public static class VanillaExpandedFramework_Building_OutfitStand_GetFloatMenuOptionToEquipWeapon_Patch
    {
        [HarmonyPostfix]
        static void NoWeaponEquipping(Pawn selPawn, ref FloatMenuOption __result)
        {
            if (StaticCollectionsClass.draftable_animals.Contains(selPawn) && !StaticCollectionsClass.canEquipWeapon_animals.Contains(selPawn))
            {
                __result = null;
            }

        }
    }
}
