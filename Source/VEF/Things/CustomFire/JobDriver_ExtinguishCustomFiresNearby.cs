using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VEF.Things;

public class JobDriver_ExtinguishCustomFiresNearby : JobDriver
{
    protected int ticksSpentExtinguishing;

    public const TargetIndex FireIndex = TargetIndex.A;

    protected CustomFire TargetFire => (CustomFire)job.targetA.Thing;

    public override string GetReport()
    {
        if (TargetFire?.Def.translationKeyReportExtinguishFiresNearby != null)
            return TargetFire.Def.translationKeyReportExtinguishFiresNearby.Translate();
        return base.GetReport();
    }

    protected virtual float MaxDurationTicks => 600f.SecondsToTicks();

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        AddEndCondition(() => ticksSpentExtinguishing < MaxDurationTicks ? JobCondition.Ongoing : JobCondition.Succeeded);

        var initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(FireIndex, t => !t.Destroyed);
        var findClosestFire = ToilMaker.MakeToil();
        var beat = ToilMaker.MakeToil();
        var approach = ToilMaker.MakeToil();

        findClosestFire.tickIntervalAction = _ =>
        {
            var target = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.Fire), PathEndMode.Touch, TraverseParms.For(pawn), validator: f => !job.GetTargetQueue(FireIndex).Contains(f) && pawn.CanReserve(f), searchRegionsMax: 13);

            if (target != null)
            {
                job.AddQueuedTarget(FireIndex, target);
                job.GetTargetQueue(FireIndex).RemoveWhere(f => f.Thing == null || f.ThingDestroyed);
                job.GetTargetQueue(FireIndex).Sort((f1, f2) =>
                {
                    var distSquared1 = pawn.Position.DistanceToSquared(f1.Thing.Position);
                    var distSquared2 = pawn.Position.DistanceToSquared(f2.Thing.Position);

                    if (distSquared1 == distSquared2)
                        return 0;
                    if (distSquared1 < distSquared2)
                        return -1;
                    return distSquared1 > distSquared2 ? 1 : 0;
                });
            }

            ReadyForNextToil();
        };
        findClosestFire.defaultCompleteMode = ToilCompleteMode.Never;

        approach.initAction = () =>
        {
            if (Map.reservationManager.CanReserve(pawn, TargetFire))
                pawn.Reserve(TargetFire, job);
            pawn.pather.StartPath(TargetFire, PathEndMode.Touch);
        };
        approach.tickIntervalAction = _ =>
        {
            if (pawn.pather.Moving && pawn.pather.nextCell != TargetFire.Position)
                StartBeatingFireIfAnyAt(pawn.pather.nextCell, beat);
            if (pawn.Position != TargetFire.Position)
                StartBeatingFireIfAnyAt(pawn.Position, beat);
        };
        approach.JumpIfDespawnedOrNullOrForbidden(FireIndex, findClosestFire);
        approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        approach.atomicWithPrevious = true;

        beat.tickAction = () =>
        {
            ticksSpentExtinguishing++;

            if (!pawn.CanReachImmediate(TargetFire, PathEndMode.Touch))
            {
                JumpToToil(approach);
            }
            else
            {
                if (pawn.Position != TargetFire.Position && StartBeatingFireIfAnyAt(pawn.Position, beat))
                    return;
                TargetFire.TryBeatFire(pawn);
                if (!TargetFire.Destroyed)
                    return;
                pawn.records.Increment(RecordDefOf.FiresExtinguished);
                ReadyForNextToil();
            }
        };
        beat.JumpIfDespawnedOrNullOrForbidden(FireIndex, findClosestFire);
        beat.defaultCompleteMode = ToilCompleteMode.Never;

        yield return initExtractTargetFromQueue;
        yield return Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(FireIndex, thing => !thing.Destroyed);
        yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(FireIndex);
        yield return Toils_JobTransforms.ExtractNextTargetFromQueue(FireIndex);
        yield return approach;
        yield return beat;
        yield return findClosestFire;
        yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(FireIndex);
        yield return Toils_Jump.Jump(initExtractTargetFromQueue);
    }

    private bool StartBeatingFireIfAnyAt(IntVec3 cell, Toil nextToil)
    {
        var thingList = cell.GetThingList(Map);
        for (var i = 0; i < thingList.Count; i++)
        {
            if (thingList[i] is CustomFire { parent: null } fire)
            {
                job.targetA = fire;
                pawn.pather.StopDead();
                JumpToToil(nextToil);
                return true;
            }
        }

        return false;
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref ticksSpentExtinguishing, nameof(ticksSpentExtinguishing));
    }
}