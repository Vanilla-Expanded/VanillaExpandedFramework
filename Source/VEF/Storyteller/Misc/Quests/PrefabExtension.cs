using System.Collections.Generic;
using Verse;

namespace VEF.Storyteller
{
    public class PrefabExtension : DefModExtension
    {
        public List<PrefabRoofData> roofs;
    }

    public class PrefabRoofData
    {
        public RoofDef def;
        public List<CellRect> rects;
    }
}