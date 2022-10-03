using System;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompProperties_ChangeWeather : CompProperties
    {

        //A comp class that makes this creature change the map's weather when it spawns (Fog by default)

        public int tickInterval = 250;
        public string weatherDef = "Fog";
        public bool isRandomWeathers = false;
        public List<WeatherDef> randomWeathers;


        public CompProperties_ChangeWeather()
        {
            this.compClass = typeof(CompChangeWeather);
        }


    }
}
