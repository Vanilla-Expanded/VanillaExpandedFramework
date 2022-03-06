using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_Regeneration : CompProperties
    {

        //A very simple class that regenerates wounds

        public int rateInTicks = 1000;
        public float healAmount = 0.1f;
        public bool healAll = true;

        public CompProperties_Regeneration()
        {
            this.compClass = typeof(CompRegeneration);
        }


    }
}