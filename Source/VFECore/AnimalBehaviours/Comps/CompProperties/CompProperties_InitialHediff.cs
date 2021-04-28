using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_InitialHediff : CompProperties
    {

        //A comp class that makes animals always spawn with an initial Hediff

        public string hediffname = "";
        public float hediffseverity = 0f;

        //This can be set to apply the hediff to a given body part

        public bool applyToAGivenBodypart = false;
        public BodyPartDef part = null;

        //Possibility to add random hediffs

        public bool addRandomHediffs = false;
        public int numberOfHediffs = 1;

        public CompProperties_InitialHediff()
        {
            this.compClass = typeof(CompInitialHediff);
        }
    }
}
