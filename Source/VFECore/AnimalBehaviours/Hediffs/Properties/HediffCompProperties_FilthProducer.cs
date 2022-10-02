using Verse;

namespace AnimalBehaviours
{
    public class HediffCompProperties_FilthProducer : HediffCompProperties
    {

        public string filthType = "";
        public float rate = 0f;
        public int radius = 0;
        public int ticksToCreateFilth = 600;

        public HediffCompProperties_FilthProducer()
        {
            this.compClass = typeof(HediffComp_FilthProducer);
        }
    }
}

