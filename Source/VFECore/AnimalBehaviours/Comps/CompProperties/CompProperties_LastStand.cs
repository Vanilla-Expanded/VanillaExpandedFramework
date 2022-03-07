using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_LastStand : CompProperties
    {

        //A comp class that makes animals scale their melee cooldowns the more damaged they are

        public float finalCoolDownMultiplier = 2f;
    

        public CompProperties_LastStand()
        {
            this.compClass = typeof(CompLastStand);
        }
    }
}
