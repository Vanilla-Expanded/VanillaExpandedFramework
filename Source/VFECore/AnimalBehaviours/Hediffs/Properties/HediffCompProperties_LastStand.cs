using Verse;

namespace AnimalBehaviours
{
    public class HediffCompProperties_LastStand : HediffCompProperties
    {

        public float finalCoolDownMultiplier = 2f;

        public HediffCompProperties_LastStand()
        {
            this.compClass = typeof(HediffComp_LastStand);
        }
    }
}

