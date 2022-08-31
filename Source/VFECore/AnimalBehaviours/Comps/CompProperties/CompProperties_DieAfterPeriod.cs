using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_DieAfterPeriod : CompProperties
    {

        //A pretty simple comp class that will kill the animal after timeToDieInTicks ticks.
        //"effect" is just an effectFilth splash around the animal, and a hive spawn sound being played

        public int timeToDieInTicks = 1000;
        public bool justVanish = false;
        public bool effect = false;
        public string effectFilth = "Filth_Blood";

        public CompProperties_DieAfterPeriod()
        {
            this.compClass = typeof(CompDieAfterPeriod);
        }
    }
}
