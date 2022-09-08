
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompProperties_InitialAbility : CompProperties
    {

        //A comp class that makes animals always spawn with an initial ability

        public AbilityDef initialAbility;

        public CompProperties_InitialAbility()
        {
            this.compClass = typeof(CompInitialAbility);
        }


    }
}
