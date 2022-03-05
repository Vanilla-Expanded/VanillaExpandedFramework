using System.Collections.Generic;
using MVCF.Utilities;
using RimWorld;
using Verse.AI;

// ReSharper disable once CheckNamespace
namespace Reloading
{
    internal class JobDriver_ReloadFromInventory : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var thing = job.targetA.Thing;
            var comp = thing?.GetReloadableComp();

            this.FailOn(() => comp == null);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            var reloadTicks = 0;
            var done = Toils_General.Label();

            yield return Toils_Jump.JumpIf(done, () => comp == null || pawn.carryTracker.CarriedThing != null ||
                                                       !comp.NeedsReload());
            var toil = new Toil
            {
                initAction = () =>
                {
                    pawn.pather.StopDead();
                    var item = job.targetB.Thing;
                    pawn.inventory.innerContainer.TryTransferToContainer(item, pawn.carryTracker.innerContainer,
                        job.count);
                    reloadTicks = comp.ReloadTicks(pawn.carryTracker.CarriedThing);
                },
                defaultCompleteMode = ToilCompleteMode.Never,
                tickAction = () =>
                {
                    if (debugTicksSpentThisToil >= reloadTicks)
                    {
                        comp.Reload(pawn.carryTracker.CarriedThing)?.Destroy();
                        JumpToToil(done);
                    }
                }
            };
            toil.WithProgressBar(TargetIndex.A, () => debugTicksSpentThisToil / (float) reloadTicks);
            yield return toil;

            yield return done;
        }
    }
}