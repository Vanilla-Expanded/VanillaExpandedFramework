using RimWorld;
using Verse;
using Verse.AI;

namespace VEF.Buildings;

public class WorkGiver_RefuelSecondary : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Refuelable);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (CompProperties_Refuelable_DualFuel.allSecondaryFuelDefs.Contains(t.def) is false)
        {
            return false;
        }
        if (t.TryGetComp<CompRefuelable_DualFuel>() is CompRefuelable_DualFuel comp)
        {
            return CanRefuelSecondary(pawn, t, comp, forced);
        }
        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t.TryGetComp<CompRefuelable_DualFuel>() is CompRefuelable_DualFuel comp)
        {
            return RefuelSecondaryJob(pawn, t, comp, forced);
        }
        return null;
    }

    private bool CanRefuelSecondary(Pawn pawn, Thing t, CompRefuelable_DualFuel compRefuelable, bool forced = false)
    {
        if (compRefuelable == null || compRefuelable.parent.Fogged() || compRefuelable.IsSecondaryFull)
        {
            return false;
        }

        if (!forced && !compRefuelable.allowAutoRefuelSecondary)
        {
            return false;
        }

        if (!forced && !compRefuelable.ShouldAutoRefuelSecondaryNow)
        {
            return false;
        }

        if (!pawn.CanReserve(t, 1, -1, null, forced))
        {
            return false;
        }

        if (t.Faction != pawn.Faction)
        {
            return false;
        }

        if (FindBestSecondaryFuel(pawn, t, compRefuelable) == null)
        {
            ThingFilter fuelFilter = compRefuelable.Props.secondaryFuelFilter;
            JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter.Summary));
            return false;
        }

        return true;
    }

    private Job RefuelSecondaryJob(Pawn pawn, Thing t, CompRefuelable_DualFuel compRefuelable, bool forced = false)
    {
        Thing thing = FindBestSecondaryFuel(pawn, t, compRefuelable);
        Job job = JobMaker.MakeJob(InternalDefOf.VEF_RefuelSecondary, t, thing);
        job.count = compRefuelable.GetSecondaryFuelCountToFullyRefuel();
        return job;
    }

    private Thing FindBestSecondaryFuel(Pawn pawn, Thing refuelable, CompRefuelable_DualFuel comp)
    {
        ThingFilter filter = comp.Props.secondaryFuelFilter;
        return GenClosest.ClosestThingReachable(
            pawn.Position,
            pawn.Map,
            filter.BestThingRequest,
            PathEndMode.ClosestTouch,
            TraverseParms.For(pawn),
            9999f,
            Validator);

        bool Validator(Thing x)
        {
            if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
            {
                return false;
            }
            if (!filter.Allows(x))
            {
                return false;
            }
            return true;
        }
    }


}
