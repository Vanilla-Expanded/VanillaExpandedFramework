using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_ChangeDefIfNotUnique : CompProperties
    {

        //A comp class that will change this animal to a different one if it is not the only one in the map

        public string defToChangeTo = "";

        public CompProperties_ChangeDefIfNotUnique()
        {
            this.compClass = typeof(CompChangeDefIfNotUnique);
        }


    }
}
