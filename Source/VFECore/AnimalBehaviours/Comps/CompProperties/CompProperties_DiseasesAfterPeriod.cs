using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompProperties_DiseasesAfterPeriod : CompProperties
    {

        //A comp class that will apply a random hediff to the animal after timeToDieInTicks ticks.
       
        public int timeToApplyInTicks = 1000;

        public List<HediffDef> hediffsToApply = null;

        public float percentageOfMaxToReapply = 0.8f;
        
        public CompProperties_DiseasesAfterPeriod()
        {
            this.compClass = typeof(CompDiseasesAfterPeriod);
        }
    }
}
