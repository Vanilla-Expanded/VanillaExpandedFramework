using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace VEF.AnimalBehaviours
{
    public class JobDriver_IngestWeird : JobDriver, IEatingDriver
    {
        private Toil chewing;

        public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

        public const TargetIndex IngestibleSourceInd = TargetIndex.A;

        private const TargetIndex TableCellInd = TargetIndex.B;

        private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

        private Thing IngestibleSource => this.job.GetTarget(IngestibleSourceInd).Thing;

        private float ChewDurationMultiplier => 1f;

        public bool GainingNutritionNow => !IngestibleSource.DestroyedOrNull() && CurToil == chewing;

        public override void ExposeData()
        {
            base.ExposeData();

        }

        public override string GetReport()
        {

            return base.GetReport();
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();

        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (this.pawn.Faction != null)
            {
                Thing ingestibleSource = this.IngestibleSource;
                if (!this.pawn.Reserve(ingestibleSource, this.job, 10, 1, null, errorOnFailed))
                {
                    return false;
                }
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompEatWeirdFood comp = pawn.TryGetComp<CompEatWeirdFood>();
            if (comp == null)
            { yield break; }

            this.FailOn(() => this.IngestibleSource.Destroyed);

            chewing = ChewAnything(this.pawn, 1f, IngestibleSourceInd, TableCellInd).FailOn((Toil x) => !this.IngestibleSource.Spawned && (this.pawn.carryTracker == null || this.pawn.carryTracker.CarriedThing != this.IngestibleSource)).FailOnCannotTouch(IngestibleSourceInd, PathEndMode.Touch);
            foreach (Toil toil in this.PrepareToIngestToils(chewing))
            {
                yield return toil;
            }

            yield return chewing;
            yield return FinalizeIngestAnything(this.pawn, IngestibleSourceInd, comp);
            yield return Toils_Jump.JumpIf(chewing, () => this.job.GetTarget(IngestibleSourceInd).Thing is Corpse && this.pawn.needs?.food?.CurLevelPercentage < EatCorpseBodyPartsUntilFoodLevelPct);

        }

        private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
        {

            return this.PrepareToIngestToils_NonToolUser();
        }



        private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
        {
            yield return this.ReserveFood();
            yield return Toils_Goto.GotoThing(IngestibleSourceInd, PathEndMode.Touch);
        }

        private Toil ReserveFood()
        {
            var toil = ToilMaker.MakeToil();

            toil.initAction = delegate
            {
                    
                        
                Thing thing = this.job.GetTarget(IngestibleSourceInd).Thing;
                                               
                if (!this.pawn.Reserve(thing, this.job, 10, 1))
                {
                    Log.Error(string.Concat(new object[]
                    {
                        "Pawn food reservation for ",
                        this.pawn,
                        " on job ",
                        this,
                        " failed, because it could not register food from ",
                        thing,
                        " - amount: ",
                        1
                    }));
                    this.pawn.jobs.EndCurrentJob(JobCondition.Errored);
                }
                this.job.count = 1;
                    
                    
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.atomicWithPrevious = true;

            return toil;
        }

        public static Toil ChewAnything(Pawn chewer, float durationMultiplier, TargetIndex ingestibleInd, TargetIndex eatSurfaceInd = TargetIndex.None)
        {
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Thing thing = actor.CurJob.GetTarget(ingestibleInd).Thing;

                // toil.actor.pather.StopDead(); ??
                actor.jobs.curDriver.ticksLeftThisToil = Mathf.RoundToInt(500 * durationMultiplier);
                if (thing.Spawned)
                {
                    thing.Map.physicalInteractionReservationManager.Reserve(chewer, actor.CurJob, thing);
                }
            };
            toil.tickIntervalAction = delegate(int delta)
            {
                if (chewer != toil.actor)
                {
                    toil.actor.rotationTracker.FaceCell(chewer.Position);
                }
                else
                {
                    Thing thing = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
                    if (thing != null && thing.Spawned)
                    {
                        toil.actor.rotationTracker.FaceCell(thing.Position);
                    }
                    else if (eatSurfaceInd != TargetIndex.None && toil.actor.CurJob.GetTarget(eatSurfaceInd).IsValid)
                    {
                        toil.actor.rotationTracker.FaceCell(toil.actor.CurJob.GetTarget(eatSurfaceInd).Cell);
                    }
                }
                toil.actor.GainComfortFromCellIfPossible(delta);
            };
            toil.WithProgressBar(ingestibleInd, delegate
            {
                Thing thing = toil.actor.CurJob.GetTarget(ingestibleInd).Thing;
                if (thing == null)
                {
                    return 1f;
                }
                return 1f - toil.actor.jobs.curDriver.ticksLeftThisToil / Mathf.Round(500 * durationMultiplier);
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(ingestibleInd);
            toil.AddFinishAction(delegate
            {
                if (chewer == null)
                {
                    return;
                }
                if (chewer.CurJob == null)
                {
                    return;
                }
                Thing thing = chewer.CurJob.GetTarget(ingestibleInd).Thing;
                if (thing == null)
                {
                    return;
                }
                if (chewer.Map.physicalInteractionReservationManager.IsReservedBy(chewer, thing))
                {
                    chewer.Map.physicalInteractionReservationManager.Release(chewer, toil.actor.CurJob, thing);
                }
            });



            toil.handlingFacing = true;
            //Toils_Ingest.AddIngestionEffects(toil, chewer, ingestibleInd, eatSurfaceInd);
            return toil;
        }

        public static Toil FinalizeIngestAnything(Pawn ingester, TargetIndex ingestibleInd, CompEatWeirdFood comp)
        {
            Toil toil = ToilMaker.MakeToil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(ingestibleInd).Thing;

                float nutrition = comp.Props.nutrition;

                if (comp.Props.areFoodSourcesPlants)
                {
                    if (thing is Plant plant)
                    {
                        nutrition *= plant.Growth;
                    }
                }

                if (comp.Props.fullyDestroyThing)
                {
                    if (comp.Props.drainBattery)
                    {
                        if (thing is Building_Battery battery)
                        {
                            CompPowerBattery compPower = battery.TryGetComp<CompPowerBattery>();
                            compPower.SetStoredEnergyPct(compPower.StoredEnergyPct - comp.Props.percentageDrain);
                        }
                        else thing.Destroy();

                    }
                    else thing.Destroy();

                }
                else
                {
                    if (thing.def.useHitPoints && !comp.Props.ignoreUseHitPoints)
                    {
                        thing.HitPoints -= (int)(thing.MaxHitPoints * comp.Props.percentageOfDestruction);
                        if (thing.HitPoints <= 0)
                        {
                            thing.Destroy();
                        }
                    }
                    else
                    {
                        int thingsToDestroy = (int)(comp.Props.percentageOfDestruction * thing.def.stackLimit);
                        //Log.Message(thingsToDestroy.ToString());

                        thing.stackCount -= thingsToDestroy;
                        //Log.Message(thing.stackCount.ToString());
                        if (thing.stackCount < 10)
                        {
                            thing.Destroy();
                        }

                    }
                    if ((actor.def.defName == "AA_AngelMoth") && (actor.Faction == Faction.OfPlayer) && (thing.TryGetComp<CompQuality>() != null) && (thing.TryGetComp<CompQuality>().Quality == QualityCategory.Legendary))
                    {
                        actor.health.AddHediff(HediffDef.Named("AA_AteFinestClothes"));
                    }

                }

                if (comp.Props.hediffWhenEaten != "")
                {
                    actor.health.AddHediff(HediffDef.Named(comp.Props.hediffWhenEaten));
                }

                if (comp.Props.advanceLifeStage && actor.Map != null)
                {
                   
                    comp.currentFeedings++;
                    if (comp.currentFeedings >= comp.Props.advanceAfterXFeedings && (!ModsConfig.OdysseyActive || !actor.training.HasLearned(InternalDefOf.VEF_CycleSeverance)))
                    {
                        if (comp.Props.fissionAfterXFeedings)
                        {
                            if (!comp.Props.fissionOnlyIfTamed || (actor.Faction!=null && actor.Faction.IsPlayer))
                            {
                                for (int i = 0; i < comp.Props.numberOfOffspring; i++)
                                {
                                    PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(comp.Props.defToFissionTo), actor.Faction, PawnGenerationContext.NonPlayer, -1,true, true, false, false, true, 1f, false, false, true, true, false);
                                    Pawn newPawn = PawnGenerator.GeneratePawn(request);
                                    newPawn.playerSettings.AreaRestrictionInPawnCurrentMap = actor.playerSettings.AreaRestrictionInPawnCurrentMap;
                                    newPawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, actor);
                                    GenSpawn.Spawn(newPawn, actor.Position, actor.Map);

                                }

                                actor.Destroy();
                            }


                        }
                        else
                        {
                            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(comp.Props.defToAdvanceTo), actor.Faction, PawnGenerationContext.NonPlayer,-1, true, true, false, false, true, 1f, false, false, true, true, false);
                            Pawn newPawn = PawnGenerator.GeneratePawn(request);
                            if (actor.Name!=null && !actor.Name.ToString().UncapitalizeFirst().Contains(actor.def.label))
                            {
                                newPawn.Name = actor.Name;
                            }
                            if (actor.training != null) {
                                newPawn.training = actor.training;
                            }
                            if (actor.health != null)
                            {
                                newPawn.health = actor.health;
                            }
                            if (actor.foodRestriction != null)
                            {
                                newPawn.foodRestriction = actor.foodRestriction;
                            }
                            if (actor.playerSettings!=null && actor.playerSettings.AreaRestrictionInPawnCurrentMap != null)
                            {
                                newPawn.playerSettings.AreaRestrictionInPawnCurrentMap = actor.playerSettings.AreaRestrictionInPawnCurrentMap;
                            }

                           
                            GenSpawn.Spawn(newPawn, actor.Position, actor.Map);
                            actor.Destroy();
                        }

                    }
                }

                if (!ingester.Dead)
                {
                    ingester.needs.food.CurLevel += nutrition;
                }
                ingester.records.AddTo(RecordDefOf.NutritionEaten, nutrition);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}

