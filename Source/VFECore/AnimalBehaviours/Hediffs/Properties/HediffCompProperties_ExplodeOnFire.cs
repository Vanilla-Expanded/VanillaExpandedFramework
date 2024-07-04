
using Verse;
using RimWorld;


namespace AnimalBehaviours
{
    public class HediffCompProperties_ExplodeOnFire : HediffCompProperties
    {
        public int minFireToExplode;

        public DamageDef damageType;
        public int damageAmount = -1;
        public float radius;
        public int ticksToRecheck;

        public int checkInterval = 300;

        public HediffCompProperties_ExplodeOnFire()
        {
            this.compClass = typeof(HediffComp_ExplodeOnFire);
        }
    }
}
