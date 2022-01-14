using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_NoFilth : CompProperties
    {

        //This comp class adds and removes animal to a static class, so it can be easily (and laglessly) accessed by a Harmony patch 
        //to make animals not drop (or carry) any filth around. Effectively, FilthRate 0


        public CompProperties_NoFilth()
        {
            this.compClass = typeof(CompNoFilth);
        }
    }
}

