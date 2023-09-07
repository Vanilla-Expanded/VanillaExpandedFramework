using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    public class WorkGiver_PickUpProcessor : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => CachedAdvancedProcessorsManager.GetFor(pawn.Map).AwaitingPickup;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsBurning() || t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced) || t.Faction != pawn.Faction)
                return false;

            if (!CachedCompAdvancedProcessor.GetFor(t).PickupReady)
            {
                JobFailReason.Is("PipeSystem_NothingToPickUp".Translate());
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(PSDefOf.PS_PickUpProcessor, t);
    }
}