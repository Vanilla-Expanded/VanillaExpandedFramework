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
        public static readonly Dictionary<string, List<TileDef>> tilesTagCache = new Dictionary<string, List<TileDef>>();

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

        /// <summary>
        /// Generate structures on map from TiledStructureDef
        /// </summary>
        /// <param name="def"></param>
        /// <param name="center"></param>
        /// <param name="map"></param>
        public static void Generate(this TiledStructureDef def, IntVec3 center, Map map)
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
            // Tile in center
            var tilesNumber = def.tilesNumber;
            if (!def.centerTileDefs.NullOrEmpty())
            {
                GenerateTileIn(CellRect.CenteredOn(center, def.maxSize, def.maxSize), map, def.centerTileDefs.RandomElement());
                usableCenters.Remove(center);
                tilesNumber--;
            }
            // Generate other tiles
            for (int i = 0; i < tilesNumber; i++)
            {
                var rCell = usableCenters.RandomElement();
                GenerateTileIn(CellRect.CenteredOn(rCell, def.maxSize, def.maxSize), map, def.allowedTileDefs.RandomElement());
                usableCenters.Remove(rCell);
            }
        }

        /// <summary>
        /// Generate TileDef in rect
        /// </summary>
        /// <param name="cellRect"></param>
        /// <param name="map"></param>
        /// <param name="tileDef"></param>
        private static void GenerateTileIn(CellRect cellRect, Map map, TileDef tileDef)
        {
            GenOption.GetAllMineableIn(cellRect, map);
            var layoutDef = tileDef.tileLayouts.RandomElement();
            LayoutUtils.CleanRect(layoutDef, map, cellRect, true);
            layoutDef.Generate(cellRect, map);
        }
    }
}
