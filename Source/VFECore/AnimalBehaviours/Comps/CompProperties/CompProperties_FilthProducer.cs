
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_FilthProducer : CompProperties
    {

        //A class to make a creature produce filth around it. A new piece of filth of "filthType" is created
        //every "ticksToCreateFilth", but ONLY "rate" percent of the time

        public string filthType = "";
        public float rate = 0f;
        public int radius = 0;
        public int ticksToCreateFilth = 600;



        public CompProperties_FilthProducer()
        {

            this.compClass = typeof(CompFilthProducer);
        }
    }
}
