using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace VanillaWeaponsExpandedLaser.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class VWEL_Pawn_GetGizmos_WeaponGizmoGetter_Patch
    {
        [HarmonyPostfix]
        public static void GetGizmos_PostFix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            var pawn_EquipmentTracker = __instance.equipment;
            if (pawn_EquipmentTracker != null)
            {
                var thingWithComps = pawn_EquipmentTracker.Primary;
                //(ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);

                if (thingWithComps != null)
                {
                    var CompLaserCapacitor = thingWithComps.GetComp<CompLaserCapacitor>();
                    if (CompLaserCapacitor != null)
                        if (GizmoGetter(CompLaserCapacitor).Count() > 0)
                            if (__instance != null)
                                if (__instance.Faction == Faction.OfPlayer)
                                    __result = __result.Concat(GizmoGetter(CompLaserCapacitor));
                }
            }
        }
        public static IEnumerable<Gizmo> GizmoGetter(CompLaserCapacitor CompWargearWeapon)
        {
            var enumerator = CompWargearWeapon.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }
        }

    }

}