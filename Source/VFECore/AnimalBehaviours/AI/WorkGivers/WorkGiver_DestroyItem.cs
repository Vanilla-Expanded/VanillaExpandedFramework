using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class WorkGiver_DestroyItem : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {

            return pawn.Map.GetComponent<DestroyableObjects_MapComponent>().objects_InMap;


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
            if (t == null || t.IsBurning() || !t.TryGetComp<CompDestroyThisItem>().itemNeedsDestruction)
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
            return new Job(InternalDefOf.VEF_DestroyItem, t);
        }
    }
}
