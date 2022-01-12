using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompProperties_DigPeriodically : CompProperties
    {

        //A comp class that just makes an animal dig a resource every ticksToDig ticks

        public List<string> customThingToDig = null;
        public List<int> customAmountToDig = null;
        public int ticksToDig = 60000;
        public bool onlyWhenTamed = false;
        //Should items be spawned forbidden?
        public bool spawnForbidden = false;
        //Dig biome rocks. Animal will only dig rocks found on this biome, ignoring customThingToDig
        public bool digBiomeRocks = false;

        public CompProperties_DigPeriodically()
        {
            this.compClass = typeof(CompDigPeriodically);
        }


    }
}