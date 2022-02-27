using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFEMech;

namespace VFECore
{

    public static class Patch_Verb
    {

        [HarmonyPatch(typeof(Verb), nameof(Verb.Available))]
        public static class Available
        {

            public static void Postfix(Verb __instance, ref bool __result)
            {
                // Unusable shield verbs don't get counted
                if (__result && __instance.EquipmentSource != null && __instance.EquipmentSource.IsShield(out CompShield shieldComp))
                    __result = shieldComp.UsableNow;
            }
        }

        [HarmonyPatch(typeof(VerbProperties), "AdjustedCooldown", new Type[]
        {
            typeof(Verb), typeof(Pawn)
        })]
        public static class VerbProperties_AdjustedCooldown_Patch
        {
            public static void Postfix(ref float __result, Verb ownerVerb, Pawn attacker)
            {
                var pawn = ownerVerb.CasterPawn;
                if (pawn != null)
                {
                    __result *= pawn.GetStatValue(VFEDefOf.VEF_VerbCooldownFactor);
                }
            }
        }

        [HarmonyPatch(typeof(ShotReport), "HitReportFor")]
        public static class ShotReport_HitReportFor
        {
            public static bool accuracy;
            public static void Prefix(ref ShotReport __result, Thing caster, Verb verb, LocalTargetInfo target)
            {
                var projectileClass = verb.GetProjectile()?.thingClass;
                if (projectileClass != null && typeof(TeslaProjectile).IsAssignableFrom(projectileClass))
                {
                    accuracy = true;
                }
            }
        }

        [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
        public static class Verb_LaunchProjectile_TryCastShot
        {
            public static void Prefix(Verb_LaunchProjectile __instance)
            {
                var projectileClass = __instance.GetProjectile()?.thingClass;
                if (projectileClass != null && typeof(TeslaProjectile).IsAssignableFrom(projectileClass))
                {
                    ShotReport_HitReportFor.accuracy = true;
                }
            }
            public static void Postfix()
            {
                ShotReport_HitReportFor.accuracy = false;
            }
        }

        [HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_StandardTarget", MethodType.Getter)]
        public static class ShotReport_AimOnTargetChance_StandardTarget
        {
            public static void Postfix(ref float __result)
            {
                if (ShotReport_HitReportFor.accuracy)
                {
                    __result = 1f;
                }
            }
        }
    }

}
