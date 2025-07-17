using Verse;

namespace VEF.AnimalBehaviours
{
    public class CompProperties_Draftable : CompProperties
    {

        //This comp class adds and removes animal to a static class, used to make draftable animals

        //If true, adds animals to the non-fleeing mechanic too
        public bool makeNonFleeingToo = false;

        //If true, the animal can equip and fire weapons
        public bool canHandleWeapons = false;

        //Use the TrainableDef VEF_Beastmastery
        public bool conditionalOnTrainability = false;

        public int checkingInterval = 500;

        public CompProperties_Draftable()
        {
            this.compClass = typeof(CompDraftable);
        }
    }
}

