using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VFEMech;
using Verse.Sound;

namespace VFECore
{
    [HarmonyPatch(typeof(Projectile), "Launch", new Type[]
    {
        typeof(Thing),
        typeof(Vector3),
        typeof(LocalTargetInfo),
        typeof(LocalTargetInfo),
        typeof(ProjectileHitFlags),
        typeof(bool),
        typeof(Thing),
        typeof(ThingDef)
    })]
    public static class Projectile_Launch_Patch
    {
        public static void Postfix(Projectile __instance, Thing launcher, Vector3 origin, ref LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            if (__instance is ExpandableProjectile expandableProjectile && expandableProjectile.def.reachMaxRangeAlways)
            {
                expandableProjectile.SetDestinationToMax(equipment);
            }
            else if (__instance.IsHomingProjectile(out var comp))
            {
                __instance.usedTarget = __instance.intendedTarget;
                __instance.SetDestination(__instance.intendedTarget.CenterVector3 + comp.DispersionOffset);
                comp.originLaunchCell = (Vector3)NonPublicFields.Projectile_origin.GetValue(__instance);
            }
            else if (launcher is Pawn pawn && pawn.health.hediffSet.hediffs.Any(x => x.TryGetComp<HediffComp_Targeting>()?.Props.neverMiss ?? false))
            {
                __instance.usedTarget = __instance.intendedTarget;
                __instance.SetDestination(__instance.intendedTarget.CenterVector3);
            }
        }

        public static void SetDestination(this Projectile projectile, Vector3 destination)
        {
            var projDestination = ((Vector3)NonPublicFields.Projectile_destination.GetValue(projectile));
            if (Vector3.Distance(projDestination.Yto0(), destination.Yto0()) >= 0.1f)
            {
                NonPublicFields.Projectile_origin.SetValue(projectile, projectile.ExactPosition);
                NonPublicFields.Projectile_destination.SetValue(projectile, Vector3.RotateTowards(projDestination, destination, 1, 9999999));
                NonPublicFields.Projectile_ticksToImpact.SetValue(projectile, Mathf.CeilToInt((float)NonPublicProperties.Projectile_get_StartingTicksToImpact(projectile)));
            }
        }
        public static bool IsHomingProjectile(this Projectile projectile, out CompHomingProjectile comp)
        {
            comp = projectile.GetComp<CompHomingProjectile>();
            return comp != null;
        }
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Tick))]
    public static class Projectile_Tick_Patch
    {
        public static void Prefix(Projectile __instance)
        {
            if (__instance.IsHomingProjectile(out var comp))
            {
                if (comp.CanChangeTrajectory())
                {
                    __instance.SetDestination(__instance.intendedTarget.CenterVector3);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class Projectile_Impact_Patch
    {
        public static bool Prefix(Projectile __instance, ref Thing hitThing)
        {
            if (__instance.IsHomingProjectile(out var comp))
            {
                if (hitThing != __instance.intendedTarget.Thing)
                {
                    foreach (var thing in GenRadial.RadialDistinctThingsAround(__instance.Position, __instance.Map, 3f, true))
                    {
                        if (thing == __instance.intendedTarget.Thing)
                        {
                            if (Vector3.Distance(thing.DrawPos.Yto0(), __instance.ExactPosition.Yto0()) <= 0.5f)
                            {
                                hitThing = thing;
                            }
                        }
                    }
                }
                if (hitThing != null && comp.Props.hitSound != null)
                {
                    comp.Props.hitSound.PlayOneShot(hitThing);
                }
            }
            return true;
        }
    }
}
