using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VEF.Things;

public class JobDriver_BeatCustomFire : JobDriver
{
    public const TargetIndex FireIndex = TargetIndex.A;
    protected CustomFire TargetFire => (CustomFire)job.targetA.Thing;

    public override string GetReport()
    {
        if (TargetFire?.Def.translationKeyReportBeatFire != null)
            return TargetFire.Def.translationKeyReportBeatFire.Translate();
        return base.GetReport();
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(FireIndex);

        var approach = ToilMaker.MakeToil();
        var beat = ToilMaker.MakeToil();
        
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
        approach.FailOnDespawnedOrNull(FireIndex);
        approach.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        approach.atomicWithPrevious = true;

        beat.tickAction = () =>
        {
            if (!pawn.CanReachImmediate(TargetFire, PathEndMode.Touch))
            {
                JumpToToil(approach);
                return;
            }

            if (pawn.Position != TargetFire.Position && StartBeatingFireIfAnyAt(pawn.Position, beat))
                return;

            TargetFire.TryBeatFire(pawn);
            if (!TargetFire.Destroyed)
                return;

            pawn.records.Increment(RecordDefOf.FiresExtinguished);
            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        };
        beat.FailOnDespawnedOrNull(FireIndex);
        beat.defaultCompleteMode = ToilCompleteMode.Never;

        yield return approach;
        yield return beat;
    }

    protected virtual bool StartBeatingFireIfAnyAt(IntVec3 cell, Toil nextToil)
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
}