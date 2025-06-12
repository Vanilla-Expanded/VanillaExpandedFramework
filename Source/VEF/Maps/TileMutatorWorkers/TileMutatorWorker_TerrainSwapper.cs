
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VEF.Maps
{
    public class TileMutatorWorker_TerrainSwapper : TileMutatorWorker
    {
        public TileMutatorWorker_TerrainSwapper(TileMutatorDef def)
            : base(def)
        {
        }

        public override void GeneratePostTerrain(Map map)
        {
            TileMutatorExtension extension = this.def.GetModExtension<TileMutatorExtension>();
            if (extension != null)
            {
                foreach (IntVec3 cell in map.AllCells)
                {
                    if (cell.GetTerrain(map) == extension.terrainToSwap)
                    {
                        map.terrainGrid.SetTerrain(cell, extension.terrainToSwapTo);
                    }
                }

            }


        }
    }
}