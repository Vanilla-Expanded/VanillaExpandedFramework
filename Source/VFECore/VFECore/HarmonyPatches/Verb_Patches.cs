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
using System.Reflection.Emit;
using System.Reflection;

namespace VFECore
{
    public static class VerbAccuracyUtility
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

        // unused, doesn't really help for never miss effect
        //private static IntVec3 FindCellToHit(Vector3 origin, Projectile projectile, Pawn victim)
        //{
        //    if (victim.pather?.curPath != null)
        //    {
        //        float projectileSpeed = 0;
        //        int victimSpeed = 0;
        //        bool startCalculation = false;
        //        var nodes = victim.pather.curPath.NodesReversed.ListFullCopy();
        //        nodes.Reverse();
        //        var prevCell = victim.DrawPos.ToIntVec3();
        //        var speedPairs = new Dictionary<IntVec3, Pair<float, float>>();
        //        foreach (var cell in nodes)
        //        {
        //            if (startCalculation)
        //            {
        //                projectileSpeed = ((origin.Yto0() - cell.ToVector3Shifted().Yto0()).magnitude) / projectile.def.projectile.SpeedTilesPerTick;
        //                victimSpeed += CostToMoveIntoCell(victim, prevCell, cell);
        //                speedPairs[cell] = new Pair<float, float>(victimSpeed, projectileSpeed);
        //            }
        //            if (cell == victim.DrawPos.ToIntVec3())
        //            {
        //                startCalculation = true;
        //            }
        //            prevCell = cell;
        //        }
        //        if (speedPairs.Any())
        //        {
        //            var closestCell = speedPairs.MinBy(x => Math.Abs(x.Value.First - x.Value.Second));
        //            return closestCell.Key;
        //        }
        //    }
        //    return victim.Position;
        //}
        //
        //private static int CostToMoveIntoCell(Pawn pawn, IntVec3 prevCell, IntVec3 c)
        //{
        //    int num = (c.x != prevCell.x && c.z != prevCell.z) ? pawn.TicksPerMoveDiagonal : pawn.TicksPerMoveCardinal;
        //    num += pawn.Map.pathing.For(pawn).pathGrid.CalculatedCostAt(c, perceivedStatic: false, pawn.Position);
        //    Building edifice = c.GetEdifice(pawn.Map);
        //    if (edifice != null)
        //    {
        //        num += edifice.PathWalkCostFor(pawn);
        //    }
        //    if (num > 450)
        //    {
        //        num = 450;
        //    }
        //    if (pawn.CurJob != null)
        //    {
        //        Pawn locomotionUrgencySameAs = pawn.jobs.curDriver.locomotionUrgencySameAs;
        //        if (locomotionUrgencySameAs != null && locomotionUrgencySameAs != pawn && locomotionUrgencySameAs.Spawned)
        //        {
        //            int num2 = CostToMoveIntoCell(locomotionUrgencySameAs, prevCell, c);
        //            if (num < num2)
        //            {
        //                num = num2;
        //            }
        //        }
        //        else
        //        {
        //            switch (pawn.jobs.curJob.locomotionUrgency)
        //            {
        //                case LocomotionUrgency.Amble:
        //                    num *= 3;
        //                    if (num < 60)
        //                    {
        //                        num = 60;
        //                    }
        //                    break;
        //                case LocomotionUrgency.Walk:
        //                    num *= 2;
        //                    if (num < 50)
        //                    {
        //                        num = 50;
        //                    }
        //                    break;
        //                case LocomotionUrgency.Jog:
        //                    num = num;
        //                    break;
        //                case LocomotionUrgency.Sprint:
        //                    num = Mathf.RoundToInt((float)num * 0.75f);
        //                    break;
        //            }
        //        }
        //    }
        //    return Mathf.Max(num, 1);
        //}

