﻿
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;


namespace VEF.Graphics
{
    [StaticConstructorOnStartup]
    public static class StaticCollectionsClass
    {
        public static Dictionary<ThingDef, Dictionary<ThingDef,int>> graphicOffsets = new Dictionary<ThingDef, Dictionary<ThingDef, int>>();
       

        static StaticCollectionsClass()
        {
            List<GraphicOffsets> allgraphicOffsetLists = DefDatabase<GraphicOffsets>.AllDefsListForReading.ToList();
            foreach (GraphicOffsets individualList in allgraphicOffsetLists)
            {
                if (graphicOffsets.ContainsKey(individualList.thingDef))
                {
                    graphicOffsets[individualList.thingDef].AddRange(individualList.ingredientsAndOffsetList);

                }
                else {
                    graphicOffsets[individualList.thingDef] = individualList.ingredientsAndOffsetList;
                }
            }

           
        }


    }
}
