using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VEF.Graphics
{
    public class JobDriver_CustomizeItem : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(120, TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, true);
            yield return Toils_General.Do(() =>
            {
                var comp = TargetA.Thing.TryGetComp<CompGraphicCustomization>();
                comp.Customize();
            });
        }
    }
}
