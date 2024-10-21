using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    internal class JobDriver_BringToProcessor : JobDriver
    {
        protected Thing Building => job.GetTarget(TargetIndex.A).Thing;
        protected CompAdvancedResourceProcessor Comp => CachedCompAdvancedProcessor.GetFor(Building);

        protected Thing Ingredient => job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(Building, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(Ingredient, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);

            AddEndCondition(() => Comp.Process != null && Comp.Process.ShouldDoNow() ? JobCondition.Ongoing : JobCondition.Incompletable);
            AddEndCondition(() => Comp.Process != null && (Comp.Process.GetOwnerFor(Ingredient.def)?.Require==true || Comp.Process.GetOwnerForCategory(Ingredient.def.thingCategories)?.Require == true) 
            ? JobCondition.Ongoing : JobCondition.Succeeded);
            yield return Toils_General.DoAtomic(delegate
            {
                var owner = Comp.Process.GetOwnerFor(Ingredient.def);
                if(owner == null)
                {
                    owner = Comp.Process.GetOwnerForCategory(Ingredient.def.thingCategories);
                }
                job.count = owner.Required;
                owner.BeingFilled = true;
            });

            var reserveIngredient = Toils_Reserve.Reserve(TargetIndex.B);
            yield return reserveIngredient;

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);

            yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B);

            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveIngredient, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            yield return Toils_General.Wait(Comp.Props.ticksToFill)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
                .WithProgressBarToilDelay(TargetIndex.A);

            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                CachedAdvancedProcessorsManager.GetFor(Map).AddIngredient(Comp, Ingredient);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }
    }
}