using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class HediffCompProperties_HeatPusher : HediffCompProperties
    {
        public int tickCounter = 60;
        public float heatPushMaxTemperature = 99999f;
        public float heatPushMinTemperature = -99999f;
        public float heatPerSecond;

        public HediffCompProperties_HeatPusher()
        {
            this.compClass = typeof(HediffComp_HeatPusher);
        }
    }
}
