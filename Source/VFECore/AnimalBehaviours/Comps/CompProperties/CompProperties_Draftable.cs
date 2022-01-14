using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_Draftable : CompProperties
    {

        //This comp class adds and removes animal to a static class, used to make draftable animals

        //If true, adds animals to the non-fleeing mechanic too
        public bool makeNonFleeingToo = false;

        public CompProperties_Draftable()
        {
            this.compClass = typeof(CompDraftable);
        }
    }
}

