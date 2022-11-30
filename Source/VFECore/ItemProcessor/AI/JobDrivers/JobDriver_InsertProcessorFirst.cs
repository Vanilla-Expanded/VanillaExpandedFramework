using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace ItemProcessor
{
    public class JobDriver_InsertProcessorFirst : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null) && this.pawn.Reserve(this.job.targetB, this.job, 1, -1, null);
        }

        public override void Notify_PatherFailed()
        {

            Building_ItemProcessor building_processor = (Building_ItemProcessor)this.job.GetTarget(TargetIndex.A).Thing;

            building_processor.processorStage = ProcessorStage.IngredientsChosen;

            this.EndJobWith(JobCondition.ErroredPather);

        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Log.Message("I am inside the job now, with "+pawn.ToString(), false);
            Building_ItemProcessor building_processor = (Building_ItemProcessor)this.job.GetTarget(TargetIndex.A).Thing;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
           
            this.FailOnBurningImmobile(TargetIndex.A);

            yield return Toils_General.DoAtomic(delegate
            {

                if (building_processor.ExpectedAmountFirstIngredient != 0)
                {
                    this.job.count = building_processor.ExpectedAmountFirstIngredient - building_processor.CurrentAmountFirstIngredient;
                }
                else
                {
                    this.job.count = 1;
                };


            });
            Toil reserveIngredient = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return reserveIngredient;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveIngredient, TargetIndex.B, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, building_processor.GetComp<CompItemProcessor>()?.Props.mustLoadFromInteractionSpot ?? false ? PathEndMode.InteractionCell : PathEndMode.Touch);
            yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, building_processor.GetComp<CompItemProcessor>()?.Props.mustLoadFromInteractionSpot ?? false ? PathEndMode.InteractionCell : PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (building_processor.processorStage != ProcessorStage.Inactive)
                    {

                        if (building_processor.compItemProcessor.Props.transfersIngredientLists)
                        {
                            if (this.job.targetB.Thing.TryGetComp<CompIngredients>() is CompIngredients ingredientComp)
                            {
                                building_processor.ingredients.AddRange(ingredientComp.ingredients);
                            }

                        }
                        building_processor.CurrentAmountFirstIngredient += this.job.targetB.Thing.stackCount;
                        if (building_processor.ExpectedAmountFirstIngredient != 0)
                        {
                            if (building_processor.CurrentAmountFirstIngredient >= building_processor.ExpectedAmountFirstIngredient)
                            {
                                building_processor.firstIngredientComplete = true;
                            }
                        }
                        building_processor.TryAcceptFirst(this.job.targetB.Thing, 0, true);
                        building_processor.Notify_StartProcessing();
                        //this.job.targetB.Thing.Destroy();

                    }


                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;





        }
    }
}