        //[HarmonyPatch(typeof(Projectile), "Launch", new Type[]
        //{
        //    typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags),typeof(bool), typeof(Thing), typeof(ThingDef)
        //})]
        //public static class Projectile_Launch_Patch
        //{
        //    public static void Prefix(Projectile __instance, Thing launcher, Vector3 origin, ref LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        //    {
        //        if (forceHit && intendedTarget.Thing is Pawn victim && victim.pather.MovingNow)
        //        {
        //            usedTarget = FindCellToHit(origin, __instance, victim);
        //        }
        //    }
        //}

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
    public static class ShotReport_HitReportFor_Patch
    {
        public static Thing curCaster;
        public static void Prefix(Thing caster, Verb verb, LocalTargetInfo target)
        {
            curCaster = caster;
            VerbAccuracyUtility.CheckAccuracyEffects(verb, target, out VerbAccuracyUtility.forceHit, out VerbAccuracyUtility.forceMiss);
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class Verb_LaunchProjectile_TryCastShot
    {
        public static void Prefix(Verb_LaunchProjectile __instance)
        {
            VerbAccuracyUtility.CheckAccuracyEffects(__instance, __instance.CurrentTarget, out VerbAccuracyUtility.forceHit, out VerbAccuracyUtility.forceMiss);
        }
        public static void Postfix()
        {
            VerbAccuracyUtility.forceHit = false;
            VerbAccuracyUtility.forceMiss = false;
        }
    }

    [HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_StandardTarget", MethodType.Getter)]
    public static class ShotReport_AimOnTargetChance_StandardTarget
    {
        public static void Postfix(ref float __result)
        {
            if (VerbAccuracyUtility.forceHit)
            {
                __result = 1f;
            }
            else if (VerbAccuracyUtility.forceMiss)
            {
                __result = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(CastPositionFinder), "TryFindCastPosition")]
    public static class CastPositionFinder_TryFindCastPosition_Patch
    {
        public static void Prefix(ref CastPositionRequest newReq)
        {
            var meleeRangeOverride = newReq.verb?.EquipmentSource?.def.GetModExtension<ThingDefExtension>()?.meleeRangeOverride;
            if (meleeRangeOverride != null)
            {
                newReq.maxRangeFromTarget = Mathf.Max(newReq.maxRangeFromTarget, meleeRangeOverride.Value);
            }
        }
    }

    [HarmonyPatch(typeof(Verb), "TryFindShootLineFromTo")]
    public static class Verb_TryFindShootLineFromTo_Patch
    {
        public static void Prefix(Verb __instance)
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = __instance.CasterPawn;
        }

        public static void Postfix()
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = null;
        }
    }

    [HarmonyPatch]
    public static class Toils_Combat_GotoCastPosition_Patch
    {
        public static MethodBase TargetMethod()
        {
            var gotoCastPositionMethod = typeof(Toils_Combat).GetNestedTypes(AccessTools.all).SelectMany(innerType => AccessTools.GetDeclaredMethods(innerType))
                .FirstOrDefault(method => method.Name.Contains("<GotoCastPosition>") && method.ReturnType == typeof(void) && method.GetParameters().Length == 0);
            return gotoCastPositionMethod;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].OperandIs(ShootTuning.MeleeRange))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Job), "verbToUse"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Toils_Combat_GotoCastPosition_Patch), nameof(GetMeleeReachRange)));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static float GetMeleeReachRange(this Pawn caster, Verb verb)
        {
            var meleeRangeOverride = verb?.EquipmentSource?.def.GetModExtension<ThingDefExtension>()?.meleeRangeOverride;
            if (meleeRangeOverride != null)
            {
                return meleeRangeOverride.Value;
            }
            return ShootTuning.MeleeRange;
        }
    }

    [HarmonyPatch(typeof(AttackTargetFinder), "FindBestReachableMeleeTarget")]
    public static class AttackTargetFinder_FindBestReachableMeleeTarget_Patch
    {
        public static void Prefix(Pawn searcherPawn)
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = searcherPawn;
        }

        public static void Postfix()
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = null;
        }
    }

    [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob")]
    public static class JobGiver_ConfigurableHostilityResponse_TryGetAttackNearbyEnemyJob_Patch
    {
        public static void Prefix(Pawn pawn)
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = pawn;
        }

        public static void Postfix()
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = null;
        }
    }

    [HarmonyPatch]
    public static class Toils_Combat_FollowAndMeleeAttack_Patch
    {
        public static FieldInfo targetInd;

        public static void Prefix(Toil ___followAndAttack)
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = ___followAndAttack.actor;
        }

        public static void Postfix()
        {
            ReachabilityImmediate_CanReachImmediate_Patch.curPawn = null;
        }

        public static MethodBase TargetMethod()
        {
            foreach (var nested in typeof(Toils_Combat).GetNestedTypes(AccessTools.all))
            {
                foreach (var method in nested.GetMethods(AccessTools.all))
                {
                    if (method.Name.Contains("<FollowAndMeleeAttack>"))
                    {
                        targetInd = nested.GetField("targetInd");
                        return method;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            foreach (var instruction in codeInstructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder
                    && localBuilder.LocalIndex == 6)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, targetInd);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 5);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 6);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Toils_Combat_FollowAndMeleeAttack_Patch), "TryOverrideDestinationAndPathMode"));
                }
            }
        }

        public static void TryOverrideDestinationAndPathMode(TargetIndex targetInd, Pawn actor,
            ref LocalTargetInfo destination, ref PathEndMode mode)
        {
            Job curJob = actor.jobs.curJob;
            LocalTargetInfo target = curJob.GetTarget(targetInd);
            Thing thing = target.Thing;
            var verbToUse = actor.GetMeleeVerb();
            var meleeReachRange = actor.GetMeleeReachRange(verbToUse);
            if (meleeReachRange > ShootTuning.MeleeRange)
            {
                CastPositionRequest newReq = default(CastPositionRequest);
                newReq.caster = actor;
                newReq.target = thing;
                newReq.verb = verbToUse;
                newReq.maxRangeFromTarget = meleeReachRange;
                newReq.wantCoverFromTarget = false;
                if (!CastPositionFinder.TryFindCastPosition(newReq, out var dest))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    destination = dest;
                    mode = PathEndMode.OnCell;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ReachabilityImmediate), nameof(ReachabilityImmediate.CanReachImmediate), new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(Map), typeof(PathEndMode), typeof(Pawn) })]
    public static class ReachabilityImmediate_CanReachImmediate_Patch
    {
        public static Pawn curPawn;

        public static void Postfix(ref bool __result, IntVec3 start, LocalTargetInfo target, Map map, PathEndMode peMode, Pawn pawn)
        {
            if (__result is false && curPawn != null)
            {
                var verbToUse = curPawn.GetMeleeVerb();
                var meleeReachRange = curPawn.GetMeleeReachRange(verbToUse);
                var distance = target.Cell.DistanceTo(start);
                __result = distance <= meleeReachRange && GenSight.LineOfSight(start, target.Cell, map);
            }
        }

        public static Verb GetMeleeVerb(this Pawn pawn)
        {
            return pawn.jobs.curJob?.verbToUse ?? pawn.equipment?.PrimaryEq?.PrimaryVerb;
        }
    }

    [HarmonyPatch(typeof(JobDriver_Wait), "CheckForAutoAttack")]
    public static class JobDriver_Wait_CheckForAutoAttack_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var label = generator.DefineLabel();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Brtrue_S && codes[i - 1].Calls(AccessTools.Method(typeof(VerbProperties), "get_IsMeleeAttack")))
                {
                    codes[i + 1].labels.Add(label);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 9);
                    yield return new CodeInstruction(OpCodes.Call, 
                        AccessTools.Method(typeof(JobDriver_Wait_CheckForAutoAttack_Patch), nameof(IsVanillaMeleeAttack)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static bool IsVanillaMeleeAttack(Verb verb)
        {
            if (verb.Caster is Pawn pawn && pawn.GetMeleeReachRange(verb) > ShootTuning.MeleeRange)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AttackTargetFinder), "BestAttackTarget")]
    public static class AttackTargetFinder_BestAttackTarget_Patch
    {
        public static void Prefix(IAttackTargetSearcher searcher, ref float maxDist)
        {
            if (searcher is Pawn pawn)
            {
                var verbToUse = pawn.GetMeleeVerb();
                var meleeReachRange = pawn.GetMeleeReachRange(verbToUse);
                if (meleeReachRange > ShootTuning.MeleeRange)
                {
                    maxDist = Mathf.Max(maxDist, meleeReachRange);
                }
            }
        }
    }
}

