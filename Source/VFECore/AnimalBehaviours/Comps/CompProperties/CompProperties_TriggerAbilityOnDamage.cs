
using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_TriggerAbilityOnDamage : CompProperties
    {

        public AbilityDef ability;

        public float minDamageToTrigger = 0;


        public CompProperties_TriggerAbilityOnDamage()
        {
            this.compClass = typeof(CompTriggerAbilityOnDamage);
        }


    }
}