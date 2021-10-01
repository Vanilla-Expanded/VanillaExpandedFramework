namespace VFECore.Abilities
{
    using System.Collections.Generic;
    using Verse;
    using Verse.AI;

    public class JobDriver_CastAbilityOnce : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override string GetReport() => this.pawn.GetComp<CompAbilities>().currentlyCasting.def.JobReportString;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);



            //yield return Toils_Combat.GotoCastPosition(TargetIndex.A);

            CompAbilities comp     = this.pawn.GetComp<CompAbilities>();
            Toil          waitToil = Toils_General.Wait(comp.currentlyCasting.GetCastTimeForPawn(), TargetIndex.A);
            waitToil.WithProgressBarToilDelay(TargetIndex.C);
            yield return waitToil;

            Toil castToil = new Toil();
            castToil.initAction = () =>
                                  {
                                      LocalTargetInfo target = castToil.actor.jobs.curJob.GetTarget(TargetIndex.A);
                                      comp.currentlyCasting.Cast(target);
                                  };
            castToil.defaultCompleteMode = ToilCompleteMode.Instant;
            castToil.atomicWithPrevious  = true;
            yield return castToil;

        }
    }
}
