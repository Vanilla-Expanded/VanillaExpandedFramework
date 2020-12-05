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

        public CompProperties_DigPeriodically()
        {
            this.compClass = typeof(CompDigPeriodically);
        }


    }
}