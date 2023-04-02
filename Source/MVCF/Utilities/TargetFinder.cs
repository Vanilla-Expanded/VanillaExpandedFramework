using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MVCF.Utilities;

public static class TargetFinder
{
    public static Verb SearchVerb;


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
        SearchVerb = verb;
        if (verb.IsIncendiary_Ranged()) flags |= TargetScanFlags.NeedNonBurning;
        var target = AttackTargetFinder.BestAttackTarget(searcher, flags, validator, minDist, maxDist, locus, maxTravelRadiusFromLocus, canBashDoors,
            canTakeTargetsCloserThanEffectiveMinRange,
            canBashFences);
        SearchVerb = null;
        return target;
    }

    public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, out Verb verbUsed, TargetScanFlags flags, Predicate<Thing> validator = null,
        float minDist = 0f,
        float maxDist = 9999f, IntVec3 locus = default, float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBashDoors = false,
        bool canTakeTargetsCloserThanEffectiveMinRange = true, bool canBashFences = false, bool setCurrent = true, bool canMove = true)
    {
        MVCF.LogFormat($"Intercepted BestAttackTarget from {searcher} with validator {validator}, and range {minDist}~{maxDist}", LogLevel.Info);
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
                MVCF.LogFormat($"Found target {target} for verb {verb.Verb}");
                if (target is null) continue;
                var score = verb.GetScore(pawn, target.Thing);
                MVCF.LogFormat($"Score is {score}");
                if (score <= bestScore) continue;
                bestScore = score;
                bestTarget = target;
                bestVerb = verb.Verb;
            }

            MVCF.LogFormat($"Final target: {bestTarget} with verb {bestVerb} and score {bestScore}", LogLevel.Info);
            if (bestVerb is not null && bestTarget?.Thing is not null)
            {
                if (setCurrent) man.CurrentVerb = bestVerb;
                verbUsed = bestVerb;
                return bestTarget;
            }
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
}
