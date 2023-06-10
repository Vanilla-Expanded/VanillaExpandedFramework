using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

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
            // Get usables centers
            var usableCenters = new List<IntVec3>();
            var possibleCells = CellRect.CenteredOn(center, (def.maxSize / 2) + (def.maxSize * def.maxDistanceFromCenter));
            foreach (var cell in possibleCells)
            {
                if (!cell.InBounds(map)) continue;
                if (Math.Abs(cell.x - center.x) % def.maxSize != 0) continue;
                if (Math.Abs(cell.z - center.z) % def.maxSize != 0) continue;
                usableCenters.Add(cell);
            }
            // Keep track of used structureLayout
            var usedTileDefs = new Dictionary<TileDef, int>();
            var usedLayouts = new List<StructureLayoutDef>();
            // Get amount of tiles
            var tilesNumber = def.tilesNumber;
            if (quest != null)
                tilesNumber = def.tilesNumberRange.Lerped(quest.challengeRating / 4);
            // Tile in center
            if (!def.centerTileDefs.NullOrEmpty())
            {
                GenerateTileIn(CellRect.CenteredOn(center, def.maxSize, def.maxSize), map, def.centerTileDefs.RandomElement(), ref usedLayouts);
                usableCenters.Remove(center);
                tilesNumber--;
            }
            // Required tile(s)
            foreach (var req in def._requiredTileDefs)
            {
                var tile = req.Value;
                for (int i = 0; i < req.Key.x; i++) // Spawn min amount
                {
                    var rCell = usableCenters.RandomElement();
                    GenerateTileIn(CellRect.CenteredOn(rCell, def.maxSize, def.maxSize), map, tile, ref usedLayouts);
                    AddToDict(tile, ref usedTileDefs);
                    usableCenters.Remove(rCell);
                    tilesNumber--;
                }
            }
            // Generate other tiles
            for (int i = 0; i < tilesNumber; i++)
            {
                var rCell = usableCenters.RandomElement();
                GenerateTileIn(CellRect.CenteredOn(rCell, def.maxSize, def.maxSize), map, GetRandomTileDefFrom(def._allowedTileDefs, ref usedTileDefs), ref usedLayouts);
                usableCenters.Remove(rCell);
            }
        }

        private static TileDef GetRandomTileDefFrom(Dictionary<IntVec2, TileDef> tileDefs, ref Dictionary<TileDef, int> usedTileDefs)
        {
            // Get usable tiles
            var pool = new List<TileDef>();
            foreach (var pair in tileDefs)
            {
                var max = pair.Key.z;
                var tileDef = pair.Value;
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
        }
    }
}
