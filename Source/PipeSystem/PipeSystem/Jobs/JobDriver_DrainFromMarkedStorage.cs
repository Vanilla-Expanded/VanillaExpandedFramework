using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    public class JobDriver_DrainFromMarkedStorage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => Map.designationManager.DesignationOn(TargetThingA, PSDefOf.PS_Drain) == null);

            var thing = job.targetA.Thing;
            var compRS = thing.TryGetComp<CompResourceStorage>();
            this.FailOn(() => compRS?.Props.extractOptions == null);
            this.FailOn(() => compRS.CurrentManualExtractAmount().itemAmount <= 0);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            var extractTime = compRS.Props.extractOptions.extractTime;
            if (compRS.Props.extractOptions.extractTimeScalesWithAmount)
                extractTime *= compRS.CurrentManualExtractAmount().itemAmount;
            yield return Toils_General.Wait(Mathf.Max(Mathf.RoundToInt(extractTime), 1)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOnDestroyedOrNull(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);

            Toil finalize = ToilMaker.MakeToil();
            finalize.initAction = delegate
            {
                var (extractResourceAmount, extractItemAmount) = compRS.CurrentManualExtractAmount();
                var opt = compRS.Props.extractOptions;

                compRS.DrawResource(extractResourceAmount);

                while (extractItemAmount > 0)
                {
                    var createdThing = ThingMaker.MakeThing(opt.thing);
                    createdThing.stackCount = Mathf.Min(extractItemAmount, createdThing.def.stackLimit);
                    extractItemAmount -= createdThing.stackCount;
                    GenPlace.TryPlaceThing(createdThing, pawn.Position, Map, ThingPlaceMode.Near);
                }

                Map.designationManager.DesignationOn(thing, PSDefOf.PS_Drain)?.Delete();
            };
            finalize.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finalize;
        }
    }
}