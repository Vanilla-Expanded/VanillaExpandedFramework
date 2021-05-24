using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    class SymbolResolver_KCSG_GenerateRoad : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            int x = rp.rect.Corners.ElementAt(2).x,
                y = rp.rect.Corners.ElementAt(2).z;
            Map map = BaseGen.globalSettings.map;

            GridUtils.AddRoadToGrid(CurrentGenerationOption.grid, CurrentGenerationOption.doors);

            int o = 0;
            for (int i = 0; i < CurrentGenerationOption.grid.Length; i++)
            {
                for (int j = 0; j < CurrentGenerationOption.grid[i].Length; j++)
                {
                    IntVec3 cell = new IntVec3(x + i, 0, y - j);
                    switch (CurrentGenerationOption.grid[i][j].Type)
                    {
                        case CellType.ROAD:
                            o++;
                            GenUtils.GenerateTerrainAt(map, cell, CurrentGenerationOption.settlementLayoutDef.roadDef);
                            break;

                        case CellType.MAINROAD:
                            GenUtils.GenerateTerrainAt(map, cell, CurrentGenerationOption.settlementLayoutDef.mainRoadDef);
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
