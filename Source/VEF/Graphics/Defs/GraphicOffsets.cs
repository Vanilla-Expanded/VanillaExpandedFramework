using RimWorld;
using System;
using Verse;
using System.Collections.Generic;


namespace VEF.Graphics
{
    [StaticConstructorOnStartup]
    public class GraphicOffsets : Def
    {
        //The ThingDef this GraphicOffsets is targetting
        public ThingDef thingDef;

        //A Dictionary of ingredient ThingDefs and their offset
        public Dictionary<ThingDef, int> ingredientsAndOffsetList;
    }



}

