
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class StaticCollections
    {
        public static Dictionary<ThingDef, int> graphicOffsets = new Dictionary<ThingDef, int>();
       

        static StaticCollections()
        {
            List<GraphicOffsets> allgraphicOffsetLists = DefDatabase<GraphicOffsets>.AllDefsListForReading.ToList();
            foreach (GraphicOffsets individualList in allgraphicOffsetLists)
            {

                graphicOffsets.AddRange(individualList.ingredientsAndOffsetList);
            }

           
        }


    }
}
