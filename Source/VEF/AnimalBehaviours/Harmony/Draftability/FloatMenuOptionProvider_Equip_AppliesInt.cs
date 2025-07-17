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


    [HarmonyPatch(typeof(FloatMenuOptionProvider_Equip))]
    [HarmonyPatch("AppliesInt")]
    public static class VanillaExpandedFramework_FloatMenuOptionProvider_Equip_AppliesInt_Patch
    {
        [HarmonyPostfix]
        static void NoWeaponEquipping(FloatMenuContext context, ref bool __result)
        {
            if (StaticCollectionsClass.draftable_animals.Contains(context.FirstSelectedPawn) && !StaticCollectionsClass.canEquipWeapon_animals.Contains(context.FirstSelectedPawn))
            {
                __result = false;
            }

        }
    }
}
