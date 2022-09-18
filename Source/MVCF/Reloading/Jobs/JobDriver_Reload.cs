using System.Collections.Generic;
using MVCF.Reloading.Comps;
using MVCF.Utilities;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

// ReSharper disable once CheckNamespace
namespace Reloading
{
    public class JobDriver_Reload : JobDriver
    {
        private const TargetIndex AMMO_INDEX = TargetIndex.A;
        private Sustainer reloadSound;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(AMMO_INDEX), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var comp = job.verbToUse.Managed().TryGetComp<VerbComp_Reloadable>();

            this.FailOn(() => comp == null);
            this.FailOn(() => !comp.NeedsReload());
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            var getNextIngredient = Toils_General.Label();
            yield return getNextIngredient;
            foreach (var toil in ReloadFromCarried(comp)) yield return toil;
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(AMMO_INDEX);
            yield return Toils_Goto.GotoThing(AMMO_INDEX, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(AMMO_INDEX).FailOnSomeonePhysicallyInteracting(AMMO_INDEX);
            yield return Toils_Haul.StartCarryThing(AMMO_INDEX, false, true);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(AMMO_INDEX).NullOrEmpty());
            foreach (var toil in ReloadFromCarried(comp)) yield return toil;

            yield return new Toil
            {
                initAction = () =>
                {
                    var t = pawn.carryTracker.CarriedThing;
                    if (t is {Destroyed: false})
                        pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var dropped);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        private IEnumerable<Toil> ReloadFromCarried(VerbComp_Reloadable comp)
        {
            var done = Toils_General.Label();
            yield return Toils_Jump.JumpIf(done,
                () => pawn.carryTracker.CarriedThing == null || !comp.CanReloadFrom(pawn.carryTracker.CarriedThing));
            var reloadTicks = 0;
            var toil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
                initAction = () =>
                {
                    reloadTicks = comp.ReloadTicks(pawn.carryTracker.CarriedThing);
                    reloadSound = null;
                },
                tickAction = () =>
                {
                    if (debugTicksSpentThisToil >= reloadTicks)
                    {
                        comp.Reload(pawn.carryTracker.CarriedThing)?.Destroy();
                        JumpToToil(done);
                    }

                    comp.ReloadEffect(debugTicksSpentThisToil, reloadTicks);

                    reloadSound?.Maintain();
                }
            };
            toil.WithProgressBar(TargetIndex.A, () => debugTicksSpentThisToil / (float) reloadTicks);
            yield return toil;
            yield return done;
            yield return Toils_General.Do(() => reloadSound?.End());
        }
    }
}