using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF.Utilities;

public static class TargetFinder
{
    private static Verb searchVerb;

    public static bool CurrentEffectiveVerb_Prefix(ref Verb __result, Pawn __instance)
    {
        if (searchVerb is null)
        {
            if (__instance.stances?.curStance is Stance_Busy { verb: { } verb })
            {
                MVCF.Log($"Giving stance verb {verb} from CurrentEffectiveVerb", LogLevel.Tick);
                __result = verb;
                return false;
            }

            var man = __instance.Manager();
            if (!man.HasVerbs || man.SearchVerb == null || !man.SearchVerb.Available()) return true;
            MVCF.Log($"Giving SearchVerb {man.SearchVerb} from CurrentEffectiveVerb", LogLevel.Tick);
            __result = man.SearchVerb;
            return false;
        }

        MVCF.Log($"Giving searchVerb {searchVerb} from CurrentEffectiveVerb", LogLevel.Tick);
        __result = searchVerb;
        return false;
    }

    public static IEnumerable<CodeInstruction> AttackTargetTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.MethodReplacer(
        AccessTools.Method(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestAttackTarget)),
        AccessTools.Method(typeof(TargetFinder), nameof(BestAttackTarget_Replacement)));

    public static IEnumerable<CodeInstruction> BestTargetTranspiler(IEnumerable<CodeInstruction> instructions) => instructions.MethodReplacer(
        AccessTools.Method(typeof(AttackTargetFinder), nameof(AttackTargetFinder.BestShootTargetFromCurrentPosition)),
        AccessTools.Method(typeof(TargetFinder), nameof(BestShootTargetFromCurrentPosition_Replacement)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAttackTarget BestAttackTarget_Replacement(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null,
        float minDist = 0f,
        float maxDist = 9999f, IntVec3 locus = default, float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBashDoors = false,
        bool canTakeTargetsCloserThanEffectiveMinRange = true, bool canBashFences = false) =>
        BestAttackTarget(searcher, out _, flags, validator, minDist, maxDist, locus, maxTravelRadiusFromLocus, canBashDoors,
            canTakeTargetsCloserThanEffectiveMinRange,
            canBashFences);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, Verb verb, TargetScanFlags flags, Predicate<Thing> validator = null,
        float minDist = 0f,
        float maxDist = 9999f, IntVec3 locus = default, float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBashDoors = false,
        bool canTakeTargetsCloserThanEffectiveMinRange = true, bool canBashFences = false)
    {
        searchVerb = verb;
        if (verb.IsIncendiary_Ranged()) flags |= TargetScanFlags.NeedNonBurning;
        var target = AttackTargetFinder.BestAttackTarget(searcher, flags, validator, minDist, maxDist, locus, maxTravelRadiusFromLocus, canBashDoors,
            canTakeTargetsCloserThanEffectiveMinRange,
            canBashFences);
        searchVerb = null;
        return target;
    }

    public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, out Verb verbUsed, TargetScanFlags flags, Predicate<Thing> validator = null,
        float minDist = 0f,
        float maxDist = 9999f, IntVec3 locus = default, float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBashDoors = false,
        bool canTakeTargetsCloserThanEffectiveMinRange = true, bool canBashFences = false, bool setCurrent = true, bool canMove = true)
    {
        MVCF.Log($"Intercepted BestAttackTarget from {searcher} with validator {validator}, and range {minDist}~{maxDist}", LogLevel.Important);
        if (searcher.Thing is Pawn pawn)
        {
            IAttackTarget bestTarget = null;
            var man = pawn.Manager();
            var bestScore = 0f;
            Verb bestVerb = null;
            foreach (var verb in man.CurrentlyUseableRangedVerbs)
            {
                var maxDistance = maxDist;
                var minDistance = minDist;
                if (!canMove)
                {
                    minDistance = Mathf.Max(minDistance, verb.Verb.verbProps.minRange);
                    maxDistance = Mathf.Min(maxDistance, verb.Verb.verbProps.range);
                }

                var target = BestAttackTarget(searcher, verb.Verb, flags, validator, minDistance, maxDistance, locus, maxTravelRadiusFromLocus, canBashDoors,
                    canMove && canTakeTargetsCloserThanEffectiveMinRange, canBashFences);
                MVCF.Log($"Found target {target} for verb {verb.Verb}");
                if (target is null) continue;
                var score = verb.GetScore(pawn, target.Thing);
                MVCF.Log($"Score is {score}");
                if (score <= bestScore) continue;
                bestScore = score;
                bestTarget = target;
                bestVerb = verb.Verb;
            }

            MVCF.Log($"Final target: {bestTarget} with verb {bestVerb} and score {bestScore}", LogLevel.Important);
            if (bestVerb is not null && bestTarget?.Thing is not null && setCurrent) man.CurrentVerb = bestVerb;
            verbUsed = bestVerb;
            return bestTarget;
        }

        verbUsed = searcher.CurrentEffectiveVerb;
        return AttackTargetFinder.BestAttackTarget(searcher, flags, validator, minDist, maxDist, locus, maxTravelRadiusFromLocus, canBashDoors,
            canTakeTargetsCloserThanEffectiveMinRange,
            canBashFences);
    }

    public static IAttackTarget BestShootTargetFromCurrentPosition_Replacement(IAttackTargetSearcher searcher, TargetScanFlags flags,
        Predicate<Thing> validator = null,
        float minDistance = 0f,
        float maxDistance = 9999f) =>
        BestAttackTarget(searcher, out _, flags, validator, minDistance, maxDistance, canTakeTargetsCloserThanEffectiveMinRange: false, canMove: false);

    public static IEnumerable<Patch> GetPatches()
    {
        yield return Patch.Prefix(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.CurrentEffectiveVerb)),
            AccessTools.Method(typeof(TargetFinder), nameof(CurrentEffectiveVerb_Prefix)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(JobGiver_AIFightEnemy), "FindAttackTarget"),
            AccessTools.Method(typeof(TargetFinder), nameof(AttackTargetTranspiler)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob"),
            AccessTools.Method(typeof(TargetFinder), nameof(AttackTargetTranspiler)));
        yield return Patch.Transpiler(AccessTools.Method(typeof(JobDriver_Wait), "CheckForAutoAttack"),
            AccessTools.Method(typeof(TargetFinder), nameof(BestTargetTranspiler)));
    }
}