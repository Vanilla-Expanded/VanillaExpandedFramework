using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    public class TiledStructureDef : Def
    {
        public int tilesNumber = 4;
        public IntRange tilesNumberRange = new IntRange(1, 4);
        public int maxDistanceFromCenter = 1;

        public List<TileDef> centerTileDefs = new List<TileDef>();
        public List<TileTagOtion> allowedTileDefs = new List<TileTagOtion>();

        internal Dictionary<IntVec2, TileDef> _allowedTileDefs;
        internal Dictionary<IntVec2, TileDef> _requiredTileDefs;

        internal int maxSize;

        public void Resolve()
        {
            // Get all allowed/required tiles
            _allowedTileDefs = new Dictionary<IntVec2, TileDef>();
            _requiredTileDefs = new Dictionary<IntVec2, TileDef>();

            var allowedAll = new List<TileDef>();
            for (int i = 0; i < allowedTileDefs.Count; i++)
            {
                var opt = allowedTileDefs[i];
                if (opt.count.x > 0)
                    _requiredTileDefs.Add(opt.count, opt.def);

                _allowedTileDefs.Add(opt.count, opt.def);
                allowedAll.Add(opt.def);
            }
            // Set maxSize
            var centerMax = GetBiggestTileSize(centerTileDefs);
            var allowedMax = GetBiggestTileSize(allowedAll);
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

    public class TileTagOtion
    {
        public TileDef def;
        public IntVec2 count = new IntVec2(0, 0);
    }

    public class TileDef : Def
    {
        public List<StructureLayoutDef> tileLayouts;
    }
}