
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_ChangeWeather : CompProperties
    {

        //A comp class that makes this creature change the map's weather when it spawns (Fog by default)

        public string weatherDef = "Fog";


        public CompProperties_ChangeWeather()
        {
            this.compClass = typeof(CompChangeWeather);
        }


    }
}
