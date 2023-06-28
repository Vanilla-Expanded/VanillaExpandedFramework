using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace KCSG
{
    public static class TileUtils
    {
        /// <summary>
        /// Generate structures on map from TiledStructureDef
        /// </summary>
        /// <param name="def"></param>
        /// <param name="center"></param>
        /// <param name="map"></param>
        /// <param name="quest"></param>
        public static void Generate(this TiledStructureDef def, IntVec3 center, Map map, Quest quest = null)
        {
            GenOption.tiledRects = new Dictionary<CellRect, TileDef>();
            // Get usables centers
            var usableCenters = new List<IntVec3>();
            var possibleCells = CellRect.CenteredOn(center, ((def.maxSize + def.rectSizeIncrease) / 2) + ((def.maxSize + def.rectSizeIncrease) * def.maxDistanceFromCenter));
            foreach (var cell in possibleCells)
            {
                if (!cell.InBounds(map)) continue;
                if (Math.Abs(cell.x - center.x) % (def.maxSize + def.rectSizeIncrease) != 0) continue;
                if (Math.Abs(cell.z - center.z) % (def.maxSize + def.rectSizeIncrease) != 0) continue;
                usableCenters.Add(cell);
            }
            // Keep track of used things
            var usedTileDefs = new Dictionary<TileDef, int>();
            var usedLayouts = new List<StructureLayoutDef>();
            var usedCenters = new List<IntVec3>();
            // Get amount of tiles
            var tilesNumber = def.tilesNumber;
            if (quest != null)
                tilesNumber = def.tilesNumberRange.Lerped(quest.challengeRating / 4);
            // Tile in center
            if (!def.centerTileDefs.NullOrEmpty())
            {
                GenerateTileIn(CellRect.CenteredOn(center, def.maxSize, def.maxSize), map, def.centerTileDefs.RandomElement(), ref usedLayouts);
                GenerateIncreaseTerrain(def, center, map);
                usableCenters.Remove(center);
                usedCenters.Add(center);
                tilesNumber--;
            }
            // Required tile(s)
            var requiredTiles = new List<TileDef>();
            foreach (var req in def._requiredTileDefs)
            {
                var tile = req.Key;
                for (int i = 0; i < req.Value.x; i++)
                    requiredTiles.Add(tile);
            }
            requiredTiles.Shuffle();
            for (int i = 0; i < requiredTiles.Count; i++)
            {
                var tile = requiredTiles[i];
                GetAdjacentIntvec3(def, ref usedCenters, ref usableCenters, out IntVec3 cell);
                GenerateTileIn(CellRect.CenteredOn(cell, def.maxSize, def.maxSize), map, tile, ref usedLayouts);
                GenerateIncreaseTerrain(def, cell, map);
                AddToDict(tile, ref usedTileDefs);
                tilesNumber--;
            }
            // Generate other tiles
            for (int i = 0; i < tilesNumber; i++)
            {
                var tile = GetRandomTileDefFrom(def._allowedTileDefs, ref usedTileDefs);
                GetAdjacentIntvec3(def, ref usedCenters, ref usableCenters, out IntVec3 cell);
                GenerateTileIn(CellRect.CenteredOn(cell, def.maxSize, def.maxSize), map, tile, ref usedLayouts);
                GenerateIncreaseTerrain(def, cell, map);
                AddToDict(tile, ref usedTileDefs);
                tilesNumber--;
            }
        }

        private static TileDef GetRandomTileDefFrom(Dictionary<TileDef, IntVec2> tileDefs, ref Dictionary<TileDef, int> usedTileDefs)
        {
            // Get usable tiles
            var pool = new List<TileDef>();
            foreach (var pair in tileDefs)
            {
                var max = pair.Value.z;
                var tileDef = pair.Key;
                if (max > 0 && usedTileDefs.TryGetValue(tileDef) is int spawnedCount && spawnedCount >= max)
                    continue;

                pool.Add(tileDef);
            }
            // Choose random
            var choosedTile = pool.RandomElement();
            // Add it to used
            AddToDict(choosedTile, ref usedTileDefs);

            return choosedTile;
        }

        private static void AddToDict<T>(T el, ref Dictionary<T, int> dict)
        {
            if (dict.ContainsKey(el))
                dict[el]++;
            else
                dict.Add(el, 1);
        }

        private static void GetAdjacentIntvec3(TiledStructureDef def, ref List<IntVec3> used, ref List<IntVec3> left, out IntVec3 cell)
        {
            if (def.placeTileAdjacent && used.Count > 0)
            {
                var pool = new List<IntVec3>();
                for (int i = 0; i < left.Count; i++)
                {
                    var c = left[i];
                    if (used.Any(ce => (ce.x == c.x && Math.Abs(ce.z - c.z) == (def.maxSize + def.rectSizeIncrease)) || (ce.z == c.z && Math.Abs(ce.x - c.x) == (def.maxSize + def.rectSizeIncrease))))
                    {
                        pool.Add(c);
                    }
                }
                cell = pool.RandomElement();
            }
            else
            {
                cell = left.RandomElement();
            }

            left.Remove(cell);
            used.Add(cell);
        }

        private static void GenerateIncreaseTerrain(TiledStructureDef def, IntVec3 cell, Map map)
        {
            if (def.increaseTerrainDef != null)
            {
                var fRect = CellRect.CenteredOn(cell, def.maxSize, def.maxSize);
                var iRect = CellRect.CenteredOn(cell, def.maxSize + def.rectSizeIncrease, def.maxSize + def.rectSizeIncrease);
                var cells = iRect.Cells.ToList();
                cells.RemoveAll(c => fRect.Contains(c));

                for (int j = 0; j < cells.Count; j++)
                {
                    var c = cells[j];
                    if (map.terrainGrid.TerrainAt(c).affordances.Contains(TerrainAffordanceDefOf.Bridgeable))
                        map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
                    else
                        map.terrainGrid.SetTerrain(c, def.increaseTerrainDef);
                }
            }
        }

        /// <summary>
        /// Generate TileDef in rect
        /// </summary>
        /// <param name="cellRect"></param>
        /// <param name="map"></param>
        /// <param name="tileDef"></param>
        private static void GenerateTileIn(CellRect cellRect, Map map, TileDef tileDef, ref List<StructureLayoutDef> usedLayouts)
        {
            // Get not yet used layouts
            var layoutPool = new List<StructureLayoutDef>();
            for (int i = 0; i < tileDef.tileLayouts.Count; i++)
            {
                var layout = tileDef.tileLayouts[i];
                if (!usedLayouts.Contains(layout))
                    layoutPool.Add(layout);
            }
            // Random choice
            var layoutDef = layoutPool.Count > 0 ? layoutPool.RandomElement() : tileDef.tileLayouts.RandomElement();
            usedLayouts.Add(layoutDef);
            // Generate
            GenOption.GetAllMineableIn(cellRect, map);
            LayoutUtils.CleanRect(layoutDef, map, cellRect, true);
            layoutDef.Generate(cellRect, map);

            GenOption.tiledRects.Add(cellRect, tileDef);
        }
    }
}
