using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    public class JobDriver_FillStorage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed) && pawn.Reserve(job.targetB, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            var thing = job.targetA.Thing;
            var compRS = thing.TryGetComp<CompResourceStorage>();
            this.FailOn(() => compRS == null);
            var props = compRS.Props;

            yield return Toils_Reserve.Reserve(TargetIndex.B, stackCount: (int)compRS.AmountCanAccept);

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);

            yield return Toils_Haul.StartCarryThing(TargetIndex.B, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            yield return Toils_General.Wait(props.refillOptions.refillTime)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);

            Toil finalize = new Toil
            {
                initAction = delegate
                {
                    var fuel = job.targetB.Thing;
                    int amountAdjusted = fuel.stackCount * props.refillOptions.ratio;

                    int max = (int)compRS.AmountCanAccept;

                    if (amountAdjusted > max)
                    {
                        compRS.AddResource(max);
                        fuel.SplitOff(max / props.refillOptions.ratio).Destroy();
                    }
                    else
                    {
                        compRS.AddResource(amountAdjusted);
                        fuel.Destroy();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return finalize;
        }
    }
}