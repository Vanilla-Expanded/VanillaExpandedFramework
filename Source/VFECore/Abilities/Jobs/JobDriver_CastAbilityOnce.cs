namespace VFECore.Abilities
{
    using System.Collections.Generic;
    using Verse;
    using Verse.AI;

    public class JobDriver_CastAbilityOnce : JobDriver
    {
        private CompAbilities cachedComp;
        public CompAbilities CompAbilities
        {
            get
            {
                if (cachedComp is null)
                {
                    cachedComp = this.pawn.GetComp<CompAbilities>();
                }
                return cachedComp;
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override string GetReport() => CompAbilities.currentlyCasting.def.JobReportString;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);



            //yield return Toils_Combat.GotoCastPosition(TargetIndex.A);

            CompAbilities comp     = CompAbilities;
            Toil          waitToil = Toils_General.Wait(comp.currentlyCasting.GetCastTimeForPawn(), TargetIndex.A);
            waitToil.WithProgressBarToilDelay(TargetIndex.C);

            if (this.TargetA.Pawn != this.pawn)
                waitToil.AddPreTickAction(() =>
                                          {
                                              if (Find.Selector.IsSelected(this.pawn))
                                                  GenDraw.DrawAimPie(this.pawn, this.TargetA, this.ticksLeftThisToil, 0.2f);
                                          });
            comp.currentlyCasting.WarmupToil(waitToil);
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
