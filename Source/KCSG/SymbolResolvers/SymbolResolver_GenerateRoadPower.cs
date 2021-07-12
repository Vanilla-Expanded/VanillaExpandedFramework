using RimWorld;
using RimWorld.BaseGen;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_GenerateRoadPower : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            CGO.currentGenStep = "Generating roads";
            Map map = BaseGen.globalSettings.map;

            rp.rect.EdgeCells.ToList().ForEach(cell => SpawnConduit(cell, map));
            CGO.currentGenStepMoreInfo = "Connecting all buildings";
            GridUtils.AddRoadToGrid(CGO.grid, CGO.doors);
            GenerateRoad(rp, map);
        }

        private void GenerateRoad(ResolveParams rp, Map map)
        {
            CGO.currentGenStepMoreInfo = "Placing terrain";
            int x = rp.rect.Corners.ElementAt(2).x,
                y = rp.rect.Corners.ElementAt(2).z;

            for (int i = 0; i < CGO.grid.Length; i++)
            {
                for (int j = 0; j < CGO.grid[i].Length; j++)
                {
                    IntVec3 cell = new IntVec3(x + i, 0, y - j);
                    if (cell.InBounds(map))
                    {
                        switch (CGO.grid[i][j].Type)
                        {
                            case CellType.ROAD:
                                SpawnConduit(cell, BaseGen.globalSettings.map);
                                GenUtils.GenerateTerrainAt(map, cell, CGO.settlementLayoutDef.roadDef);
                                break;

                            case CellType.MAINROAD:
                                SpawnConduit(cell, BaseGen.globalSettings.map);
                                GenUtils.GenerateTerrainAt(map, cell, CGO.preRoadTypes?.Count > 0 ? CGO.preRoadTypes.RandomElement() : CGO.settlementLayoutDef.mainRoadDef);
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            BaseGen.symbolStack.Push("kcsg_addfields", rp, null);
        }

        private void SpawnConduit(IntVec3 cell, Map map)
        {
            if (cell.Walkable(map))
            {
                if (map.terrainGrid.TerrainAt(cell).affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);

                Thing c = ThingMaker.MakeThing(KThingDefOf.KCSG_PowerConduit);
                c.SetFactionDirect(map.ParentFaction);
                GenSpawn.Spawn(c, cell, map, WipeMode.VanishOrMoveAside);
            }
        }
    }
}