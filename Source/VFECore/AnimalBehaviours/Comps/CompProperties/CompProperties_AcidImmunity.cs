
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_AcidImmunity : CompProperties
    {

        //This is just an empty Comp. The new Hediff_AcidBuildup checks if the creature has it, and doesn't apply damage if so

        public CompProperties_AcidImmunity()
        {
            this.compClass = typeof(CompAcidImmunity);
        }


    }
}