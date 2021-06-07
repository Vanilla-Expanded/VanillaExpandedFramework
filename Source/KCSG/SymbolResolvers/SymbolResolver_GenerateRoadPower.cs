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
            CurrentGenerationOption.currentGenStep = "Generating roads";
            Map map = BaseGen.globalSettings.map;

            rp.rect.EdgeCells.ToList().ForEach(cell => SpawnConduit(cell, map));
            CurrentGenerationOption.currentGenStepMoreInfo = "Connecting all buildings";
            GridUtils.AddRoadToGrid(CurrentGenerationOption.grid, CurrentGenerationOption.doors);
            GenerateRoad(rp, map);
        }

        private void GenerateRoad(ResolveParams rp, Map map)
        {
            CurrentGenerationOption.currentGenStepMoreInfo = "Placing terrain";
            int x = rp.rect.Corners.ElementAt(2).x,
                y = rp.rect.Corners.ElementAt(2).z;

            for (int i = 0; i < CurrentGenerationOption.grid.Length; i++)
            {
                for (int j = 0; j < CurrentGenerationOption.grid[i].Length; j++)
                {
                    IntVec3 cell = new IntVec3(x + i, 0, y - j);
                    if (cell.InBounds(map))
                    {
                        switch (CurrentGenerationOption.grid[i][j].Type)
                        {
                            case CellType.ROAD:
                                SpawnConduit(cell, BaseGen.globalSettings.map);
                                GenUtils.GenerateTerrainAt(map, cell, CurrentGenerationOption.settlementLayoutDef.roadDef);
                                break;

                            case CellType.MAINROAD:
                                SpawnConduit(cell, BaseGen.globalSettings.map);
                                GenUtils.GenerateTerrainAt(map, cell, CurrentGenerationOption.preRoadTypes?.Count > 0 ? CurrentGenerationOption.preRoadTypes.RandomElement() : CurrentGenerationOption.settlementLayoutDef.mainRoadDef);
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

                Thing c = ThingMaker.MakeThing(KDefOf.KCSG_PowerConduit);
                c.SetFactionDirect(map.ParentFaction);
                GenSpawn.Spawn(c, cell, map, WipeMode.VanishOrMoveAside);
            }
        }
    }
}