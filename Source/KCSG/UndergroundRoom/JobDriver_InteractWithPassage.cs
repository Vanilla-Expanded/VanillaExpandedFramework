using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace KCSG.UndergroundRoom
{
    public class JobDriver_InteractWithPassage : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (TargetThingA.TryGetComp<CompUndergroundPassage>() == null)
                return false;

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            var enter = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            enter.AddFinishAction(delegate
            {
                Find.World.GetComponent<UndergroundManager>().Enter(TargetThingA.TryGetComp<CompUndergroundPassage>(), pawn);
            });
            yield return enter;
        }
    }
}
