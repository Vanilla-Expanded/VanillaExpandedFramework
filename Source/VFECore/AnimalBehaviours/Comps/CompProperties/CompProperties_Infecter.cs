
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_Infecter : CompProperties
    {

        //A comp class to make attacks from this animal produce additional infections

        //Note that it won't do anything on its own, it also needs a damage type with
        //damage class DamageWorker_ExtraInfecter

        public int infectionChance = 10;
        public bool worsenExistingInfection = false;
        public float severityToAdd = 0.15f;

        public CompProperties_Infecter()
        {
            this.compClass = typeof(CompInfecter);
        }


    }
}
