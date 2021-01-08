using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Buildings;

namespace VFE.Mechanoids.AI.WorkGivers
{
    class WorkGiver_AttachTurret : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Undefined);
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (CompMachineChargingStation.cachedChargingStationsDict.TryGetValue(t, out CompMachineChargingStation comp)) 
            {
                if (comp == null || comp.turretToInstall == null)
                    return false;

                Pawn myPawn = comp.myPawn;
                if (myPawn == null || myPawn.Dead || !myPawn.Spawned)
                {
                    JobFailReason.Is("VFEMechNoTurret".Translate());
                    return false;
                }
                List<ThingDefCountClass> products = comp.turretToInstall.costList;
                foreach (ThingDefCountClass thingNeeded in products)
                {
                    if (!pawn.Map.itemAvailability.ThingsAvailableAnywhere(thingNeeded, pawn))
                    {
                        JobFailReason.Is("VFEMechNoResources".Translate());
                        return false;
                    }
                }
                return pawn.CanReserveAndReach(t, PathEndMode.OnCell, Danger.Deadly, ignoreOtherReservations: forced);
            }
            return false;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            List<ThingDefCountClass> products = t.TryGetComp<CompMachineChargingStation>().turretToInstall.costList;
            List<Thing> toGrab = new List<Thing>();
            List<int> toGrabCount = new List<int>();
            foreach (ThingDefCountClass thingNeeded in products)
            {
                List<Thing> thingsOfThisType=RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, t.Position, new IntRange(thingNeeded.count, thingNeeded.count), (Thing thing) => thing.def == thingNeeded.thingDef);
                if(thingsOfThisType==null)
                {
                    return null;
                }
                toGrab.AddRange(thingsOfThisType);
                int totalCountNeeded = thingNeeded.count;
                foreach(Thing thingGrabbed in thingsOfThisType)
                {
                    if(thingGrabbed.stackCount >= totalCountNeeded)
                    {
                        toGrabCount.Add(totalCountNeeded);
                        totalCountNeeded = 0;
                    }
                    else
                    {
                        toGrabCount.Add(thingGrabbed.stackCount);
                        totalCountNeeded -= thingGrabbed.stackCount;
                    }
                }
            }
            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("VFE_Mechanoids_AttachTurret"), t);
            job.targetQueueB = toGrab.Select((Thing f) => new LocalTargetInfo(f)).ToList();
            job.countQueue = toGrabCount.ToList();
            return job;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (CompMachineChargingStation compMachineChargingStation in CompMachineChargingStation.cachedChargingStations)
            {
                if (compMachineChargingStation.parent.Map == pawn.Map)
                {
                    yield return compMachineChargingStation.parent;
                }
            }
        }
    }
}
