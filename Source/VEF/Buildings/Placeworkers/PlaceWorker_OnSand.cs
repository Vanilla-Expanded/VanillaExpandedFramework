using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace VEF.Buildings
{
    public class PlaceWorker_OnSand : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(loc, rot, checkingDef.Size))
            {
                if (map.terrainGrid.TerrainAt(c).categoryType != TerrainDef.TerrainCategoryType.Sand)
                {
                    return new AcceptanceReport("VFE_NeedsSand".Translate());
                }
            }
           


            return true;
        }

        




    }


}


