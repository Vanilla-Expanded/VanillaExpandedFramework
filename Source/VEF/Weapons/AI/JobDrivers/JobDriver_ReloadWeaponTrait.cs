
using RimWorld;
using RimWorld.Planet;
using RimWorld.Utility;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VEF.Weapons
{
    public class JobDriver_ReloadWeaponTrait : JobDriver
    {
        private const TargetIndex GearInd = TargetIndex.A;

        private const TargetIndex AmmoInd = TargetIndex.B;

        private Thing Gear => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompApplyWeaponTraits reloadableComp = Gear?.TryGetComp<CompApplyWeaponTraits>();

            this.FailOn(() => reloadableComp == null);
           
            this.FailOn(() => !reloadableComp.NeedsReload());
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            Toil getNextIngredient = Toils_General.Label();
            yield return getNextIngredient;
            foreach (Toil item in ReloadAsMuchAsPossible(reloadableComp))
            {
                yield return item;
            }
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
            foreach (Toil item2 in ReloadAsMuchAsPossible(reloadableComp))
            {
                yield return item2;
            }
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                if (carriedThing != null && !carriedThing.Destroyed)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing _);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }

        private IEnumerable<Toil> ReloadAsMuchAsPossible(CompApplyWeaponTraits reloadable)
        {
            Toil done = Toils_General.Label();
            yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null || pawn.carryTracker.CarriedThing.stackCount < reloadable.MinAmmoNeeded());
            yield return Toils_General.Wait(reloadable.AbilityDetailsForWeapon(reloadable.GetDetails()).baseReloadTicks).WithProgressBarToilDelay(TargetIndex.A);
            Toil toil = ToilMaker.MakeToil("ReloadAsMuchAsPossible");
            toil.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                reloadable.ReloadFrom(pawn,carriedThing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield return done;
        }
    }
}