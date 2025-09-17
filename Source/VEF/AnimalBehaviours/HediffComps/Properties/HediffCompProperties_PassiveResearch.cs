using System;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_PassiveResearch : HediffCompProperties
    {

        public int researchPoints = 1;
        public int tickInterval = 6000;
      

        public HediffCompProperties_PassiveResearch()
        {
            this.compClass = typeof(HediffComp_PassiveResearch);
        }
    }
}
