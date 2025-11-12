using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace VEF.Buildings
{
    public class JobDriver_StudyBuilding : JobDriver
    {

        public const int totalTime = GenTicks.TicksPerRealSecond * 20; // 20 seconds
        public int totalTimer = 0;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A).Thing, this.job, 1, -1, null, true);
        }
        private StudiableBuilding Building => (StudiableBuilding)job.GetTarget(TargetIndex.A).Thing;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            StudiableBuildingDetails contentDetails = Building.def.GetModExtension<StudiableBuildingDetails>();

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
                if (actor.skills != null && contentDetails.skillForStudying!=null)
                {
                    actor.skills.Learn(contentDetails.skillForStudying, 0.025f * delta);
                }

                actor.rotationTracker.FaceTarget(actor.CurJob.GetTarget(TargetIndex.A));

                totalTimer += delta;
                if (totalTimer > totalTime)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
            };
            if (contentDetails.showProgressBar)
            {
                study.WithProgressBar(TargetIndex.A, () => (float)totalTimer / totalTime);
            }
            study.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            if (contentDetails.showResearchEffecter)
            {
                study.WithEffect(EffecterDefOf.Research, TargetIndex.A);
            }
            
            study.defaultCompleteMode = ToilCompleteMode.Never;
            if(contentDetails.skillForStudying != null)
            {
                study.activeSkill = () => contentDetails.skillForStudying;
            }       
            study.handlingFacing = true;
            study.AddFinishAction(delegate
            {

                Building.Study(pawn);
            });
            yield return study;



        }
    }
}
