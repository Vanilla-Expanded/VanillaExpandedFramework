using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFEMech;
using VFECore.Abilities;

namespace VFECore
{

    public static class Patch_Verb
    {
        public static bool forceHit;
        public static bool forceMiss;

        public static void CheckAccuracyEffects(Verb verb, LocalTargetInfo target, out bool forceHit, out bool forceMiss)
        {
            forceHit = forceMiss = false;
            if (verb.caster is Pawn attacker && attacker.health?.hediffSet?.hediffs != null)
            {
                foreach (var hediff in attacker.health.hediffSet.hediffs)
                {
                    var comp = hediff.TryGetComp<HediffComp_Targeting>();
                    if (comp != null)
                    {
                        forceHit  = comp.Props.neverMiss;
                        forceMiss = comp.Props.neverHit;
                    }
                }
            }

            if (target.Thing is Pawn attackee && attackee.health?.hediffSet?.hediffs != null)
            {
                foreach (var hediff in attackee.health.hediffSet.hediffs)
                {
                    var comp = hediff.TryGetComp<HediffComp_Targeting>();
                    if (comp != null)
                    {
                        forceHit  = comp.Props.alwaysHit;
                        forceMiss = comp.Props.alwaysMiss;
                    }
                }
            }

            var projectileClass = verb.GetProjectile()?.thingClass;
            if (projectileClass != null && typeof(TeslaProjectile).IsAssignableFrom(projectileClass))
            {
                forceHit = true;
            }
        }

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
            public static void Prefix(Verb verb, LocalTargetInfo target)
            {
                CheckAccuracyEffects(verb, target, out forceHit, out forceMiss);
            }
        }

        [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
        public static class Verb_LaunchProjectile_TryCastShot
        {
            public static void Prefix(Verb_LaunchProjectile __instance)
            {
                CheckAccuracyEffects(__instance, __instance.CurrentTarget, out forceHit, out forceMiss);
            }
            public static void Postfix()
            {
                forceHit = false;
                forceMiss = false;
            }
        }

        [HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_StandardTarget", MethodType.Getter)]
        public static class ShotReport_AimOnTargetChance_StandardTarget
        {
            public static void Postfix(ref float __result)
            {
                if (forceHit)
                {
                    __result = 1f;
                } else if (forceMiss)
                {
                    __result = 0f;
                }
            }
        }
    }

}
