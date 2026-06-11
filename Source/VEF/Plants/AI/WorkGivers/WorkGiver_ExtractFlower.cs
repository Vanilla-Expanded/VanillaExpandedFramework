using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace VEF.Plants
{
    public class WorkGiver_ExtractFlower : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {

            return pawn.Map.GetComponent<MapComponent_BloomingPlants>().flowersOrderedForExtraction_InMap;


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

            bool result;
            Plant_Blooming plant = t as Plant_Blooming;
            if (plant == null || plant.IsBurning() || !plant.plantAwaitingExtraction)
            {
                result = false;
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
            return new Job(InternalDefOf.VEF_ExtractFlower, t);
        }
    }
}
