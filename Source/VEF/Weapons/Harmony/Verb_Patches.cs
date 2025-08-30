using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using VEF.Hediffs;
using Verse.AI;
using System.Reflection.Emit;
using System.Reflection;
using VEF.Apparels;

namespace VEF.Weapons
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

      

    }

    [HarmonyPatch(typeof(Verb), nameof(Verb.Available))]
    public static class VanillaExpandedFramework_Verb_Available_Patch
    {
        public static void Postfix(Verb __instance, ref bool __result)
        {
            // Unusable shield verbs don't get counted
            if (__result && __instance.EquipmentSource != null && __instance.EquipmentSource.IsShield(out Apparels.CompShield shieldComp))
                __result = shieldComp.UsableNow;
        }
    }

    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new Type[]
    {
            typeof(Verb), typeof(Pawn)
    })]
    public static class VanillaExpandedFramework_VerbProperties_AdjustedCooldown_Patch
    {
        public static void Postfix(ref float __result, Verb ownerVerb, Pawn attacker)
        {
            var pawn = ownerVerb.CasterPawn;
            if (pawn != null)
            {
                __result *= pawn.GetStatValue(VEFDefOf.VEF_VerbCooldownFactor);
            }
        }
    }

    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.HitReportFor))]
    public static class VanillaExpandedFramework_ShotReport_HitReportFor_Patch
    {
        public static Thing curCaster;
        public static void Prefix(Thing caster, Verb verb, LocalTargetInfo target)
        {
            curCaster = caster;
            VerbAccuracyUtility.CheckAccuracyEffects(verb, target, out VerbAccuracyUtility.forceHit, out VerbAccuracyUtility.forceMiss);
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class VanillaExpandedFramework_Verb_LaunchProjectile_TryCastShot
    {
        public static void Prefix(Verb_LaunchProjectile __instance)
        {
            VerbAccuracyUtility.CheckAccuracyEffects(__instance, __instance.CurrentTarget, out VerbAccuracyUtility.forceHit, out VerbAccuracyUtility.forceMiss);
        }
        public static void Finalizer()
        {
            VerbAccuracyUtility.forceHit = false;
            VerbAccuracyUtility.forceMiss = false;
        }
    }

    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.AimOnTargetChance_StandardTarget), MethodType.Getter)]
    public static class VanillaExpandedFramework_ShotReport_AimOnTargetChance_StandardTarget
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

    [HarmonyPatch(typeof(CastPositionFinder), nameof(CastPositionFinder.TryFindCastPosition))]
    public static class VanillaExpandedFramework_CastPositionFinder_TryFindCastPosition_Patch
    {
        public static void Prefix(ref CastPositionRequest newReq)
        {
            var weapon = newReq.verb?.EquipmentSource?.def;
            if (weapon != null && weapon.StatBaseDefined(VEFDefOf.VEF_MeleeWeaponRange))
            {
                newReq.maxRangeFromTarget = Mathf.Max(newReq.maxRangeFromTarget, weapon.GetStatValueAbstract(VEFDefOf.VEF_MeleeWeaponRange));
            }
        }
    }

    [HarmonyPatch(typeof(Verb), nameof(Verb.TryFindShootLineFromTo))]
    public static class VanillaExpandedFramework_Verb_TryFindShootLineFromTo_Patch
    {
        public static Pawn curPawn;
        public static void Prefix(Verb __instance)
        {
            curPawn = __instance.CasterPawn;
        }

        public static void Finalizer()
        {
            curPawn = null;
        }
    }

    [HarmonyPatch]
    public static class VanillaExpandedFramework_Toils_Combat_GotoCastPosition_Patch
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
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Job), nameof(Job.verbToUse)));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MeleeReachCombatUtility), nameof(MeleeReachCombatUtility.GetMeleeReachRange)));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }

    [HarmonyPatch(typeof(AttackTargetFinder), "FindBestReachableMeleeTarget")]
    public static class VanillaExpandedFramework_AttackTargetFinder_FindBestReachableMeleeTarget_Patch
    {
        public static Pawn curPawn;
        public static void Prefix(Pawn searcherPawn)
        {
            curPawn = searcherPawn;
        }

        public static void Finalizer()
        {
            curPawn = null;
        }
    }

    [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob")]
    public static class VanillaExpandedFramework_JobGiver_ConfigurableHostilityResponse_TryGetAttackNearbyEnemyJob_Patch
    {
        public static Pawn curPawn;

        public static void Prefix(Pawn pawn)
        {
            curPawn = pawn;
        }

        public static void Finalizer()
        {
            curPawn = null;
        }
    }

    [HotSwappable]
    [HarmonyPatch]
    public static class VanillaExpandedFramework_Toils_Combat_FollowAndMeleeAttack_Patch
    {
        public static FieldInfo targetInd;
        public static Pawn curPawn;
    
        public static void Prefix(Toil ___followAndAttack)
        {
            curPawn = ___followAndAttack.actor;
        }
    
        public static void Finalizer()
        {
            curPawn = null;
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
                        if (targetInd != null)
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
                    && localBuilder.LocalIndex == 8)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, targetInd);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 7);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 8);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(VanillaExpandedFramework_Toils_Combat_FollowAndMeleeAttack_Patch), nameof(TryOverrideDestinationAndPathMode)));
                }
            }
        }
    
        public static void TryOverrideDestinationAndPathMode(TargetIndex targetInd, Pawn actor,
            ref LocalTargetInfo destination, ref PathEndMode mode)
        {
            curPawn = actor;
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
                var oldRange = verbToUse.verbProps.range;
                verbToUse.verbProps.range = meleeReachRange;
                if (!CastPositionFinder.TryFindCastPosition(newReq, out var dest))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    destination = dest;
                    mode = PathEndMode.OnCell;
                }
                verbToUse.verbProps.range = oldRange;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower), "AtDestinationPosition")]
    public static class VanillaExpandedFramework_Pawn_PathFollower_AtDestinationPosition_Patch
    {
        public static Pawn curPawn;
        public static void Prefix(Pawn_PathFollower __instance, Pawn ___pawn)
        {
            curPawn = ___pawn;
        }
        public static void Finalizer(Pawn_PathFollower __instance)
        {
            curPawn = null;
        }
    }
    
    [HotSwappable]
    [HarmonyPatch(typeof(ReachabilityImmediate), nameof(ReachabilityImmediate.CanReachImmediate), new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(Map), typeof(PathEndMode), typeof(Pawn) })]
    public static class VanillaExpandedFramework_ReachabilityImmediate_CanReachImmediate_Patch
    {
    
        public static void Postfix(ref bool __result, IntVec3 start, LocalTargetInfo target, Map map, PathEndMode peMode, Pawn pawn)
        {
            var curPawn = VanillaExpandedFramework_Toils_Combat_FollowAndMeleeAttack_Patch.curPawn
                ?? VanillaExpandedFramework_JobGiver_ConfigurableHostilityResponse_TryGetAttackNearbyEnemyJob_Patch.curPawn
                ?? VanillaExpandedFramework_AttackTargetFinder_FindBestReachableMeleeTarget_Patch.curPawn
                ?? VanillaExpandedFramework_Verb_TryFindShootLineFromTo_Patch.curPawn;
            if (VanillaExpandedFramework_Pawn_PathFollower_AtDestinationPosition_Patch.curPawn == curPawn)
            {
                return;
            }
            if (__result is false && curPawn != null)
            {
                var verbToUse = curPawn.GetMeleeVerb();
                var meleeReachRange = curPawn.GetMeleeReachRange(verbToUse);
                var distance = target.Cell.DistanceTo(start);
                __result = distance <= meleeReachRange && GenSight.LineOfSight(start, target.Cell, map);
            }
        }
    }
    
    [HarmonyPatch(typeof(JobDriver_Wait), "CheckForAutoAttack")]
    public static class VanillaExpandedFramework_JobDriver_Wait_CheckForAutoAttack_Patch
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
                        AccessTools.Method(typeof(MeleeReachCombatUtility), nameof(MeleeReachCombatUtility.IsVanillaMeleeAttack)));
                    yield return new CodeInstruction(OpCodes.Brtrue_S, codes[i].operand);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget))]
    public static class VanillaExpandedFramework_AttackTargetFinder_BestAttackTarget_Patch
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

    public static class MeleeReachCombatUtility
    {
        public static float GetMeleeReachRange(this Pawn caster, Verb verb)
        {
            var weapon = verb?.EquipmentSource?.def;
            if (weapon != null && weapon.StatBaseDefined(VEFDefOf.VEF_MeleeWeaponRange))
            {
                return weapon.GetStatValueAbstract(VEFDefOf.VEF_MeleeWeaponRange);
            }
            return ShootTuning.MeleeRange;
        }


        public static Verb GetMeleeVerb(this Pawn pawn)
        {
            return pawn.jobs.curJob?.verbToUse ?? pawn.equipment?.PrimaryEq?.PrimaryVerb;
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
}

