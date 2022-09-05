using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class HediffCompProperties_LightSustenance : HediffCompProperties
    {
        //This is equivalent to the CompLightSustenance class, but just adds things through a hediff

        public HediffCompProperties_LightSustenance()
        {
            this.compClass = typeof(HediffComp_LightSustenance);
        }
    }
}
