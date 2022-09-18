using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PipeSystem
{
    public class WorkGiver_DrainFromMarkedStorage : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn) => Danger.Deadly;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Designation> desList = pawn.Map.designationManager.AllDesignations;
            for (int i = 0; i < desList.Count; i++)
            {
                var des = desList[i];
                if (des.def == PSDefOf.PS_Drain)
                {
                    yield return des.target.Thing;
                }
            }
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(PSDefOf.PS_Drain);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(t, PSDefOf.PS_Drain) == null)
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(PSDefOf.PS_DrainFromMarkedStorage, t);
        }
    }
}
