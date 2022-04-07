using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_AnimalProductOnCaravan : CompProperties
    {

      
        public int gatheringIntervalTicks = 30000;
        public int resourceAmount = 1;
        public ThingDef resourceDef = null;

       
        public CompProperties_AnimalProductOnCaravan()
        {
            this.compClass = typeof(CompAnimalProductOnCaravan);
        }


    }
}
