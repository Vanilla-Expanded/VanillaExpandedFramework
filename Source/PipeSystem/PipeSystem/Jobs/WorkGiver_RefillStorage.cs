using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    internal class WorkGiver_RefillStorage : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => CachedPipeNetManager.GetFor(pawn.Map).wantRefill;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var comp = CachedCompResourceStorage.GetFor(t);
            if (comp == null || comp.AmountCanAccept < 1 || t.IsForbidden(pawn) || !pawn.CanReserve(t, ignoreOtherReservations: forced) || t.Faction != pawn.Faction)
                return false;

            if (FindBestFuel(pawn, comp.Props.refillOptions.thing) == null)
            {
                JobFailReason.Is("PipeSystem_NothingToRefill".Translate(t.LabelCap));
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var comp = CachedCompResourceStorage.GetFor(t);

            Thing bestFuel = FindBestFuel(pawn, comp.Props.refillOptions.thing);

            Job job = JobMaker.MakeJob(PSDefOf.PS_FillStorage, t, bestFuel);
            job.count = (int)(comp.AmountCanAccept * comp.Props.refillOptions.ratio);

            return job;
        }

        private static Thing FindBestFuel(Pawn pawn, ThingDef fuel)
        {
            bool validator(Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x);
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(fuel), PathEndMode.ClosestTouch, TraverseParms.For(pawn), validator: validator);
        }
    }
}
