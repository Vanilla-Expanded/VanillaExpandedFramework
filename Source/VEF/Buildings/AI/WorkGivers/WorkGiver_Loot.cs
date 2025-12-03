using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;


namespace VEF.Buildings
{
    public class WorkGiver_Loot : WorkGiver_Scanner
    {


        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.GetComponent<MapComponent_InteractableBuildingsInMap>().lootables_InMap;
        }



        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }



        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return pawn.Map.GetComponent<MapComponent_InteractableBuildingsInMap>().lootables_InMap.Count == 0;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            LootableBuilding_Custom building = t as LootableBuilding_Custom;


            if (building == null)
            {
                return false;
            }


            if (t.IsForbidden(pawn))
            {
                return false;
            }

            if (!pawn.CanReserve(building, 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(building, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (building.IsBurning())
            {
                return false;
            }

            if (building.LootableExtension.useHackingSpeed && (pawn.skills.GetSkill(SkillDefOf.Intellectual).TotallyDisabled  || StatDefOf.HackingSpeed.Worker.IsDisabledFor(pawn)))
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(InternalDefOf.VFE_Loot, t);
        }
    }
}
