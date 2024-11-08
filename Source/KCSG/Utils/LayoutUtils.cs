using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace KCSG
{
    public static class LayoutUtils
    {
        /// <summary>
        /// Generate layoutDef in rect
        /// </summary>
        public static void Generate(this StructureLayoutDef layout, CellRect rect, Map map, Faction factionOverride = null)
        {
            // Get random wall stuff if randomizeWall is set to true
            ThingDef wallForRoom = null;
            if (layout.randomizeWallStuffAtGen || (GenOption.StuffableOptions != null && GenOption.StuffableOptions.randomizeWall))
                wallForRoom = RandomUtils.RandomWallStuffWeighted(ThingDefOf.Wall);
            // Generate all layouts
            var faction = map.ParentFaction;
            if (factionOverride != null)
            {
                faction = factionOverride;
            }
           
            var cells = rect.Cells.ToList();
            for (int index = 0; index < layout.layouts.Count; index++)
            {
                for (int h = 0; h < layout.sizes.z; h++)
                {
                    for (int w = 0; w < layout.sizes.x; w++)
                    {
                        var cell = cells[(h * layout.sizes.x) + w];
                        var symbol = layout._layouts[index][h, w];

                        if (cell.InBounds(map) && symbol != null)
                            symbol.Generate(layout, map, cell, faction, wallForRoom);
                    }
                }
            }
            // Generate terrain
            GenerateTerrainGrid(layout, cells, map);
            // Generate roof
            GenerateRoofGrid(layout, cells, map);
        }

        /// <summary>
        /// Generate roof from layout resolved roof grid
        /// </summary>
        private static void GenerateRoofGrid(StructureLayoutDef layout, List<IntVec3> cells, Map map)
        {
            if (layout._roofGrid != null && layout._roofGrid.Length > 0)
            {
                for (int h = 0; h < layout.sizes.z; h++)
                {
                    for (int w = 0; w < layout.sizes.x; w++)
                    {
                        var cell = cells[(h * layout.sizes.x) + w];
                        var roof = layout._roofGrid[h, w];

                        if (cell.InBounds(map) && roof != null)
                        {
                            if (roof == "0" && layout.forceGenerateRoof)
                            {
                                map.roofGrid.SetRoof(cell, null);
                                continue;
                            }

                            var currentRoof = cell.GetRoof(map);
                            if (roof == "1" && (layout.forceGenerateRoof || currentRoof == null))
                            {
                                map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
                                ClearInterferesWithRoof(cell, map);
                                continue;
                            }

                            if (roof == "2" && (layout.forceGenerateRoof || currentRoof == null || currentRoof == RoofDefOf.RoofConstructed))
                            {
                                map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThin);
                                ClearInterferesWithRoof(cell, map);
                                continue;
                            }

                            if (roof == "3")
                            {
                                map.roofGrid.SetRoof(cell, RoofDefOf.RoofRockThick);
                                ClearInterferesWithRoof(cell, map);
                                continue;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Despawn everything that block/interfer with roofs
        /// </summary>
        private static void ClearInterferesWithRoof(IntVec3 cell, Map map)
        {
            var t = cell.GetPlant(map);
            if (t != null && t.def.plant != null && t.def.plant.interferesWithRoof)
                t.DeSpawn();

            GenOption.DespawnMineableAt(cell);
        }

        /// <summary>
        /// Generate terrain grid from layout
        /// </summary>
        private static void GenerateTerrainGrid(StructureLayoutDef layout, List<IntVec3> cells, Map map)
        {
            if (layout._terrainGrid == null || layout._terrainGrid.Length == 0)
                return;

            var useColorGrid = layout._terrainColorGrid != null && layout._terrainColorGrid.Length > 0;
            for (int h = 0; h < layout.sizes.z; h++)
            {
                for (int w = 0; w < layout.sizes.x; w++)
                {
                    var cell = cells[(h * layout.sizes.x) + w];
                    var terrain = layout._terrainGrid[h, w];
                    if (terrain == null || !cell.InBounds(map))
                        continue;
                    //if (!cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy))
                    //{
                    //    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Bridge);
                    //}
                    //else
                    //{
                        GenOption.DespawnMineableAt(cell);
                        map.terrainGrid.SetTerrain(cell, terrain);
                    //}

                    if (useColorGrid)
                        map.terrainGrid.SetTerrainColor(cell, layout._terrainColorGrid[h, w]);
                }
            }
        }

        /// <summary>
        /// Clean structure spawn rect. Full clean remove everything but player/map pawns.
        /// Normal clean remove filth, non-natural buildings and stone/slag chunks
        /// </summary>
        public static void CleanRect(StructureLayoutDef layout, Map map, CellRect rect, bool fullClean)
        {
            var mapFaction = map.ParentFaction;
            var player = Faction.OfPlayer;
            var cells = rect.Cells.ToList();

            if (layout == null || layout._roofGrid == null || layout._roofGrid.Length == 0)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    var cell = cells[i];
                    if (cell.InBounds(map))
                        CleanAt(cell, map, fullClean, mapFaction, player);
                }
            }
            else
            {
                var width = rect.Width;
                var height = rect.Height;

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        var cell = cells[(h * width) + w];
                        var roof = layout._roofGrid[h, w];

                        if (cell.InBounds(map) && roof != ".")
                            CleanAt(cell, map, fullClean, mapFaction, player);
                    }
                }
            }
            // Update roof grid if it was a fullclean
            if (fullClean)
                map.roofGrid?.RoofGridUpdate();
        }

        /// <summary>
        /// Clean at a cell. Terrain & things
        /// </summary>
        private static void CleanAt(IntVec3 c, Map map, bool fullClean, Faction mapFaction, Faction player)
        {
            // Clean things
            var things = c.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Thing thing && thing.Spawned)
                {
                    if (thing is Pawn p && p.Faction != mapFaction && p.Faction != player)
                    {
                        thing.DeSpawn();
                    }
                    else if (fullClean && thing.def.category != ThingCategory.Plant)
                    {
                        thing.DeSpawn();
                    }
                    // Clear filth, buildings, stone/slag chunks
                    else if (thing.def.category == ThingCategory.Filth ||
                             (thing.def.category == ThingCategory.Building && !thing.def.building.isNaturalRock) ||
                             (thing.def.thingCategories != null && thing.def.thingCategories.Contains(ThingCategoryDefOf.Chunks)))
                    {
                        thing.DeSpawn();
                    }
                }
            }
            // Clean terrain
            if (map.terrainGrid.UnderTerrainAt(c) is TerrainDef terrain && terrain != null)
            {
                map.terrainGrid.SetTerrain(c, terrain);
            }
            // Clean roof
            if (fullClean)
                map.roofGrid.SetRoof(c, null);
        }
    }
}