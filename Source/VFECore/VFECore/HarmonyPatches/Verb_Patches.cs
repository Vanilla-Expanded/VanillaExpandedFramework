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
using Verse.AI;

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

        private static IntVec3 FindCellToHit(Vector3 origin, Projectile projectile, Pawn victim)
        {
            if (victim.pather?.curPath != null)
            {
                float projectileSpeed = 0;
                int victimSpeed = 0;
                bool startCalculation = false;
                var nodes = victim.pather.curPath.NodesReversed.ListFullCopy();
                nodes.Reverse();
                var prevCell = victim.DrawPos.ToIntVec3();
                var speedPairs = new Dictionary<IntVec3, Pair<float, float>>();
                foreach (var cell in nodes)
                {
                    if (startCalculation)
                    {
                        projectileSpeed = ((origin.Yto0() - cell.ToVector3Shifted().Yto0()).magnitude) / projectile.def.projectile.SpeedTilesPerTick;
                        victimSpeed += CostToMoveIntoCell(victim, prevCell, cell);
                        victim.Map.debugDrawer.FlashCell(cell, 0, Math.Abs(victimSpeed - projectileSpeed).ToString(), 20);
                        speedPairs[cell] = new Pair<float, float>(victimSpeed, projectileSpeed);
                    }
                    if (cell == victim.DrawPos.ToIntVec3())
                    {
                        startCalculation = true;
                    }
                    prevCell = cell;
                }
                if (speedPairs.Any())
                {
                    var closestCell = speedPairs.MinBy(x => Math.Abs(x.Value.First - x.Value.Second));
                    return closestCell.Key;
                }
            }
            return victim.Position;
        }

        private static int CostToMoveIntoCell(Pawn pawn, IntVec3 prevCell, IntVec3 c)
        {
            int num = (c.x != prevCell.x && c.z != prevCell.z) ? pawn.TicksPerMoveDiagonal : pawn.TicksPerMoveCardinal;
            num += pawn.Map.pathing.For(pawn).pathGrid.CalculatedCostAt(c, perceivedStatic: false, pawn.Position);
            Building edifice = c.GetEdifice(pawn.Map);
            if (edifice != null)
            {
                num += edifice.PathWalkCostFor(pawn);
            }
            if (num > 450)
            {
                num = 450;
            }
            if (pawn.CurJob != null)
            {
                Pawn locomotionUrgencySameAs = pawn.jobs.curDriver.locomotionUrgencySameAs;
                if (locomotionUrgencySameAs != null && locomotionUrgencySameAs != pawn && locomotionUrgencySameAs.Spawned)
                {
                    int num2 = CostToMoveIntoCell(locomotionUrgencySameAs, prevCell, c);
                    if (num < num2)
                    {
                        num = num2;
                    }
                }
                else
                {
                    switch (pawn.jobs.curJob.locomotionUrgency)
                    {
                        case LocomotionUrgency.Amble:
                            num *= 3;
                            if (num < 60)
                            {
                                num = 60;
                            }
                            break;
                        case LocomotionUrgency.Walk:
                            num *= 2;
                            if (num < 50)
                            {
                                num = 50;
                            }
                            break;
                        case LocomotionUrgency.Jog:
                            num = num;
                            break;
                        case LocomotionUrgency.Sprint:
                            num = Mathf.RoundToInt((float)num * 0.75f);
                            break;
                    }
                }
            }
            return Mathf.Max(num, 1);
        }

        [HarmonyPatch(typeof(Projectile), "Launch", new Type[]
        {
            typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags),typeof(bool), typeof(Thing), typeof(ThingDef)
        })]
        public static class Projectile_Launch_Patch
        {
            public static void Prefix(Projectile __instance, Thing launcher, Vector3 origin, ref LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
            {
                if (forceHit && intendedTarget.Thing is Pawn victim && victim.pather.MovingNow)
                {
                    usedTarget = FindCellToHit(origin, __instance, victim);
                }
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
