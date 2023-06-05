using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class TiledStructureDef : Def
    {
        public int tilesNumber = 4;
        public IntRange tilesNumberRange = new IntRange(1, 4);
        public int maxDistanceFromCenter = 1;

        public List<string> centerTileTags = new List<string>();
        public List<string> allowedTileTags = new List<string>();

        internal List<TileDef> centerTileDefs;
        internal List<TileDef> allowedTileDefs;

        internal int maxSize;

        public void Resolve()
        {
            // Get all center tiles
            centerTileDefs = new List<TileDef>();
            for (int i = 0; i < centerTileTags.Count; i++)
            {
                var tag = centerTileTags[i];
                if (TileUtils.tilesTagCache.ContainsKey(tag))
                {
                    centerTileDefs.AddRange(TileUtils.tilesTagCache[tag]);
                }
            }
            // Get all allowed tiles
            allowedTileDefs = new List<TileDef>();
            for (int i = 0; i < allowedTileTags.Count; i++)
            {
                var tag = allowedTileTags[i];
                if (TileUtils.tilesTagCache.ContainsKey(tag))
                {
                    allowedTileDefs.AddRange(TileUtils.tilesTagCache[tag]);
                }
            }
            // Set maxSize
            var centerMax = GetBiggestTileSize(centerTileDefs);
            var allowedMax = GetBiggestTileSize(allowedTileDefs);
            maxSize = Math.Max(allowedMax, centerMax);
        }

        /// <summary>
        /// Return biggest size of all structures of all tiles
        /// </summary>
        /// <param name="tiles">List of tile definition</param>
        /// <returns>Biggest size of list</returns>
        private int GetBiggestTileSize(List<TileDef> tiles)
        {
            var size = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                for (int j = 0; j < tile.tileLayouts.Count; j++)
                {
                    var layout = tile.tileLayouts[j];
                    if (layout.maxSize > size)
                        size = layout.maxSize;
                }
            }

            return size;
        }
    }

    public class TileDef : Def
    {
        public string tag;
        public List<StructureLayoutDef> tileLayouts;
    }
}