using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class HediffCompProperties_ChangeWeather : HediffCompProperties
    {
        public string weatherDef = "Fog";

        public HediffCompProperties_ChangeWeather()
        {
            this.compClass = typeof(HediffComp_ChangeWeather);
        }
    }
}
