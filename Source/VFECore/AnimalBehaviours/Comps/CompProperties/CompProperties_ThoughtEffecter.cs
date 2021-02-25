
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_ThoughtEffecter : CompProperties
    {

        //A comp class that makes an animal produce a certain Thought in nearby pawns

        public int radius = 1;
        public int tickInterval = 1000;
        public string thoughtDef = "AteWithoutTable";
        public bool showEffect = false;
        public bool needsToBeTamed = false;
        public bool conditionalOnWellBeing = false;
        public string thoughtDefWhenSuffering = "AteWithoutTable";



        public CompProperties_ThoughtEffecter()
        {
            this.compClass = typeof(CompThoughtEffecter);
        }
    }
}


