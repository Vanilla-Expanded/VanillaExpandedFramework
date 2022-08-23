
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_HediffWhenFleeing : CompProperties
    {

        public int tickInterval = 60;
        public HediffDef hediffToCause;
        public bool graphicAndSoundEffect = false;
        public bool hediffOnRadius = false;
        public float radius = 3;


        public CompProperties_HediffWhenFleeing()
        {
            this.compClass = typeof(CompHediffWhenFleeing);
        }


    }
}