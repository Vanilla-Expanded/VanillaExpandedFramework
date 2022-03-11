using System.Collections.Generic;
using MVCF.Reloading.Comps;
using MVCF.Utilities;
using RimWorld;
using Verse.AI;

// ReSharper disable once CheckNamespace
namespace Reloading
{
    public class JobDriver_ReloadFromInventory : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var comp = job.verbToUse.Managed().TryGetComp<VerbComp_Reloadable>();

            this.FailOn(() => comp == null);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            var reloadTicks = 0;
            var done = Toils_General.Label();

            yield return Toils_Jump.JumpIf(done, () => comp == null || pawn.carryTracker.CarriedThing != null || !comp.NeedsReload());
            var toil = new Toil
            {
                initAction = () =>
                {
                    pawn.pather.StopDead();
                    var item = job.targetA.Thing;
                    pawn.inventory.innerContainer.TryTransferToContainer(item, pawn.carryTracker.innerContainer, job.count);
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