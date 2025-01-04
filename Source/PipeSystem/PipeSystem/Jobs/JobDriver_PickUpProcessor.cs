using System.Collections.Generic;
using System.Threading;
using RimWorld;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    public class JobDriver_PickUpProcessor : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            var thing = job.targetA.Thing;
            var comp = thing.TryGetComp<CompAdvancedResourceProcessor>();
            this.FailOn(() => comp == null);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            yield return Toils_General.Wait(comp?.ProcessDef.extractTicks ?? 200)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);

            var toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                CachedAdvancedProcessorsManager.GetFor(Map).PickupDone(comp);
                comp.Process.SpawnOrPushToNet(pawn.Position, out List<Thing> outThings, pawn);
                if (!outThings.NullOrEmpty())
                {
                    
                    var outThing = outThings[0];
                    comp.Process?.HandleIngredientsAndQuality(outThing);
                    
                    var currentPriority = StoreUtility.CurrentStoragePriorityOf(outThing);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(outThing, pawn, Map, currentPriority, pawn.Faction, out var foundCell))
                    {
                        if (foundCell != IntVec3.Invalid)
                        {
                            job.SetTarget(TargetIndex.C, foundCell);
                            job.SetTarget(TargetIndex.B, outThing);
                            job.count = outThing.stackCount;
                        }
                        
                    }
                    else
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                }
                else
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.C);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            var carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, storageMode: true);
        }
    }
}