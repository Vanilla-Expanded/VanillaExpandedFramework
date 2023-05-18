using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class TiledStructureDef : Def
    {
        public int tilesNumber = 4;
        public int maxDistanceFromCenter = 1;

        public List<string> allowedTileTags = new List<string>();
    }

    public class TileDef : Def
    {
        public string tag;
        public List<StructureLayoutDef> tileLayouts;
    }
}