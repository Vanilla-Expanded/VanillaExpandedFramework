
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    public class CompProperties_InitialAbility : CompProperties
    {

       
        
        public AbilityDef initialAbility;

        public CompProperties_InitialAbility()
        {
            this.compClass = typeof(CompInitialAbility);
        }


    }
}
