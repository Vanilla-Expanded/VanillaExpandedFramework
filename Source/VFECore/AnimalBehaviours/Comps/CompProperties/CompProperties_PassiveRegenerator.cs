
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_PassiveRegenerator : CompProperties
    {

        //A comp class that makes an animal produce a certain Thought in nearby pawns

        public int radius = 1;
        public int tickInterval = 1000;      
        public bool showEffect = false;
        public bool needsToBeTamed = false;

        public CompProperties_PassiveRegenerator()
        {
            this.compClass = typeof(CompPassiveRegenerator);
        }
    }
}
