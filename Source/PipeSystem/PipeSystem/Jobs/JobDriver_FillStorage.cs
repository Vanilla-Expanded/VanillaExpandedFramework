using System.Collections.Generic;
using UnityEngine;
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
            this.FailOn(() => compRS?.Props.refillOptions == null);
            this.FailOn(() => !compRS.markedForRefill);
            var props = compRS.Props;

            yield return Toils_Reserve.Reserve(TargetIndex.B, stackCount: (int)compRS.AmountCanAccept);

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);

            yield return Toils_Haul.StartCarryThing(TargetIndex.B, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            var refillTime = compRS.Props.refillOptions.refillTime;
            if (compRS.Props.refillOptions.refillTimeScalesWithAmount)
                refillTime *= job.targetB.Thing.stackCount;
            yield return Toils_General.Wait(Mathf.Max(Mathf.RoundToInt(refillTime), 1))
                .FailOnDestroyedNullOrForbidden(TargetIndex.B)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);

            Toil finalize = ToilMaker.MakeToil();
            finalize.initAction = delegate
            {
                var fuel = job.targetB.Thing;
                var amountAdjusted = fuel.stackCount * props.refillOptions.ratio;

                var max = compRS.AmountCanAccept;

                if (amountAdjusted > max)
                {
                    compRS.AddResource(max);
                    fuel.SplitOff(Mathf.Max(Mathf.FloorToInt(max / props.refillOptions.ratio), 1)).Destroy();
                }
                else
                {
                    compRS.AddResource(amountAdjusted);
                    fuel.Destroy();
                }
            };
            finalize.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finalize;
        }
    }
}