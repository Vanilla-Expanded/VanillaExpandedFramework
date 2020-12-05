using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_InitialHediff : CompProperties
    {

        //A comp class that makes animals always spawn with an initial Hediff

        public string hediffname = "";
        public float hediffseverity = 0f;      

        public CompProperties_InitialHediff()
        {
            this.compClass = typeof(CompInitialHediff);
        }
    }
}
