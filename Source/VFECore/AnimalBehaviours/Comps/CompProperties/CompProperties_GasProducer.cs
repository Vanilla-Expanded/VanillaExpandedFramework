
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_GasProducer : CompProperties
    {

        //A laggy comp class that allows an animal to release gases around it

        //It is laggy because too much gas particles in RimWorld are laggy, not by itself

        public string gasType = "";
        public float rate = 0f;
        public int radius = 0;
        public bool generateIfDowned = false;

        public CompProperties_GasProducer()
        {
            this.compClass = typeof(CompGasProducer);
        }
    }
}
