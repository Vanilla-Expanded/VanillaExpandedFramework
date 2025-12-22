using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace VEF.Buildings;

public class JobDriver_RefuelSecondary : JobDriver
{
    private const TargetIndex RefuelableInd = TargetIndex.A;
    private const TargetIndex FuelInd = TargetIndex.B;
    public const int RefuelingDuration = 240;

    protected Thing Refuelable => job.GetTarget(TargetIndex.A).Thing;

    protected CompRefuelable_DualFuel RefuelableComp => Refuelable.TryGetComp<CompRefuelable_DualFuel>();

    protected Thing Fuel => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (pawn.Reserve(Refuelable, job, 1, -1, null, errorOnFailed))
        {
            return pawn.Reserve(Fuel, job, 1, -1, null, errorOnFailed);
        }
        return false;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        AddEndCondition(() => (!RefuelableComp.IsSecondaryFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
        AddFailCondition(() => !job.playerForced && !RefuelableComp.ShouldAutoRefuelSecondaryNowIgnoringFuelPct);
        AddFailCondition(() => !RefuelableComp.allowAutoRefuelSecondary && !job.playerForced);

        yield return Toils_General.DoAtomic(delegate
        {
            job.count = RefuelableComp.GetSecondaryFuelCountToFullyRefuel();
        });

        Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
        yield return reserveFuel;
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true)
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(RefuelingDuration)
            .FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
            .WithProgressBarToilDelay(TargetIndex.A);
        yield return FinalizeSecondaryRefueling(TargetIndex.A, TargetIndex.B);
    }

    public static Toil FinalizeSecondaryRefueling(TargetIndex refuelableInd, TargetIndex fuelInd)
    {
        Toil toil = ToilMaker.MakeToil("FinalizeSecondaryRefueling");
        toil.initAction = delegate
        {
            Job curJob = toil.actor.CurJob;
            Thing thing = curJob.GetTarget(refuelableInd).Thing;
            if (toil.actor.CurJob.placedThings.NullOrEmpty())
            {
                thing.TryGetComp<CompRefuelable_DualFuel>().RefuelSecondary(new List<Thing> { curJob.GetTarget(fuelInd).Thing });
            }
            else
            {
                thing.TryGetComp<CompRefuelable_DualFuel>().RefuelSecondary(
                    toil.actor.CurJob.placedThings.Select((ThingCountClass p) => p.thing).ToList());
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }
}
