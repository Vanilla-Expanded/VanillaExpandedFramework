
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompProperties_CauseIncident : CompProperties
    {

        public int checkingInterval = 450000;
        public bool requiresTamed = false;
        public string incidentToCause;

        public CompProperties_CauseIncident()
        {
            this.compClass = typeof(CompCauseIncident);
        }


    }
}