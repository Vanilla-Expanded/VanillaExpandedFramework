using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_Draftable : CompProperties
    {

        //This comp class adds and removes animal to a static class, so it can be easily (and laglessly) accessed by a patch to the Pawn.Gizmo 
        //method and used to make draftable animals

        public bool explodable = false;
        public bool rage = false;
        public bool chickenRimPox = false;
        public bool carrymore = false;
        public bool adrenalineburst = false;
        public bool insectclouds = false;
        public bool stampede = false;
        public bool poisonouscloud = false;
        public bool burrowing = false;
        public bool dinostamina = false;
        public bool horror = false;
        public bool mechablast = false;
        public bool keensenses = false;
        public bool catreflexes = false;
        public bool orbitalstrike = false;

        public CompProperties_Draftable()
        {
            this.compClass = typeof(CompDraftable);
        }
    }
}

