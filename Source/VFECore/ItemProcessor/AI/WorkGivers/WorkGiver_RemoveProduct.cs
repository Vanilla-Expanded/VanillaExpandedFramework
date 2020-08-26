using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
namespace ItemProcessor
{
    public class WorkGiver_RemoveProduct : WorkGiver_Scanner
    {


        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {

            return pawn.Map.GetComponent<ItemProcessor_MapComponent>().itemProcessors_InMap;


        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }



        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_ItemProcessor building_processor = t as Building_ItemProcessor;
            bool result;
            if (building_processor == null || building_processor.processorStage != ProcessorStage.Finished)
            {
                return false;
            }

            else
            {
                if (!t.IsForbidden(pawn))
                {
                    LocalTargetInfo target = t;
                    if (pawn.CanReserve(target, 1, -1, null, forced))
                    {
                        result = true;
                        return result;
                    }
                }
                result = false;
            }
            return result;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {

            return new Job(DefDatabase<JobDef>.GetNamed("IP_RemoveProduct", true), t);
        }


    }
}


