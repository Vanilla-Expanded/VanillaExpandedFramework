using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;


namespace VEF.Buildings
{
    public class JobDriver_Loot : JobDriver
    {

        public int totalTimeCached = -1;
        public int TotalTime
        {
            get {
                if (totalTimeCached == -1)
                {
                    float hackingModifier = 1;
                    if (Building.LootableExtension.useHackingSpeed) {
                        hackingModifier *= pawn.GetStatValue(StatDefOf.HackingSpeed);
                    }
                    totalTimeCached = (int)(GenTicks.TicksPerRealSecond * Building.LootableExtension.secondsToOpen / hackingModifier);
                }
                return totalTimeCached; 
            
            }
        
        
        }
      
        public int totalTimer = 0;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A).Thing, this.job, 1, -1, null, true);
        }
        private LootableBuilding_Custom Building => (LootableBuilding_Custom)job.GetTarget(TargetIndex.A).Thing;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Thing building = this.job.GetTarget(TargetIndex.A).Thing;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            if (TargetA.Thing.def.hasInteractionCell)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            }
            else
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            }

            Toil study = ToilMaker.MakeToil();

            study.tickIntervalAction = delta =>
            {
                Pawn actor = study.actor;


                actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));

                totalTimer += delta;
                if (totalTimer > TotalTime)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };

            study.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            study.WithProgressBar(TargetIndex.A, () => (float)totalTimer / TotalTime);
            study.defaultCompleteMode = ToilCompleteMode.Never;

            study.handlingFacing = true;
            study.AddFinishAction(delegate
            {
                Building.Open();
            });
            yield return study;



        }
    }
}
