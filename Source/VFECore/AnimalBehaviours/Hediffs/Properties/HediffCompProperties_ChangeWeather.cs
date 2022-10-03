using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class HediffCompProperties_ChangeWeather : HediffCompProperties
    {
        public int tickInterval = 250;
        public string weatherDef = "Fog";
        public bool isRandomWeathers = false;
        public List<WeatherDef> randomWeathers;

        public HediffCompProperties_ChangeWeather()
        {
            this.compClass = typeof(HediffComp_ChangeWeather);
        }
    }
}
