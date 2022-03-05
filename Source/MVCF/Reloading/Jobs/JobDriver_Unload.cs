using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Reloading
{
    internal class JobDriver_Unload : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var thing = job.GetTarget(TargetIndex.A).Thing;
            var comp = thing?.TryGetComp<CompReloadable>();

            this.FailOn(() => comp == null);
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            var done = Toils_General.Label();
            var reloadTicks = 0;
            var toil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Never,
                defaultDuration = comp.Props.ReloadTimePerShot.SecondsToTicks(),
                initAction = () =>
                {
                    reloadTicks = (comp.Props.ReloadTimePerShot * comp.ShotsRemaining).SecondsToTicks();
                },
                tickAction = () =>
                {
                    if (debugTicksSpentThisToil >= reloadTicks)
                    {
                        comp.Unload();
                        JumpToToil(done);
                    }

                    if (debugTicksSpentThisToil == reloadTicks - 2f.SecondsToTicks())
                        comp.Props.ReloadSound?.PlayOneShot(pawn);
                }
            };
            toil.WithProgressBar(TargetIndex.A, () => (float) debugTicksSpentThisToil / (float) reloadTicks);
            yield return toil;
            yield return done;
        }
    }
}