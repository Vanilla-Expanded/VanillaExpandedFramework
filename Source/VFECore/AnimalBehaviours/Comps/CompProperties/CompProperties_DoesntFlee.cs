using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_DoesntFlee : CompProperties
    {

        //This comp class adds and removes animal to a static class, so it can be easily (and laglessly) accessed by a patch to the Pawn.Gizmo 
        //method and used to make draftable animals


        public CompProperties_DoesntFlee()
        {
            this.compClass = typeof(CompDoesntFlee);
        }
    }
}

