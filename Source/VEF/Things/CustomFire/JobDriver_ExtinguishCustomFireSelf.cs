using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VEF.Things;

public class JobDriver_ExtinguishCustomFireSelf : JobDriver
{
    public const TargetIndex FireIndex = TargetIndex.A;

    protected CustomFire TargetFire => (CustomFire)job.targetA.Thing;

    public override string GetReport()
    {
        if (TargetFire != null)
        {
            if (TargetFire.parent != null)
                return TargetFire.Def.translationKeyReportExtinguishingFireOn.Translate(TargetFire.parent.LabelCap, TargetFire.parent.Named("TARGET"));
            return TargetFire.Def.translationKeyReportExtinguishingFire.Translate();
        }
        return "ReportExtinguishingFire".Translate();
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var selfExtinguishDuration = TargetFire?.SelfExtinguishDuration(pawn) ?? 150;

        if (selfExtinguishDuration > 0)
        {
            var panicToil = ToilMaker.MakeToil();
            panicToil.defaultCompleteMode = ToilCompleteMode.Delay;
            panicToil.defaultDuration = selfExtinguishDuration;
            yield return panicToil;
        }

        var extinguishToil = ToilMaker.MakeToil();
        extinguishToil.initAction = () =>
        {
            TargetFire!.Destroy();
            pawn.records.Increment(RecordDefOf.FiresExtinguished);
        };
        extinguishToil.FailOnDestroyedOrNull(FireIndex);
        extinguishToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return extinguishToil;
    }
}