
using RimWorld;
using UnityEngine;
using Verse;

namespace VEF.Plants
{
    public class Designator_ReplantFlower : Designator_Install
    {
        public override string Label => "VPE_ReplantFlower".Translate();

        public override string Desc => "VPE_ReplantFlower_Desc".Translate();

        public Designator_ReplantFlower()
        {
            icon = ContentFinder<Texture2D>.Get("UI/Gizmo/ReplantFlower");
            soundSucceeded = SoundDefOf.Designate_ExtractTree;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            Plant_Blooming plant = (Plant_Blooming)base.ThingToInstall;
            Thing blockingThing;
            AcceptanceReport acceptanceReport = plant.def.CanEverPlantAt(c, base.Map, out blockingThing, canWipePlantsExceptTree: true);
            if (!acceptanceReport)
            {
                return new AcceptanceReport("CannotBePlantedHere".Translate() + ": " + acceptanceReport.Reason.CapitalizeFirst());
            }
            if (plant.def.plant.interferesWithRoof && c.Roofed(base.Map))
            {
                return "CannotBePlantedHere".Translate() + ": " + "BlockedByRoof".Translate().CapitalizeFirst();
            }
            if (!plant.def.CanNowPlantAt(c, base.Map, canWipePlantsExceptTree: true))
            {
                return new AcceptanceReport("CannotBePlantedHere".Translate());
            }
            foreach (Thing thing in c.GetThingList(base.Map))
            {
                Blueprint_Install blueprint_Install = thing as Blueprint_Install;
                if (blueprint_Install != null && blueprint_Install.ThingToInstall.def.plant != null && blueprint_Install.ThingToInstall is Plant_Blooming)
                {
                    return "IdenticalThingExists".Translate();
                }
            }
            return base.CanDesignateCell(c);
        }
    }
}