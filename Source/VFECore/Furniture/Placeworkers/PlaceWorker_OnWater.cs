using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VanillaFurnitureExpanded
{
    public class PlaceWorker_OnWater : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            {
                if (!map.terrainGrid.TerrainAt(c).IsWater)
                {
                    return new AcceptanceReport("VFE_NeedsWater".Translate());
                }
            }
           


            return true;
        }

        




    }


}


