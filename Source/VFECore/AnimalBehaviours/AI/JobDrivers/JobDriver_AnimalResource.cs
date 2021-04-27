using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.AI;


namespace AnimalBehaviours
{
    public class JobDriver_AnimalResource : JobDriver_GatherAnimalBodyResources
    {
        protected override float WorkTotal
        {
            get
            {
                return 1700f;
            }
        }

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
            Toil wait = new Toil();
            wait.initAction = delegate ()
            {
                Pawn actor = wait.actor;
                Pawn pawn = (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
                actor.pather.StopDead();
                PawnUtility.ForceWait(pawn, 15000, null, true);
            };
            wait.tickAction = delegate ()
            {
                Pawn actor = wait.actor;
                actor.skills.Learn(SkillDefOf.Animals, 0.13f, false);
                this.gatherProgress += actor.GetStatValue(StatDefOf.AnimalGatherSpeed, true);
                if (this.gatherProgress >= this.WorkTotal)
                {
                    this.GetSpecificComp((Pawn)((Thing)this.job.GetTarget(TargetIndex.A))).InformGathered(this.pawn);
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                    if (ModLister.HasActiveModWithName("Alpha Animals"))
                    {
                        actor.health.AddHediff(HediffDef.Named("AA_GatheredResource"));
                    }
                }
            };
            wait.AddFinishAction(delegate
            {
                Pawn pawn = (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
                if (pawn != null && pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                }
            });
            wait.FailOnDespawnedOrNull(TargetIndex.A);
            wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            wait.AddEndCondition(delegate
            {
                if (!this.GetComp((Pawn)((Thing)this.job.GetTarget(TargetIndex.A))).ActiveAndFull)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            wait.WithProgressBar(TargetIndex.A, () => this.gatherProgress / this.WorkTotal, false, -0.5f);
            wait.activeSkill = (() => SkillDefOf.Animals);

            yield return wait;
            yield break;
        }

        private float gatherProgress;
    }
}