using System;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_CauseIncident : HediffCompProperties
    {
        public int checkingInterval = 450000;
        public bool requiresTamed = false;
        public string incidentToCause;

        public HediffCompProperties_CauseIncident()
        {
            this.compClass = typeof(HediffComp_CauseIncident);
        }
    }
}
