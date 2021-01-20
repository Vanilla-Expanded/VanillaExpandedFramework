
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_DestroyThisItem : CompProperties
    {
        public string buttonLabel = "";
        public string buttonDesc = "";
        public string buttonIcon = "";

        public string buttonCancelLabel = "";
        public string buttonCancelDesc = "";
        public string buttonCancelIcon = "";

        public CompProperties_DestroyThisItem()
        {
            this.compClass = typeof(CompDestroyThisItem);
        }


    }
}