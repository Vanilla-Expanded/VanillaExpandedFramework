using RimWorld;
using Verse;

namespace VanillaCookingExpanded
{
    public class CompProperties_StackByQuality : CompProperties
    {

        //A comp class to make items only stack if their qualities are the same

        //Used for example in Vanilla Cooking Expanded to avoid cheese of different qualities stacking, and
        //thus "ruining" the higher quality

        public CompProperties_StackByQuality()
        {
            this.compClass = typeof(CompStackByQuality);
        }
    }
}