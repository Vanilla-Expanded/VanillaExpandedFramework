using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace ItemProcessor
{
    public class JobDriver_RemoveProduct : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        public override void Notify_PatherFailed()
        {

            Building_ItemProcessor building_processor = (Building_ItemProcessor)this.job.GetTarget(TargetIndex.A).Thing;

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
                this.job.count = 1;
            });

            yield return Toils_Goto.GotoThing(TargetIndex.A, building_processor.GetComp<CompItemProcessor>()?.Props.mustLoadFromInteractionSpot ?? false ? PathEndMode.InteractionCell : PathEndMode.Touch);
            yield return Toils_General.Wait(240).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, building_processor.GetComp<CompItemProcessor>()?.Props.mustLoadFromInteractionSpot ?? false ? PathEndMode.InteractionCell : PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing newProduct;
                    if (building_processor.productsToTurnInto != null && building_processor.productsToTurnInto.Count > 0)
                    {
                        newProduct = ThingMaker.MakeThing(ThingDef.Named(building_processor.productsToTurnInto[(int)building_processor.qualityNow]));
                    }
                    else
                    {
                        newProduct = ThingMaker.MakeThing(ThingDef.Named(building_processor.productToTurnInto));
                    }

                    newProduct.stackCount = building_processor.amount;

                    if ((newProduct.TryGetComp<CompIngredients>() is CompIngredients ingredientComp) && !building_processor.compItemProcessor.Props.ignoresIngredientLists)
                    {
                        ingredientComp.ingredients = building_processor.ingredients;
                    }
                    if (building_processor.usingQualityIncreasing && newProduct.TryGetComp<CompQuality>() is CompQuality qualityComp)
                    {
                        qualityComp.SetQuality(building_processor.qualityNow, ArtGenerationContext.Colony);
                    }

                    GenSpawn.Spawn(newProduct, building_processor.InteractionCell, building_processor.Map);
                    building_processor.processorStage = ProcessorStage.ProductRemoved;
                    building_processor.ResetEverything();
                    building_processor.DestroyIngredients();

                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(newProduct);
                    IntVec3 c;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(newProduct, this.pawn, this.Map, currentPriority, this.pawn.Faction, out c, true))
                    {
                        this.job.SetTarget(TargetIndex.C, c);
                        this.job.SetTarget(TargetIndex.B, newProduct);
                        this.job.count = newProduct.stackCount;

                    }
                    else
                    {
                        this.EndJobWith(JobCondition.Incompletable);


                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);






        }
    }
}