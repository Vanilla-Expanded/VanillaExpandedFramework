
using Verse;
using RimWorld;


namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_ReducePrisonerCertainty : HediffCompProperties
    {
        public float certaintyPerTick;

        public int checkingInterval = 250;

        public HediffCompProperties_ReducePrisonerCertainty()
        {
            this.compClass = typeof(HediffComp_ReducePrisonerCertainty);
        }
    }
}
