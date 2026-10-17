using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VEF.Things;

public class WorkGiver_FightCustomFires : WorkGiver_Scanner
{
    protected const int NearbyPawnRadius = 15;
    protected const int MaxReservationCheckDistance = 15;
    protected const float HandledDistance = 5f;

    protected static readonly List<CustomFireDef> FiresWithDefaultWorkGiver = [];

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    static WorkGiver_FightCustomFires()
    {
        // Just in case loaded too early for some reason
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            var fires = DefDatabase<CustomFireDef>.AllDefsListForReading;
            for (var i = 0; i < fires.Count; i++)
            {
                var fire = fires[i];
                if (fire.useDefaultWorkGiver)
                    FiresWithDefaultWorkGiver.Add(fire);
            }
        });
    }

    public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        // After some profiling, concating the lists together was about 4-6 times faster
        // than iterating over the lists and yield returning each value.
        IEnumerable<Thing> result = [];
        for (var i = 0; i < FiresWithDefaultWorkGiver.Count; i++)
            result = result.Concat(pawn.Map.listerThings.ThingsOfDef(FiresWithDefaultWorkGiver[i]));
        
        return result;
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not CustomFire fire)
            return false;

        if (fire.parent is Pawn parent)
        {
            if (parent == pawn)
                return false;
            if ((parent.Faction == null || parent.Faction != pawn.Faction) && (parent.HostFaction == null || (parent.HostFaction != pawn.Faction && parent.HostFaction != pawn.HostFaction)))
                return false;
            if (!pawn.Map.areaManager.Home[fire.Position] && IntVec3Utility.ManhattanDistanceFlat(pawn.Position, parent.Position) > NearbyPawnRadius)
                return false;
            if (!pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
                return false;
        }
        else
        {
            if (pawn.WorkTagIsDisabled(WorkTags.Firefighting))
            {
                JobFailReason.Is("IncapableOfFirefighting".Translate());
                return false;
            }

            if (!pawn.Map.areaManager.Home[fire.Position])
            {
                JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
                return false;
            }
        }

        if ((pawn.Position - fire.Position).LengthHorizontalSquared > MaxReservationCheckDistance * MaxReservationCheckDistance && !pawn.CanReserve(fire, ignoreOtherReservations: forced))
            return false;
        if (!forced && FireIsBeingHandled(fire, pawn))
            return false;
        return true;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) => JobMaker.MakeJob(((CustomFire)t).Def.beatFireJobDef, t);

    public static bool FireIsBeingHandled(CustomFire f, Pawn potentialHandler)
    {
        if (!f.Spawned)
            return false;

        var pawn = f.Map.reservationManager.FirstRespectedReserver(f, potentialHandler);
        return pawn != null && pawn.Position.InHorDistOf(f.Position, HandledDistance);
    }
}