using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace VanillaGenesExpanded
{
    public class HediffCompProperties_CustomBlood : HediffCompProperties
    {
        public ThingDef customBloodThingDef = null;
        public string customBloodIcon = "";
        public EffecterDef customBloodEffect = null;
        public FleshTypeDef customWoundsFromFleshtype = null;

        public HediffCompProperties_CustomBlood()
        {
            this.compClass = typeof(HediffComp_CustomBlood);
        }
    }
}
