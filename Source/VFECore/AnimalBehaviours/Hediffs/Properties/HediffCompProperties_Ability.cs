
using Verse;
using RimWorld;


namespace AnimalBehaviours
{
    public class HediffCompProperties_Ability : HediffCompProperties
    {
        public AbilityDef ability;

        public int checkingInterval = 6000;

        public HediffCompProperties_Ability()
        {
            this.compClass = typeof(HediffComp_Ability);
        }
    }
}
