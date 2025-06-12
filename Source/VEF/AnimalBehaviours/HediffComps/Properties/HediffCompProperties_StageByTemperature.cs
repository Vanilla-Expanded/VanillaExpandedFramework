using System;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_StageByTemperature : HediffCompProperties
    {

        public int minTemp = 0;
        public int maxTemp = 0;
      

        public HediffCompProperties_StageByTemperature()
        {
            this.compClass = typeof(HediffComp_StageByTemperature);
        }
    }
}
