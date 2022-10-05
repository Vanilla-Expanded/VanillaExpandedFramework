using Verse;

namespace AnimalBehaviours
{
    public class HediffCompProperties_DieAfterPeriod : HediffCompProperties
    {


        public int timeToDieInTicks = 1000;
        public bool justVanish = false;
        public bool effect = false;
        public string effectFilth = "Filth_Blood";
        public string DescriptionLabel = "VEF_TimeToDie";

        public HediffCompProperties_DieAfterPeriod()
        {
            this.compClass = typeof(HediffComp_DieAfterPeriod);
        }
    }
}

