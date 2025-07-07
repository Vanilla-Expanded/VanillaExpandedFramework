using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.AI;


namespace VEF.AnimalBehaviours
{
    public class JobDriver_AnimalResource : JobDriver_GatherAnimalBodyResources
    {
        protected override float WorkTotal => 1700f;

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompAnimalProduct>();
        }

        public CompAnimalProduct GetSpecificComp(Pawn animal)
        {
            return animal.TryGetComp<CompAnimalProduct>();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil wait = ToilMaker.MakeToil();
            wait.initAction = () =>
            {
                Pawn actor = wait.actor;
                Pawn pawn = (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
                actor.pather.StopDead();
                PawnUtility.ForceWait(pawn, 15000, null, true);
            };
            wait.tickIntervalAction = delta =>
            {
                Pawn actor = wait.actor;
                actor.skills.Learn(SkillDefOf.Animals, 0.13f * delta);
                this.gatherProgress += actor.GetStatValue(StatDefOf.AnimalGatherSpeed) * delta;
                if (this.gatherProgress >= this.WorkTotal)
                {
                    this.GetSpecificComp((Pawn)(Thing)this.job.GetTarget(TargetIndex.A)).InformGathered(this.pawn);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    if (ModLister.HasActiveModWithName("Alpha Animals"))
                    {
                        actor.health.AddHediff(HediffDef.Named("AA_GatheredResource"));
                    }
                }
            };
            wait.AddFinishAction(() =>
            {
                Pawn pawn = (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
                if (pawn != null && pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            });
            wait.FailOnDespawnedOrNull(TargetIndex.A);
            wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            wait.AddEndCondition(() =>
            {
                if (!this.GetComp((Pawn)(Thing)this.job.GetTarget(TargetIndex.A)).ActiveAndFull)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            wait.WithProgressBar(TargetIndex.A, () => this.gatherProgress / this.WorkTotal);
            wait.activeSkill = () => SkillDefOf.Animals;

            yield return wait;
        }

        private float gatherProgress;
    }
}