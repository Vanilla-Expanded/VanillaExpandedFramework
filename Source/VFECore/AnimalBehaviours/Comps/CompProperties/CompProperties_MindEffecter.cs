
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_MindEffecter : CompProperties
    {

        //A comp class that makes an animal cause mental states in nearby pawns

        public int radius = 1;
        public int tickInterval = 1000;
        public string mentalState = "Berserk";
        public bool notOnlyAffectColonists = false;

        public CompProperties_MindEffecter()
        {
            this.compClass = typeof(CompMindEffecter);
        }
    }
}
