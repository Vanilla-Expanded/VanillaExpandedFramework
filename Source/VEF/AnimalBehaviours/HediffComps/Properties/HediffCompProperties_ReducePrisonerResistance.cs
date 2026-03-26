
using Verse;
using RimWorld;


namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_ReducePrisonerResistance : HediffCompProperties
    {
        public float resistancePerTick;

        public int checkingInterval = 250;

        public HediffCompProperties_ReducePrisonerResistance()
        {
            this.compClass = typeof(HediffComp_ReducePrisonerResistance);
        }
    }
}
