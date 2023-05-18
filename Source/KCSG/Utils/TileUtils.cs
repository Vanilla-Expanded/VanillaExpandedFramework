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
        private static readonly Dictionary<string, List<TileDef>> tilesTagCache = new Dictionary<string, List<TileDef>>();

        /// <summary>
        /// Cache all structure in a dict per tag
        /// </summary>
        public static void CacheTags()
        {
            if (tilesTagCache.NullOrEmpty())
            {
                var tileDefs = DefDatabase<TileDef>.AllDefsListForReading;
                for (int i = 0; i < tileDefs.Count; i++)
                {
                    var tile = tileDefs[i];
                    if (tilesTagCache.ContainsKey(tile.tag))
                    {
                        tilesTagCache[tile.tag].Add(tile);
                    }
                    else
                    {
                        tilesTagCache.Add(tile.tag, new List<TileDef> { tile });
                    }
                }
            }
        }

        public static void Generate(this TiledStructureDef tiledStructureDef, IntVec3 center, Map map)
        {
            // Get all allowed tiles
            var allowedTiles = new List<TileDef>();
            for (int i = 0; i < tiledStructureDef.allowedTileTags.Count; i++)
            {
                var tag = tiledStructureDef.allowedTileTags[i];
                if (tilesTagCache.ContainsKey(tag))
                {
                    allowedTiles.AddRange(tilesTagCache[tag]);
                }
            }
            // Get the biggest tile size from list
            var tileSize = GetBiggestTileSize(allowedTiles);
            // Get usables centers
            var usableCenters = new List<IntVec3>();
            var possibleCells = CellRect.CenteredOn(center, (tileSize / 2) + (tileSize * tiledStructureDef.maxDistanceFromCenter));
            foreach (var cell in possibleCells)
            {
                if (!cell.InBounds(map)) continue;
                if (Math.Abs(cell.x - center.x) % tileSize != 0) continue;
                if (Math.Abs(cell.z - center.z) % tileSize != 0) continue;
                usableCenters.Add(cell);
            }
            // First tile always in center
            GenerateTileIn(CellRect.CenteredOn(center, tileSize, tileSize), map, allowedTiles.RandomElement());
            usableCenters.Remove(center);
            // Generate other tiles
            for (int i = 0; i < tiledStructureDef.tilesNumber - 1; i++)
            {
                var rCell = usableCenters.RandomElement();
                GenerateTileIn(CellRect.CenteredOn(rCell, tileSize, tileSize), map, allowedTiles.RandomElement());
                usableCenters.Remove(rCell);
            }
        }

        /// <summary>
        /// Return biggest size of all structures of all tiles
        /// </summary>
        /// <param name="tiles">List of tile definition</param>
        /// <returns>Biggest size of list</returns>
        private static int GetBiggestTileSize(List<TileDef> tiles)
        {
            var size = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                for (int j = 0; j < tile.tileLayouts.Count; j++)
                {
                    var layout = tile.tileLayouts[j];
                    if (layout.size > size)
                        size = layout.size;
                }
            }

            return size;
        }

        private static void GenerateTileIn(CellRect cellRect, Map map, TileDef tileDef)
        {
            GenOption.GetAllMineableIn(cellRect, map);
            var layoutDef = tileDef.tileLayouts.RandomElement();
            LayoutUtils.CleanRect(layoutDef, map, cellRect, true);
            layoutDef.Generate(cellRect, map);
        }
    }
}
